using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.Runtime;
using Prism.Events;
using SimpleAwsSample.Models;

namespace SimpleAwsSample.Services
{
    public class AwsCognitoService : CognitoAWSCredentials, IAwsCognitoService
    {
        private readonly IEventAggregator _eventAggregator;
        string _userIdFromSsoSystem = string.Empty;

        public AWSCredentials AwsCredentials { get; private set; }


        /// <summary>
        /// For some reason, the base constructor throws a System.InvalidOperationException
        /// with the message, "The app.config/web.config files for the application did not contain region information".
        /// </summary>
        public AwsCognitoService(IEventAggregator eventAggregator)
            : base(AwsConstants.AwsAccountId, AwsConstants.AppIdentityPoolId, AwsConstants.UnAuthedRoleArn,
            AwsConstants.AuthedRoleArn, AwsConstants.AppRegionEndpoint)
        {
            Debug.WriteLine($"**** {this.GetType().Name}:  ctor\n\tAccountId={this.AccountId}\n\tIdentityPoolId={this.IdentityPoolId}\n\tUnAuthRoleArn={this.UnAuthRoleArn}\n\tAuthRoleArn={this.AuthRoleArn}\n\t");

            _eventAggregator = eventAggregator;
            ConfigureAws();
        }

        private void ConfigureAws()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(ConfigureAws)}");

            AWSConfigs.RegionEndpoint = AwsConstants.AppRegionEndpoint;
            AWSConfigs.CorrectForClockSkew = true;

            var loggingConfig = AWSConfigs.LoggingConfig;
            loggingConfig.LogMetrics = true;
            loggingConfig.LogResponses = ResponseLoggingOption.Always;
            loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
            loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;
        }

        public async Task LoginToAwsCognitoAndGetCredentialsAsync(string userIdFromSsoSystem)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsCognitoAndGetCredentialsAsync)}");

            _userIdFromSsoSystem = userIdFromSsoSystem;
            await RefreshIdentityAsync();
        }

        public override async Task<IdentityState> RefreshIdentityAsync()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(RefreshIdentityAsync)}");

            // (1) GetOpenIdTokenForDeveloperIdentity
            var openIdTokenForDeveloperIdentityResponse = await LoginToAwsWithDeveloperAuthenticatedSsoUserAsync();

            var identityState = new IdentityState(
                openIdTokenForDeveloperIdentityResponse.IdentityId,
                AwsConstants.DeveloperProviderName,
                openIdTokenForDeveloperIdentityResponse.Token,
                false);

            // (2) GetCredentialsForIdentity
            GetCredentialsForIdentityResponse credentialsResponse = await GetCredentialsForIdentityFromAwsAsync(identityState);
            AwsCredentials = credentialsResponse.Credentials;

            return identityState;
        }

        private async Task<GetCredentialsForIdentityResponse> GetCredentialsForIdentityFromAwsAsync(IdentityState identityState)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCredentialsForIdentityFromAwsAsync)}");

            GetCredentialsForIdentityResponse credentialsResponse = null;
            GetCredentialsForIdentityRequest credentialsRequest = new GetCredentialsForIdentityRequest
            {
                Logins = new Dictionary<string, string> { { AwsConstants.AwsCognitoIdentityProviderKey, identityState.LoginToken } },
                //Logins = this.CloneLogins, // this will cause an error.
                IdentityId = identityState.IdentityId
            };
        
            PublishStatusUpdateFromCredentialsRequest(credentialsRequest);

            using (var cognitoIdentityClient = new AmazonCognitoIdentityClient(AwsConstants.AppDevAccessKey,
                AwsConstants.AppDevSecretKey, AwsConstants.AppRegionEndpoint))
            {
                credentialsResponse = await cognitoIdentityClient.GetCredentialsForIdentityAsync(credentialsRequest);
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCredentialsForIdentityFromAwsAsync)}: Successfully got aws credentials!");
            }

            return credentialsResponse;
        }

        private async Task<GetOpenIdTokenForDeveloperIdentityResponse> LoginToAwsWithDeveloperAuthenticatedSsoUserAsync()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithDeveloperAuthenticatedSsoUserAsync)}");

            // Add sso user id ("GuidId") to CognitoAWSCredentials with key for our custom developer provider name:
            AddLoginToCredentials(AwsConstants.DeveloperProviderName, _userIdFromSsoSystem);

            GetOpenIdTokenForDeveloperIdentityResponse response = await GetOpenIdTokenForDeveloperIdentityFromAwsAsync();

            // Add aws cognito token to CognitoAWSCredentials with prescribed key from aws:
            AddLoginToCredentials(AwsConstants.AwsCognitoIdentityProviderKey, response.Token);

            return response;
        }

        private async Task<GetOpenIdTokenForDeveloperIdentityResponse> GetOpenIdTokenForDeveloperIdentityFromAwsAsync()
        {
            var tokenRequest = new GetOpenIdTokenForDeveloperIdentityRequest
            {
                IdentityPoolId = AwsConstants.AppIdentityPoolId,
                Logins = new Dictionary<string, string>
                {
                    {  AwsConstants.DeveloperProviderName, _userIdFromSsoSystem }
                },
                TokenDuration = (long)TimeSpan.FromDays(1).TotalSeconds
            };

            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetOpenIdTokenForDeveloperIdentityFromAwsAsync)}:  About to call GetOpenIdTokenForDeveloperIdentityAsync with {nameof(AwsConstants.DeveloperProviderName)}=[{AwsConstants.DeveloperProviderName}], SSO GuidId=[{_userIdFromSsoSystem}]");

            GetOpenIdTokenForDeveloperIdentityResponse response;
            using (var cognitoIdentityClient = new AmazonCognitoIdentityClient(AwsConstants.AppDevAccessKey,
                AwsConstants.AppDevSecretKey, AwsConstants.AppRegionEndpoint))
            {
                response = await cognitoIdentityClient.GetOpenIdTokenForDeveloperIdentityAsync(tokenRequest);
            }
            string successMessage = $"Successful call to GetOpenIdTokenForDeveloperIdentityAsync. Got response with {nameof(response.IdentityId)}=[{response.IdentityId}]";
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetOpenIdTokenForDeveloperIdentityFromAwsAsync)}:  {successMessage}, {nameof(response.Token)}={response.Token}");
            _eventAggregator.GetEvent<AddTextToUiOutput>().Publish(successMessage);
            return response;
        }

        private void AddLoginToCredentials(string providerName, string token)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(AddLoginToCredentials)}\n\t{providerName} : {token}");

            this.AddLogin(providerName, token);
        }

        private void PublishStatusUpdateFromCredentialsRequest(GetCredentialsForIdentityRequest credentialsRequest)
        {
            string message = $"About to call GetCredentialsForIdentityAsync with {credentialsRequest.Logins.Count} logins:";
            foreach (var login in credentialsRequest.Logins)
            {
                message += $"\n\tkey: {login.Key}";

                string max20CharsValue = login.Value.Length > 20
                    ? $"{login.Value.Substring(0, 20)}...(truncated)"
                    : login.Value;

                message += $"\n\tValue: {max20CharsValue}";
            }
            _eventAggregator.GetEvent<AddTextToUiOutput>().Publish(message);
        }

        public void Logout()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(Logout)}");
            _userIdFromSsoSystem = string.Empty;
            AwsCredentials = null;
        }
    }
}
