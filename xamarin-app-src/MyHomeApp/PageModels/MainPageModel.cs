using System;
using Xamarin.Forms;
using PropertyChanged;
using FreshMvvm;
using System.Threading.Tasks;
using Acr.UserDialogs;
using MyHomeApp.Services;
using System.ComponentModel;
using Particle;
using Particle.Helpers;
using MyHomeApp.Helpers;

namespace MyHomeApp.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class MainPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        public static readonly AsyncLock SyncLock = new AsyncLock();
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

            if (App.CurrentStateSubscribedId == Guid.Empty)
            {
                App.CurrentStateSubscribedId = await ParticleCloud.SharedInstance.SubscribeToMyDevicesEventsWithPrefixAsync(
                    "CurrentState",
                    Settings.DeviceId,
                    async (object s, ParticleEventArgs pe) => await SetCurrentStateEvent(s, pe)
                );
            }
        }

        private async Task SetCurrentStateEvent(object s, ParticleEventArgs pe)
        {
            await SetCurrentState(pe.EventData.Data);
        }

        public async Task ToggleFrontGardenLights()
        {
            if (!await HasInternetConnection())
                return;
            
            try
            {
                FrontGardenLightsToggleEnabled = false;
                var response = await App.Device.CallFunctionAsync("toggleFront");
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"An error has occured. Please try again. {ex.Message}");
            }
        }

        public async Task ToggleSunsetMode()
        {
            if (!await HasInternetConnection())
                return;
            
            try
            {
                SunsetModeActiveToggleEnabled = false;
                var response = await App.Device.CallFunctionAsync("toggleSunset");
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"An error has occured. Please try again. {ex.Message}");
            }
        }

        public async Task PressGarageDoorButton()
        {
            if (!await HasInternetConnection())
                return;

            if (PressingGarageDoorButton)
            {
                GarageDoorButton = false;
                return;
            }

            try
            {
                GarageDoorOperating = false;
                PressingGarageDoorButton = true;

                var response = await App.Device.CallFunctionAsync("garageButton");

                GarageDoorOperating = true;
                GarageDoorButton = false;

                await Task.Delay(500); // The toggle handler will fire causing a loop, so pause for a bit to return out of PressGarageDoorButton() method

                PressingGarageDoorButton = false;
                await ProgressOfGarageDoor();
            }
            catch (Exception ex)
            {
                await UserDialogs.Instance.AlertAsync($"Failed to press garage door button. {ex.Message}");
            }
        }

        private async Task ProgressOfGarageDoor()
        {
            var defaultStatus = GarageDoorStatusText;

            for (int i = GarageDoorOperationTimeInSeconds; i > 0; i--)
            {
                if (GarageDoorOperating)
                {
                    GarageDoorStatusText = $"Operating: {i} seconds remaining";
                    await Task.Delay(500);
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

            await App.InitializeDevice();
            
            var success = await PerformSync();

            UserDialogs.Instance.HideLoading();

            return success;
        }

        private async Task<bool> PerformSync()
        {
            using (await SyncLock.LockAsync())
            {
                if (!ConnectivityService.Instance.IsConnected)
                {
                    await UserDialogs.Instance.AlertAsync("No internet connection...");
                    return false;
                }

                Syncing = true;
                var success = false;

                try
                {
                    var currentState = await App.Device.GetVariableAsync("currentState");
                    await SetCurrentState(currentState.Result.ToString());

                    success = true;
                }
                catch (Exception ex)
                {
                    await UserDialogs.Instance.AlertAsync($"An error has occured. Please try again. {ex.Message}");
                }

                Syncing = false;

                return success;
            }
        }

        private async Task SetCurrentState(string currentState)
        {
            App.SettingCurrentState = true;

            var settings = currentState.Split('|');

            SunsetModeActiveToggleEnabled = true;
            FrontGardenLightsToggleEnabled = true;

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

            GarageDoorOperating = false;
            if (settings[3] == "CLOSED")
                GarageDoorOpen = false;
            else if (settings[3] == "OPEN")
                GarageDoorOpen = true;

            await Task.Delay(500);
            App.SettingCurrentState = false;
        }

        private async Task<bool> HasInternetConnection()
        {
            if (ConnectivityService.Instance.IsConnected)
                return true;
            
            await UserDialogs.Instance.AlertAsync("No internet connection...");
            return false;
        }
    }
}
