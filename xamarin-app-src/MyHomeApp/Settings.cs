using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MyHomeApp
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        public static string ParticleUrl { get; } = "https://api.particle.io";

        public static string DeviceId
        {
            get => AppSettings.GetValueOrDefault(nameof(DeviceId), default(string));
            set => AppSettings.AddOrUpdateValue(nameof(DeviceId), value);
        }

        public static string AccessToken
        {
            get => AppSettings.GetValueOrDefault(nameof(AccessToken), default(string));
            set => AppSettings.AddOrUpdateValue(nameof(AccessToken), value);
        }

        public static string BeaconUuid
        {
            get => AppSettings.GetValueOrDefault(nameof(BeaconUuid), default(string));
            set => AppSettings.AddOrUpdateValue(nameof(BeaconUuid), value);
        }

        public static string BeaconMajor
        {
            get => AppSettings.GetValueOrDefault(nameof(BeaconMajor), default(string));
            set => AppSettings.AddOrUpdateValue(nameof(BeaconMajor), value);
        }

        public static string BeaconMinor
        {
            get => AppSettings.GetValueOrDefault(nameof(BeaconMinor), default(string));
            set => AppSettings.AddOrUpdateValue(nameof(BeaconMinor), value);
        }

        public static double Latitude
        {
            get => AppSettings.GetValueOrDefault(nameof(Latitude), default(double));
            set => AppSettings.AddOrUpdateValue(nameof(Latitude), value);
        }

        public static double Longitude
        {
            get => AppSettings.GetValueOrDefault(nameof(Longitude), default(double));
            set => AppSettings.AddOrUpdateValue(nameof(Longitude), value);
        }

        public static bool AlertsAllowed
        {
            get => AppSettings.GetValueOrDefault(nameof(AlertsAllowed), true);
            set => AppSettings.AddOrUpdateValue(nameof(AlertsAllowed), value);
        }

        public static bool SettingsSet()
        {
            return DeviceId != default(string) && AccessToken != default(string);
        }

        public static bool BeaconSet()
        {
            return BeaconUuid != default(string) && BeaconMajor != default(string) && BeaconMinor != default(string);
        }

        public static bool GeographicRegionSet()
        {
            return Latitude != default(double) && Longitude != default(double);
        }

        public static void ClearSettings()
        {
            AppSettings.Clear();
        }
    }
}
