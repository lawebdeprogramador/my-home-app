using System;
using System.Collections.Generic;
using MyHomeApp.PageModels;
using MyHomeApp.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MyHomeApp.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class MainPage : BasePage
	{
	    public MainPage()
	    {
	        InitializeComponent();
	    }

	    private void Handle_FrontGardenLightsToggle(object sender, ToggledEventArgs e)
	    {
            if (!App.SettingCurrentState)
	            MessagingCenter.Send<string>("", "ToggleFrontGardenLights");
	    }

	    private void Handle_SunsetModeToggle(object sender, ToggledEventArgs e)
        {
            if (!App.SettingCurrentState)
	            MessagingCenter.Send<string>("", "ToggleSunsetMode");
	    }

	    private void Handle_GarageDoorButton(object sender, ToggledEventArgs e)
        {
            if (!App.SettingCurrentState)
	            MessagingCenter.Send<string>("", "PressGarageDoorButton");
	    }
	}
}