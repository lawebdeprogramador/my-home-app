using FreshMvvm;
using Xamarin.Forms;
using MyHomeApp.PageModels;
using Xamarin.Forms.Xaml;
using System;
using Particle;
using System.Threading.Tasks;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace MyHomeApp
{
    public partial class App : Application
    {
        public static bool SettingCurrentState;
        public static Guid CurrentStateSubscribedId = Guid.Empty;
        public static ParticleDevice Device;
        private static DateTime LastSleepTime = DateTime.Now;

        public App()
        {
            InitializeComponent();

            MainPage = Settings.SettingsSet() 
                               ? BuildMainPage() 
                               : BuildSetupPage();
        }

        public Page BuildSetupPage()
        {
            return FreshPageModelResolver.ResolvePageModel<SetupPageModel>();
        }

        public Page BuildMainPage()
        {
            var page = FreshPageModelResolver.ResolvePageModel<MainPageModel>();
            return new FreshNavigationContainer(page);
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
            LastSleepTime = DateTime.Now;
        }

        protected override void OnResume()
        {
            if (DateTime.Now.Subtract(LastSleepTime).TotalSeconds > 60 && Settings.SettingsSet())
            {
                MessagingCenter.Send<string>("", "PerformSync");
                MessagingCenter.Send<string>("", "SubscribeToParticleEvents");
            }
        }

        public static async Task InitializeDevice()
        {
            if (Device == null)
                Device = await ParticleCloud.SharedInstance.GetDeviceAsync(Settings.DeviceId);
        }
    }
}
