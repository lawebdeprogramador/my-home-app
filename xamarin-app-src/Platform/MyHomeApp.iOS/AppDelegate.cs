using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
//using CoreLocation;
//using UserNotifications;

// Beacon code commented out, using IFTTT atm

namespace MyHomeApp.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //public CLLocationManager locationManager;
        //public bool AlertsAllowed;

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

            //UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) => {
            //    // Handle approval
            //});

            //// Get current notification settings
            //UNUserNotificationCenter.Current.GetNotificationSettings((settings) => {
            //    AlertsAllowed = (settings.AlertSetting == UNNotificationSetting.Enabled);
            //});

            //if (Settings.SettingsSet())
            //{
            //    locationManager = new CLLocationManager();

            //    var region = new CLBeaconRegion(new NSUuid(Settings.BeaconUuid), ushort.Parse(Settings.BeaconMajor), ushort.Parse(Settings.BeaconMinor), "Garage")
            //    {
            //        NotifyOnEntry = true,
            //        NotifyOnExit = true
            //    };

            //    locationManager.AuthorizationChanged += (s, e) =>
            //    {
            //        if (e.Status == CLAuthorizationStatus.AuthorizedAlways)
            //            locationManager.StartMonitoring(region);
            //    };

            //    locationManager.RegionEntered += (s, e) => SendNotification("RegionEntered");
            //    locationManager.RegionLeft += (s, e) => SendNotification("RegionLeft");

            //    locationManager.RequestAlwaysAuthorization();
            //}

            return base.FinishedLaunching(app, options);
        }

        //private void SendNotification(string alertType)
        //{
        //    if (!AlertsAllowed) return;

        //    var content = new UNMutableNotificationContent
        //    {
        //        Title = alertType + " Alert",
        //        Subtitle = "Notification Subtitle",
        //        Body = "This is the message body of the notification.",
        //        Sound = UNNotificationSound.Default
        //    };

        //    // Deliver the notification in five seconds.
        //    var trigger = UNTimeIntervalNotificationTrigger.CreateTrigger(1, false);
        //    var requestID = Guid.NewGuid().ToString("N");
        //    var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);
        //    var center = UNUserNotificationCenter.Current;
        //    center.AddNotificationRequest(request, e => {
        //        if (e != null)
        //        {
        //            // error
        //            var error = e;
        //        }
        //    });
        //}
    }
}
