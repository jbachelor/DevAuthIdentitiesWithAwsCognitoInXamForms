using Amazon.Lambda.Model;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using SimpleAwsSample.Models;
using SimpleAwsSample.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SimpleAwsSample.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly ICustomSsoService _ssoService;
        private readonly IAwsCognitoService _awsCognitoService;
        private readonly IAwsLambdaService _awsLambdaService;

        private Random _random = new Random();
        private List<Color> _textColorChoices = new List<Color>
        {
            Color.Navy, Color.Aqua, Color.Red, Color.Blue, Color.Orange, Color.Aquamarine, Color.Azure,
            Color.Black, Color.BlueViolet, Color.Brown, Color.Coral, Color.CornflowerBlue, Color.Crimson,
            Color.DarkCyan, Color.DarkGreen, Color.Green, Color.DarkRed, Color.Fuchsia, Color.Indigo
        };

        public DelegateCommand LoginTappedCommand { get; set; }
        public DelegateCommand ClearTappedCommand { get; set; }
        public DelegateCommand ToUpperTappedCommand { get; set; }

        private Color _statusTextColor;
        public Color StatusTextColor
        {
            get { return _statusTextColor; }
            set { SetProperty(ref _statusTextColor, value); }
        }

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

            Title = "Simple Cognito-Lambda";

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
            ChooseNewStatusTextColor();
            CustomSsoUser ssoUser = _ssoService.LoginToCustomSso(Username, Password);
            AddTextToStatusTextLabel($"#########{System.Environment.NewLine}User has authenticated with custom SSO:{System.Environment.NewLine}==> Sso user id: {ssoUser.GuidId.ToString()}{System.Environment.NewLine}==> Sso user token: {ssoUser.Token}");
            var cognitoIdentity = await _awsCognitoService.LoginToAwsWithDeveloperAuthenticatedSsoUserAsync(ssoUser);
            AddTextToStatusTextLabel($"#########{System.Environment.NewLine}User now has an AWS Cognito identity with id: {cognitoIdentity.IdentityId}");
        }

        private void ChooseNewStatusTextColor()
        {
            var randomIndex = _random.Next(_textColorChoices.Count);
            var chosenColor = _textColorChoices[randomIndex];
            StatusTextColor = _textColorChoices[randomIndex];
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
