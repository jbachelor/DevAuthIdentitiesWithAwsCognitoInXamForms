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

        public override async Task<IdentityState> RefreshIdentityAsync()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(RefreshIdentityAsync)}");

            var openIdTokenForDeveloperIdentityResponse = await LoginToAwsWithDeveloperAuthenticatedSsoUserAsync();
            var identityState = new IdentityState(
                openIdTokenForDeveloperIdentityResponse.IdentityId,
                AwsConstants.DeveloperProviderName,
                openIdTokenForDeveloperIdentityResponse.Token,
                false);

            return identityState;
        }

        private async Task<GetOpenIdTokenForDeveloperIdentityResponse> LoginToAwsWithDeveloperAuthenticatedSsoUserAsync()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithDeveloperAuthenticatedSsoUserAsync)}");

            if(SsoUser == null || string.IsNullOrWhiteSpace(SsoUser.Token))
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
