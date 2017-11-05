using FreshMvvm;
using Xamarin.Forms;
using MyHomeApp.PageModels;

namespace MyHomeApp
{
    public partial class App : Application
    {
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
        }

        protected override void OnResume()
        {
            if (Settings.SettingsSet())
            {
                MessagingCenter.Send<string>("", "PerformSync");
            }
        }
    }
}
