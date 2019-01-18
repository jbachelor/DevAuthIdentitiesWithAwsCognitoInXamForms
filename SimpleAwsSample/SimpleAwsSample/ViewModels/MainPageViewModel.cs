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

        public DelegateCommand LoginTappedCommand { get; set; }
        public DelegateCommand ClearTappedCommand { get; set; }

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

        public MainPageViewModel(INavigationService navigationService, ICustomSsoService ssoService, IAwsCognitoService awsCognitoService)
            : base(navigationService)
        {
            _ssoService = ssoService;
            _awsCognitoService = awsCognitoService;
            Title = "Simple Cognito";

            LoginTappedCommand = new DelegateCommand(OnLoginTapped);
            ClearTappedCommand = new DelegateCommand(OnClearTapped);
        }

        private void OnClearTapped()
        {
            StatusText = string.Empty;
        }

        private async void OnLoginTapped()
        {
            CustomSsoUser ssoUser = _ssoService.LoginToCustomSso(Username, Password);
            AddTextToStatusTextLabel($"#########{Environment.NewLine}User has authenticated with custom SSO:{Environment.NewLine}==> Sso user id: {ssoUser.GuidId.ToString()}{Environment.NewLine}==> Sso user token: {ssoUser.Token}");
            var cognitoIdentity = await _awsCognitoService.LoginToAwsWithDeveloperAuthenticatedSsoUserAsync(ssoUser);
            AddTextToStatusTextLabel($"#########{Environment.NewLine}User now has an AWS Cognito identity with id: {cognitoIdentity.IdentityId}");
        }

        private void AddTextToStatusTextLabel(string newMessage)
        {
            StatusText += $"{newMessage}{Environment.NewLine}{Environment.NewLine}";
        }
    }
}
