using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using SimpleAwsSample.Models;

namespace SimpleAwsSample.Services
{
    public class AwsCognitoService : CognitoAWSCredentials, IAwsCognitoService
    {
        public CustomSsoUser SsoUser { get; set; }

        /// <summary>
        /// For some reason, the base constructor throws a System.InvalidOperationException
        /// with the message, "The app.config/web.config files for the application did not contain region information".
        /// </summary>
        public AwsCognitoService()
            : base(AwsConstants.AwsAccountId, AwsConstants.AppIdentityPoolId, AwsConstants.UnAuthedRoleArn,
            AwsConstants.AuthedRoleArn, AwsConstants.AppRegionEndpoint)
        {
            Debug.WriteLine($"**** {this.GetType().Name}:  ctor\n\tAccountId={this.AccountId}\n\tIdentityPoolId={this.IdentityPoolId}\n\tUnAuthRoleArn={this.UnAuthRoleArn}\n\tAuthRoleArn={this.AuthRoleArn}\n\t");

            ConfigureAws();
        }

        public CognitoAWSCredentials GetCognitoAwsCredentials()
        {
            return this;
        }

        /// <summary>
        /// This is the method to be called from a ViewModel in order to (1) GetOpenIdTokenForDeveloperIdentity, 
        /// and (2) GetCredentialsForIdentity.
        /// </summary>
        /// <returns>The identity async.</returns>
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

            // (2) GetCredentialsForIdentity ?? -- Uncommenting the line below leads to an InvalidParameterException:  Please provide a valid public provider.
            //var credentialsResponse = await GetCredentialsForIdentityFromAwsAsync(identityState);

            // Return identityState
            return identityState;
        }

        private async Task<GetCredentialsForIdentityResponse> GetCredentialsForIdentityFromAwsAsync(IdentityState identityState)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCredentialsForIdentityFromAwsAsync)}");

            GetCredentialsForIdentityResponse credentialsResponse = null;
            GetCredentialsForIdentityRequest credentialsRequest = new GetCredentialsForIdentityRequest
            {
                Logins = this.CloneLogins,
                IdentityId = identityState.IdentityId
            };

            using (var cognitoIdentityClient = new AmazonCognitoIdentityClient(AwsConstants.AppDevAccessKey,
                AwsConstants.AppDevSecretKey, AwsConstants.AppRegionEndpoint))
            {
                credentialsResponse = await cognitoIdentityClient.GetCredentialsForIdentityAsync(credentialsRequest);
            }

            return credentialsResponse;
        }

        private async Task<GetOpenIdTokenForDeveloperIdentityResponse> LoginToAwsWithDeveloperAuthenticatedSsoUserAsync()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithDeveloperAuthenticatedSsoUserAsync)}");

            if (SsoUser == null || string.IsNullOrWhiteSpace(SsoUser.Token))
            {
                throw new ApplicationException("SsoUser property must be populated with a valid SSO user.");
            }

            // Add sso user id ("GuidId") to CognitoAWSCredentials with key for our custom developer provider name:
            AddLoginToCredentials(AwsConstants.DeveloperProviderName, SsoUser.GuidId.ToString());

            GetOpenIdTokenForDeveloperIdentityResponse response = await GetOpenIdTokenForDeveloperIdentity();

            // Add aws cognito token to CognitoAWSCredentials with prescribed key from aws:
            AddLoginToCredentials(AwsConstants.AwsCognitoIdentityProviderKey, response.Token);

            return response;
        }

        private async Task<GetOpenIdTokenForDeveloperIdentityResponse> GetOpenIdTokenForDeveloperIdentity()
        {
            var tokenRequest = new GetOpenIdTokenForDeveloperIdentityRequest
            {
                IdentityPoolId = AwsConstants.AppIdentityPoolId,
                Logins = new Dictionary<string, string>
                {
                    {  AwsConstants.DeveloperProviderName, SsoUser.GuidId.ToString() }
                },
                TokenDuration = (long)TimeSpan.FromDays(1).TotalSeconds
            };

            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetOpenIdTokenForDeveloperIdentity)}:  About to call GetOpenIdTokenForDeveloperIdentityAsync with {nameof(AwsConstants.DeveloperProviderName)}=[{AwsConstants.DeveloperProviderName}], SSO GuidId=[{SsoUser.GuidId}]");

            GetOpenIdTokenForDeveloperIdentityResponse response;
            using (var cognitoIdentityClient = new AmazonCognitoIdentityClient(AwsConstants.AppDevAccessKey,
                AwsConstants.AppDevSecretKey, AwsConstants.AppRegionEndpoint))
            {
                response = await cognitoIdentityClient.GetOpenIdTokenForDeveloperIdentityAsync(tokenRequest);
            }
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetOpenIdTokenForDeveloperIdentity)}:  Done calling GetOpenIdTokenForDeveloperIdentityAsync. Got response with {nameof(response.IdentityId)}=[{response.IdentityId}], {nameof(response.Token)}={response.Token}");
            return response;
        }

        private void AddLoginToCredentials(string providerName, string token)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(AddLoginToCredentials)}\n\t{providerName} : {token}");

            this.AddLogin(providerName, token);
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
    }
}
