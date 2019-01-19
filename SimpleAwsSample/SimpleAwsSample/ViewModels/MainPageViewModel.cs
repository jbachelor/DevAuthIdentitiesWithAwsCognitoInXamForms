using Amazon.Lambda.Model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SimpleAwsSample.Models;
using SimpleAwsSample.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleAwsSample.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly ICustomSsoService _ssoService;
        private readonly IAwsCognitoService _awsCognitoService;
        private readonly IAwsLambdaService _awsLambdaService;

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

        public MainPageViewModel(INavigationService navigationService, ICustomSsoService ssoService,
            IAwsCognitoService awsCognitoService, IAwsLambdaService awsLambdaService)
            : base(navigationService)
        {
            _ssoService = ssoService;
            _awsCognitoService = awsCognitoService;
            _awsLambdaService = awsLambdaService;

            Title = "Simple Cognito";

            LoginTappedCommand = new DelegateCommand(OnLoginTapped);
            ClearTappedCommand = new DelegateCommand(OnClearTapped);
            ToUpperTappedCommand = new DelegateCommand(OnToUpperTapped);
        }

        private void OnClearTapped()
        {
            StatusText = string.Empty;
        }

        private async void OnLoginTapped()
        {
            CustomSsoUser ssoUser = _ssoService.LoginToCustomSso(Username, Password);
            AddTextToStatusTextLabel($"#########{System.Environment.NewLine}User has authenticated with custom SSO:{System.Environment.NewLine}==> Sso user id: {ssoUser.GuidId.ToString()}{System.Environment.NewLine}==> Sso user token: {ssoUser.Token}");
            var cognitoIdentity = await _awsCognitoService.LoginToAwsWithDeveloperAuthenticatedSsoUserAsync(ssoUser);
            AddTextToStatusTextLabel($"#########{System.Environment.NewLine}User now has an AWS Cognito identity with id: {cognitoIdentity.IdentityId}");
        }

        private async void OnToUpperTapped()
        {
            var request = new InvokeRequest
            {
                FunctionName = AwsConstants.LambdaToUpperFunctionArn,
                InvocationType = "RequestResponse",
                LogType = "Tail",
                Payload = StatusText
            };

            var awsResponse = await _awsLambdaService.InvokeAsync(request, _awsCognitoService.Credentials, AwsConstants.AppRegionEndpoint);
            StatusText = awsResponse.Payload.ToString();
        }

        private void AddTextToStatusTextLabel(string newMessage)
        {
            StatusText += $"{newMessage}{System.Environment.NewLine}{System.Environment.NewLine}";
        }
    }
}
