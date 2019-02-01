using Amazon.Lambda.Model;
using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using SimpleAwsSample.Models;
using SimpleAwsSample.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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
        public DelegateCommand LogoutTappedCommand { get; set; }
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

        private CustomSsoUser _loggedInSsoUser;
        public CustomSsoUser LoggedInSsoUser
        {
            get { return _loggedInSsoUser; }
            set { SetProperty(ref _loggedInSsoUser, value); }
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
            ToUpperTappedCommand = new DelegateCommand(OnToUpperTappedExecuteAwsLambdaFunction);
            LogoutTappedCommand = new DelegateCommand(OnLogoutTapped);

            _eventAggregator.GetEvent<AddTextToUiOutput>().Subscribe(AddTextToStatusTextLabel);
        }

        #region SSO and AWS Logic

        private async void OnLoginTapped()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnLoginTapped)}");
            ClearAllStatusText();

            LoginToCustomSso(Username, Password);
            await LoginToAwsCognito();
        }

        private void LoginToCustomSso(string userName, string password)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToCustomSso)}");
            LoggedInSsoUser = _ssoService.LoginToCustomSso(userName, password);
            AddTextToStatusTextLabel($"User has authenticated with custom SSO. Sso user id: {LoggedInSsoUser.GuidId.ToString()}");
        }

        private async Task LoginToAwsCognito()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsCognito)}");
            string message = string.Empty;
            try
            {
                await _awsCognitoService.LoginToAwsCognitoAndGetCredentialsAsync(LoggedInSsoUser.GuidId.ToString());
                message = $"Successfully retrieved AWS credentials!";
            }
            catch (Exception ex)
            {
                message = $"EXCEPTION:  {ex.GetType().FullName}:  {ex.Message}";
            }
            finally
            {
                AddTextToStatusTextLabel(message);
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

            StringBuilder statusMessageBuilder = new StringBuilder($"Trying to call aws ToUpper lambda with payload: {request.Payload}{System.Environment.NewLine}");

            try
            {
                var awsResponse = await _awsLambdaService.InvokeAsync(request, _awsCognitoService.AwsCredentials, AwsConstants.AppRegionEndpoint);
                var reader = new StreamReader(awsResponse.Payload);
                string payload = reader.ReadToEnd();
                statusMessageBuilder.AppendLine($"Successfully called lambda! Result:  {payload}");
            }
            catch (Exception ex)
            {
                statusMessageBuilder.AppendLine($"Call to lambda failed with {ex.GetType().Name}:  {ex.Message}");
            }
            finally
            {
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnToUpperTappedExecuteAwsLambdaFunction)}:  {statusMessageBuilder.ToString()}");
                AddTextToStatusTextLabel(statusMessageBuilder.ToString());
            }
        }

        private void OnLogoutTapped()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnLogoutTapped)}");

            AddTextToStatusTextLabel($"Logging out... Bye!");
            _awsCognitoService.Logout();
        }

        #endregion SSO and AWS Logic

        #region Dinky methods that are relatively trivial

        private void ClearAllStatusText()
        {
            StatusText = string.Empty;
        }

        private void AddTextToStatusTextLabel(string newMessage)
        {
            StatusText += $"==============={System.Environment.NewLine}{newMessage}{System.Environment.NewLine}{System.Environment.NewLine}";
        }

        #endregion Dinky methods that are relatively trivial
    }
}
