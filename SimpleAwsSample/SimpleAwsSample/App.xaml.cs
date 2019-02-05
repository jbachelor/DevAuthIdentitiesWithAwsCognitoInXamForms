using System.Diagnostics;
using Amazon;
using Prism;
using Prism.Ioc;
using SimpleAwsSample.Models;
using SimpleAwsSample.Services;
using SimpleAwsSample.ViewModels;
using SimpleAwsSample.Views;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace SimpleAwsSample
{
    public partial class App
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        protected override async void OnInitialized()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnInitialized)}");

            InitializeComponent();
            await NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(RegisterTypes)}");

            PreventExceptionInBaseConstructorOfCognitoAWSCredentials();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();

            containerRegistry.RegisterSingleton<ICustomSsoService, CustomSsoService>();
            containerRegistry.RegisterSingleton<IAwsCognitoService, AwsCognitoService>();
            containerRegistry.RegisterSingleton<IAwsLambdaService, AwsLambdaService>();

        }

        private void PreventExceptionInBaseConstructorOfCognitoAWSCredentials()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(PreventExceptionInBaseConstructorOfCognitoAWSCredentials)}");

            // Line below avoids an exception when base constructor for CognitoAWSCredentials is called:
            AWSConfigs.RegionEndpoint = AwsConstants.AppRegionEndpoint;
        }
    }
}
