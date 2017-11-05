using Plugin.SecureStorage;
using Plugin.SecureStorage.Abstractions;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace MyHomeApp
{
    public static class Settings
    {
        private static ISettings AppSettings => CrossSettings.Current;

        private static ISecureStorage AppSecureStorage => CrossSecureStorage.Current;

        public static string ParticleUrl { get; } = "https://api.particle.io";

        public static string DeviceId
        {
            get => AppSettings.GetValueOrDefault(nameof(DeviceId), default(string));
            set => AppSettings.AddOrUpdateValue(nameof(DeviceId), value);
        }

        public static string AccessToken
        {
            get => AppSecureStorage.GetValue(nameof(AccessToken), default(string));
            set => AppSecureStorage.SetValue(nameof(AccessToken), value);
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

        public static bool SettingsSet()
        {
            return DeviceId != default(string) && AccessToken != default(string);
        }

        public static void ClearSettings()
        {
            AppSettings.Clear();
            CrossSecureStorage.Current.DeleteKey(nameof(AccessToken));
        }
    }
}
