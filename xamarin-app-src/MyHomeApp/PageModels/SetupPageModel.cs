using Xamarin.Forms;
using FreshMvvm;
using PropertyChanged;
using Acr.UserDialogs;
using System.Threading;
using System.Threading.Tasks;

namespace MyHomeApp.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class SetupPageModel : FreshBasePageModel
    {
        public string DeviceId { get; set; }
        public string AccessToken { get; set; }
        public string BeaconUuid { get; set; }
        public string BeaconMajor { get; set; }
        public string BeaconMinor { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public SetupPageModel()
        {
            
        }

        public Command SaveCommand
        {
            get => new Command(async () =>
            {
                if (string.IsNullOrWhiteSpace(DeviceId) || string.IsNullOrWhiteSpace(AccessToken))
                {
                    await UserDialogs.Instance.AlertAsync("Device ID and Access Token are required.");
                }
                else
                {
                    Settings.DeviceId = DeviceId;
                    Settings.AccessToken = AccessToken;
                    //Settings.BeaconUuid = BeaconUuid;
                    //Settings.BeaconMajor = BeaconMajor;
                    //Settings.BeaconMinor = BeaconMinor;
                    Settings.Latitude = Latitude;
                    Settings.Longitude = Longitude;

                    MessagingCenter.Send<string>("", "StartMonitoringHome");
                    Application.Current.MainPage = ((App)Application.Current).BuildMainPage();
                }
            });
        }
    }
}
