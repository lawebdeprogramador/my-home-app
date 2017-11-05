using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using CoreLocation;
using UserNotifications;
using MyHomeApp.Services;
using Refit;
using Polly;
using System.Threading.Tasks;
using Xamarin.Forms;
using Acr.UserDialogs;

// Beacon code commented out, using IFTTT atm

namespace MyHomeApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public CLLocationManager locationManager;
        public bool AlertsAllowed;

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();
            LoadApplication(new App());

            UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) => {
                // Handle approval
            });

            // Get current notification settings
            UNUserNotificationCenter.Current.GetNotificationSettings((settings) => {
                AlertsAllowed = (settings.AlertSetting == UNNotificationSetting.Enabled);
            });

            locationManager = new CLLocationManager
            {
                ActivityType = CLActivityType.AutomotiveNavigation
            };

            MessagingCenter.Subscribe<string>(this, "StartMonitoringHome", _ => StartMonitoringHome(app));
            MessagingCenter.Subscribe<string>(this, "StopMonitoringHomee", _ => StopMonitoringHome());
            StartMonitoringHome(app);

            return base.FinishedLaunching(app, options);
        }

        private void StartMonitoringHome(UIApplication app)
        {
            if (Settings.GeographicRegionSet())
            {
                locationManager.AuthorizationChanged += (s, e) =>
                {
                    if (e.Status == CLAuthorizationStatus.AuthorizedAlways)
                    {
                        locationManager.StartMonitoring(HomeRegion());
                    }
                };

                locationManager.RegionEntered += async (s, e) => await OnApprochingHome(app);
                locationManager.RegionLeft += async (s, e) => await CheckIfGarageDoorIsOpen(app);

                locationManager.RequestAlwaysAuthorization();
            }
        }

        private void StopMonitoringHome()
        {
            locationManager.StopMonitoring(HomeRegion());
        }

        private CLCircularRegion HomeRegion()
        {
            // For BLE beacon use
            //var region = new CLBeaconRegion(new NSUuid(Settings.BeaconUuid), ushort.Parse(Settings.BeaconMajor), ushort.Parse(Settings.BeaconMinor), "Garage")
            //{
            //    NotifyOnEntry = true,
            //    NotifyOnExit = true
            //};

            return new CLCircularRegion(new CLLocationCoordinate2D(Settings.Latitude, Settings.Longitude), 200, "Home")
            {
                NotifyOnEntry = true,
                NotifyOnExit = true
            };
        }

        private async Task OnApprochingHome(UIApplication app)
        {
            if (ConnectivityService.Instance.IsConnected)
            {
                var apiCall = RestService.For<IParticleApi>(Settings.ParticleUrl);
                var data = new Dictionary<string, object> {
                    {"args", ""}
                };
                var response = await ResilientCall.ExecuteWithRetry(
                        async () =>
                            await apiCall.CallFunction("onApproaHome", data, Settings.DeviceId, Settings.AccessToken)
                    ).ConfigureAwait(false);

                if (response.Outcome == OutcomeType.Successful)
                {
                    if (response.Result.ReturnValue == 0)
                    {
                        TriggerNotification(new UNMutableNotificationContent
                        {
                            Title = "Approaching Home Alert",
                            Body = "You approached home, but the front garden lights didn't turn on. They were either already on, or it was not night time.",
                            Sound = UNNotificationSound.Default
                        }, app);
                    }
                    else
                    {
                        TriggerNotification(new UNMutableNotificationContent
                        {
                            Title = "Approaching Home Alert",
                            Body = "You approached home and the front garden lights turned on.",
                            Sound = UNNotificationSound.Default
                        }, app);
                    }
                }
            }
        }

        private async Task CheckIfGarageDoorIsOpen(UIApplication app)
        {
            if (ConnectivityService.Instance.IsConnected && AlertsAllowed)
            {
                var apiCall = RestService.For<IParticleApi>(Settings.ParticleUrl);
                var response = await ResilientCall.ExecuteWithRetry(
                        async () =>
                            await apiCall.GetVariable("currentState", Settings.DeviceId, Settings.AccessToken)
                    ).ConfigureAwait(false);

                if (response.Outcome == OutcomeType.Successful)
                {
                    var settings = response.Result.Result.Split('|');

                    if (settings[3] == "OPEN")
                    {
                        TriggerNotification(new UNMutableNotificationContent
                        {
                            Title = "Garage Alert",
                            Body = "Did you just leave home and forget to close the garage door?",
                            Sound = UNNotificationSound.Default
                        }, app);
                    }
                    else
                    {
                        TriggerNotification(new UNMutableNotificationContent
                        {
                            Title = "Garage Alert",
                            Body = "Congrats, you just left home and remembered to close the garage door!",
                            Sound = UNNotificationSound.Default
                        }, app);
                    }
                }
            }
        }

        private void TriggerNotification(UNMutableNotificationContent content, UIApplication app)
        {
            if (app.ApplicationState == UIApplicationState.Active)
            {
                UserDialogs.Instance.Alert(content.Body, content.Title, "OK");
            }
            else
            {
                // Deliver the notification in 1 second
                var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
                var requestID = Guid.NewGuid().ToString("N");
                var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);
                var center = UNUserNotificationCenter.Current;
                center.AddNotificationRequest(request, e =>
                {
                    if (e != null)
                    {
                        // error
                    }
                });
            }
        }
    }
}
