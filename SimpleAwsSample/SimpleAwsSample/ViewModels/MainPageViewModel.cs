using Amazon.Lambda.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using SimpleAwsSample.Models;
using SimpleAwsSample.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using static Amazon.CognitoIdentity.CognitoAWSCredentials;

namespace SimpleAwsSample.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        #region Properties & fields

        private readonly ICustomSsoService _ssoService;
        private readonly IAwsCognitoService _awsCognitoService;
        private readonly IAwsLambdaService _awsLambdaService;
        private readonly IEventAggregator _eventAggregator;

        public DelegateCommand LoginTappedCommand { get; set; }
        public DelegateCommand ClearTappedCommand { get; set; }
        public DelegateCommand ToUpperTappedCommand { get; set; }

        private string _username;
        public string Username
        {
            get { return _username; }
            set { SetProperty(ref _username, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        private string _statusText;
        public string StatusText
        {
            get { return _statusText; }
            set { SetProperty(ref _statusText, value); }
        }

        #endregion Properties & fields

        public MainPageViewModel(INavigationService navigationService, ICustomSsoService ssoService,
            IAwsCognitoService awsCognitoService, IAwsLambdaService awsLambdaService, IEventAggregator eventAggregator)
            : base(navigationService)
        {
            _ssoService = ssoService;
            _awsCognitoService = awsCognitoService;
            _awsLambdaService = awsLambdaService;
            _eventAggregator = eventAggregator;

            Username = "awesomeuser@fake.com";
            Password = "1234";
            Title = "Simple Cognito-Lambda";

            LoginTappedCommand = new DelegateCommand(OnLoginTapped);
            ClearTappedCommand = new DelegateCommand(ClearAllStatusText);
            ToUpperTappedCommand = new DelegateCommand(OnToUpperTappedExecuteAwsLambdaFunction);

            _eventAggregator.GetEvent<AddTextToUiOutput>().Subscribe(AddTextToStatusTextLabel);
        }

        #region SSO and AWS Logic

        private async void OnLoginTapped()
        {
            ClearAllStatusText();

            var validSsoUser = LoginToCustomSso(Username, Password);
            await LoginToAwsCognito();
        }


        private CustomSsoUser LoginToCustomSso(string userName, string password)
        {
            CustomSsoUser ssoUser = _ssoService.LoginToCustomSso(userName, password);
            AddTextToStatusTextLabel($"#########{System.Environment.NewLine}User has authenticated with custom SSO:{System.Environment.NewLine}==> Sso user id: {ssoUser.GuidId.ToString()}{System.Environment.NewLine}==> Sso user token: {ssoUser.Token}");
            _awsCognitoService.SsoUser = ssoUser;
            return ssoUser;
        }

        private async Task LoginToAwsCognito()
        {
            try
            {
                IdentityState awsIdentityState = await _awsCognitoService.RefreshIdentityAsync();
            }
            catch (Exception ex)
            {
                AddTextToStatusTextLabel($"{System.Environment.NewLine}EXCEPTION:  {ex.GetType().FullName}:  {ex.Message}");
            }
        }

        private async void OnToUpperTappedExecuteAwsLambdaFunction()
        {
            var request = new InvokeRequest
            {
                FunctionName = AwsConstants.LambdaToUpperFunctionArn,
                InvocationType = "RequestResponse",
                LogType = "Tail",
                Payload = "true"
            };

            AddTextToStatusTextLabel($"Calling aws ToUpper lambda with payload: {request.Payload}");

            try
            {
                var awsResponse = await _awsLambdaService.InvokeAsync(request, _awsCognitoService.AwsCredentials, AwsConstants.AppRegionEndpoint);
                var reader = new StreamReader(awsResponse.Payload);
                string payload = reader.ReadToEnd();
                AddTextToStatusTextLabel($"#########{System.Environment.NewLine}Successfully called lambda! Result:  {payload}");
            }
            catch (Exception ex)
            {
                AddTextToStatusTextLabel($"#########{System.Environment.NewLine}Call to lambda failed with {ex.GetType().Name}:  {ex.Message}");
            }
        }

        #endregion SSO and AWS Logic

        #region Dinky methods that are relatively trivial

        private void ClearAllStatusText()
        {
            StatusText = string.Empty;
        }

        private void AddTextToStatusTextLabel(string newMessage)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(AddTextToStatusTextLabel)}:  {newMessage}");
            StatusText += $"{newMessage}{System.Environment.NewLine}{System.Environment.NewLine}";
        }

        #endregion Dinky methods that are relatively trivial
    }
}
