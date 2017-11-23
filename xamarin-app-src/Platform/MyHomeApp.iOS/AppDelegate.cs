using System;
using System.Linq;

using Foundation;
using UIKit;
using CoreLocation;
using UserNotifications;
using MyHomeApp.Services;
using System.Threading.Tasks;
using Xamarin.Forms;

// Beacon code commented out

namespace MyHomeApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        public CLLocationManager locationManager;

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
                Settings.AlertsAllowed = (settings.AlertSetting == UNNotificationSetting.Enabled);
            });

            locationManager = new CLLocationManager();

            MessagingCenter.Subscribe<string>(this, "StartMonitoringHome", _ => StartMonitoringHome());
            MessagingCenter.Subscribe<string>(this, "StopMonitoringHomee", _ => StopMonitoringHome());
            StartMonitoringHome();

            return base.FinishedLaunching(app, options);
        }

        private void StartMonitoringHome()
        {
            if (Settings.SettingsSet() && Settings.GeographicRegionSet())
            {
                locationManager.AuthorizationChanged += (s, e) =>
                {
                    if (e.Status == CLAuthorizationStatus.AuthorizedAlways)
                    {
                        locationManager.StartMonitoring(HomeRegion());
                    }
                };

                locationManager.RegionEntered += async (s, e) => await OnApprochingHome();
                locationManager.RegionLeft += async (s, e) => await CheckIfGarageDoorIsOpen();
                locationManager.RequestAlwaysAuthorization();

                if (locationManager.MonitoredRegions.Count() == 0)
                {
                    try {
                        locationManager.StartMonitoring(HomeRegion());
                    }
                    catch {}
                }
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

            return new CLCircularRegion(new CLLocationCoordinate2D(Settings.Latitude, Settings.Longitude), 150, "Home")
            {
                NotifyOnEntry = true,
                NotifyOnExit = true
            };
        }

        private async Task OnApprochingHome()
        {
            try
            {
                if (ConnectivityService.Instance.IsConnected && Settings.AlertsAllowed && Settings.SettingsSet())
                {
                    await App.InitializeDevice();

                    try
                    {
                        var response = await App.Device.CallFunctionAsync("onApproaHome");
                        if (response == "1")
                        {
                            if (!Settings.LocationDebugMode) return;

                            TriggerNotification(new UNMutableNotificationContent
                            {
                                Title = "Approaching Home Alert",
                                Body = "You approached home and the front garden lights turned on.",
                                Sound = UNNotificationSound.Default
                            });
                        }
                        else
                        {
                            if (!Settings.LocationDebugMode) return;

                            TriggerNotification(new UNMutableNotificationContent
                            {
                                Title = "Approaching Home Alert",
                                Body = "You approached home, but the front garden lights didn't turn on. They were either already on, or it was not night time.",
                                Sound = UNNotificationSound.Default
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        TriggerNotification(new UNMutableNotificationContent
                        {
                            Title = "Approaching Home Alert Exception",
                            Body = ex.Message,
                            Sound = UNNotificationSound.Default
                        });
                    }
                }
                else
                {
                    TriggerNotification(new UNMutableNotificationContent
                    {
                        Title = "Approaching Home Alert Exception",
                        Body = "No internet or not allowed",
                        Sound = UNNotificationSound.Default
                    });
                }
            }
            catch (Exception ex)
            {
                TriggerNotification(new UNMutableNotificationContent
                {
                    Title = "Approaching Home Alert Exception",
                    Body = ex.Message,
                    Sound = UNNotificationSound.Default
                });
            }
        }

        private async Task CheckIfGarageDoorIsOpen()
        {
            try
            {
                if (ConnectivityService.Instance.IsConnected && Settings.AlertsAllowed && Settings.SettingsSet())
                {
                    await App.InitializeDevice();
                    
                    try
                    {
                        var currentState = await App.Device.GetVariableAsync("currentState");

                        var settings = currentState.Result.ToString().Split('|');
                        if (settings[3] == "OPEN")
                        {
                            TriggerNotification(new UNMutableNotificationContent
                            {
                                Title = "Garage Alert",
                                Body = "You just left home and forget to close the garage door!",
                                Sound = UNNotificationSound.Default
                            });
                        }
                        else
                        {
                            if (!Settings.LocationDebugMode) return;

                            TriggerNotification(new UNMutableNotificationContent
                            {
                                Title = "Garage Alert",
                                Body = "Congrats, you just left home and remembered to close the garage door!",
                                Sound = UNNotificationSound.Default
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        TriggerNotification(new UNMutableNotificationContent
                        {
                            Title = "Garage Alert Final Exception",
                            Body = ex.Message,
                            Sound = UNNotificationSound.Default
                        });
                    }
                }
                else
                {
                    TriggerNotification(new UNMutableNotificationContent
                    {
                        Title = "Garage Alert Exception",
                        Body = "No internet or not allowed",
                        Sound = UNNotificationSound.Default
                    });
                }
            }
            catch (Exception ex)
            {
                TriggerNotification(new UNMutableNotificationContent
                {
                    Title = "Garage Alert Exception",
                    Body = ex.Message,
                    Sound = UNNotificationSound.Default
                });
            }
        }

        private void TriggerNotification(UNMutableNotificationContent content)
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
