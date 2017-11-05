using System;
using Xamarin.Forms;
using System.ComponentModel;
using Acr.UserDialogs;

namespace MyHomeApp.Pages
{
    public class BasePage : ContentPage
    {
        public BasePage()
        {
            ToolbarItems.Add(new ToolbarItem("Refresh", "", () => {
                MessagingCenter.Send<string>("", "PerformSync");
            }));

            ToolbarItems.Add(new ToolbarItem("Logout", "", async () => {
                if (await UserDialogs.Instance.ConfirmAsync("Really log out?"))
                {
                    MessagingCenter.Send<string>("", "StopMonitoringHome");
                    Settings.ClearSettings();
                    Application.Current.MainPage = ((App)Application.Current).BuildSetupPage();
                }
            }));
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var basePageModel = this.BindingContext as FreshMvvm.FreshBasePageModel;
			if (basePageModel != null)
			{
				//if (basePageModel.IsModalAndHasPreviousNavigationStack())
				//{
				//	if (ToolbarItems.Count < 2)
				//	{
				//		var closeModal = new ToolbarItem("Close Modal", "", () => {
				//			basePageModel.CoreMethods.PopModalNavigationService();
				//		});

				//		ToolbarItems.Add(closeModal);
				//	}
				//}
			}
		}
    }
}
