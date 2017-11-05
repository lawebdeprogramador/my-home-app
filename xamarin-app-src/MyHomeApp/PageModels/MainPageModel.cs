using System;
using Xamarin.Forms;
using PropertyChanged;
using FreshMvvm;
using Refit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Acr.UserDialogs;
using MyHomeApp.Services;
using Polly;
using System.ComponentModel;

namespace MyHomeApp.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        public bool Syncing { get; set; }
        public bool SunsetModeActive { get; set; }
        public bool SunsetModeActiveToggleEnabled { get; set; } = true;
        public bool AreFrontGardenLightsOn { get; set; }
        public bool FrontGardenLightsToggleEnabled { get; set; } = true;
        public string SunsetLightsOffAt { get; set; }
        public string GarageDoorStatusText { get; set; } = "Currently open?";
        public bool GarageDoorOpen { get; set; }
        public bool GarageDoorButton { get; set; }
        public bool PressingGarageDoorButton { get; set; }
        public bool GarageDoorOperating { get; set; }
        public int GarageDoorOperationTimeInSeconds { get; } = 14;

        public MainPageModel()
        {
            MessagingCenter.Subscribe<string>(this, "PerformSync", async _ => await PerformSyncAndShowLoader());
            MessagingCenter.Subscribe<string>(this, "ToggleFrontGardenLights", async _ => await ToggleFrontGardenLights());
            MessagingCenter.Subscribe<string>(this, "ToggleSunsetMode", async _ => await ToggleSunsetMode());
            MessagingCenter.Subscribe<string>(this, "PressGarageDoorButton", async _ => await PressGarageDoorButton());
        }

        public override void Init(object initData)
        {
        }

        protected override async void ViewIsAppearing(object sender, EventArgs e)
        {
            base.ViewIsAppearing(sender, e);

            var success = await PerformSyncAndShowLoader();

            if (!success)
            {
                await UserDialogs.Instance.AlertAsync("An error has occured. Please refresh");
            }
        }

        public async Task ToggleFrontGardenLights()
        {
            if (Syncing) return;

            FrontGardenLightsToggleEnabled = false;

            var apiCall = RestService.For<IParticleApi>(Settings.ParticleUrl);
            var data = new Dictionary<string, object> {
                {"args", ""}
            };
            var response = await ResilientCall.ExecuteWithRetry(
                    async () =>
                        await apiCall.CallFunction("toggleFront", data, Settings.DeviceId, Settings.AccessToken)
                ).ConfigureAwait(false);

            if (response.Outcome == OutcomeType.Successful)
            {
                await PerformSync();
            }
            else
            {
                await UserDialogs.Instance.AlertAsync("An error has occured. Please try again.");
            }

            FrontGardenLightsToggleEnabled = true;
        }

        public async Task ToggleSunsetMode()
        {
            if (Syncing) return;

            if (!ConnectivityService.Instance.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync("No internet connection...");
                return;
            }

            SunsetModeActiveToggleEnabled = false;

            var apiCall = RestService.For<IParticleApi>(Settings.ParticleUrl);
            var data = new Dictionary<string, object> {
                {"args", ""}
            };
            var response = await ResilientCall.ExecuteWithRetry(
                    async () =>
                        await apiCall.CallFunction("toggleSunset", data, Settings.DeviceId, Settings.AccessToken)
                ).ConfigureAwait(false);

            if (response.Outcome == OutcomeType.Successful)
            {
                await PerformSync();
            }
            else
            {
                await UserDialogs.Instance.AlertAsync("An error has occured. Please try again.");
            }

            SunsetModeActiveToggleEnabled = true;
        }

        public async Task PressGarageDoorButton()
        {
            if (!ConnectivityService.Instance.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync("No internet connection...");
                return;
            }

            if (PressingGarageDoorButton)
            {
                GarageDoorButton = false;
                return;
            }

            GarageDoorOperating = false;

            PressingGarageDoorButton = true;

            var apiCall = RestService.For<IParticleApi>(Settings.ParticleUrl);
            var data = new Dictionary<string, object> {
                {"args", ""}
            };
            var response = await ResilientCall.ExecuteWithRetry(
                    async () =>
                        await apiCall.CallFunction("garageButton", data, Settings.DeviceId, Settings.AccessToken)
                ).ConfigureAwait(false);

            if (response.Outcome != OutcomeType.Successful)
            {
                await UserDialogs.Instance.AlertAsync("Failed to press garage door button");
            }

            GarageDoorOperating = true;
            GarageDoorButton = false;

            await Task.Delay(1000); // The toggle handler will fire causing a loop, so pause for a bit to return out of PressGarageDoorButton() method

            PressingGarageDoorButton = false;

            await ProgressOfGarageDoor();
        }

        private async Task ProgressOfGarageDoor()
        {
            var defaultStatus = GarageDoorStatusText;

            for (int i = GarageDoorOperationTimeInSeconds; i > 0; i--)
            {
                if (GarageDoorOperating)
                {
                    GarageDoorStatusText = $"Operating: {i} seconds remaining";
                    await Task.Delay(1000);
                }
                else
                {
                    GarageDoorStatusText = defaultStatus;
                    return;
                }
            }

            GarageDoorStatusText = defaultStatus;

            await PerformSyncAndShowLoader();
        }

        private async Task<bool> PerformSyncAndShowLoader()
        {
            GarageDoorOperating = false;

            UserDialogs.Instance.ShowLoading("Loading home state...");

            var success = await PerformSync();

            UserDialogs.Instance.HideLoading();

            return success;
        }

        private async Task<bool> PerformSync()
        {
            if (!ConnectivityService.Instance.IsConnected)
            {
                await UserDialogs.Instance.AlertAsync("No internet connection...");
                return false;
            }

            Syncing = true;

            var apiCall = RestService.For<IParticleApi>(Settings.ParticleUrl);
            var response = await ResilientCall.ExecuteWithRetry(
                    async () =>
                        await apiCall.GetVariable("currentState", Settings.DeviceId, Settings.AccessToken)
                ).ConfigureAwait(false);

            var success = false;
            if (response.Outcome == OutcomeType.Successful)
            {
                var settings = response.Result.Result.Split('|');
                SunsetModeActive = settings[0] == "1";
                AreFrontGardenLightsOn = settings[1] == "1";

                var hour = int.Parse(settings[2].Split(':')[0]);
                var min = int.Parse(settings[2].Split(':')[1]);
                if (min == 0)
                {
                    SunsetLightsOffAt = "n/a";
                }
                else
                {
                    DateTime dt = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, hour, min, 00);
                    SunsetLightsOffAt = $"{dt:h:mm tt}";
                }

                if (settings[3] == "CLOSED")
                    GarageDoorOpen = false;
                else if (settings[3] == "OPEN")
                    GarageDoorOpen = true;

                await Task.Delay(500); // The toggle handler will fire causing a loop, so pause for a bit to return out of ToggleFrontGardenLights() method
                success = true;
            }

            Syncing = false;

            return success;
        }
    }
}
