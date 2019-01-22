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
    public class AwsCognitoService : IAwsCognitoService
    {
        public GetOpenIdTokenForDeveloperIdentityResponse CognitoIdentity { get; set; }
        public CognitoAWSCredentials Credentials { get; set; }

        public AwsCognitoService()
        {
            Debug.WriteLine($"**** {this.GetType().Name}:  ctor");
            ConfigureAws();
        }

        public async Task<GetOpenIdTokenForDeveloperIdentityResponse> LoginToAwsWithDeveloperAuthenticatedSsoUserAsync(CustomSsoUser coolAppSsoUser)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithDeveloperAuthenticatedSsoUserAsync)}");
            AddLoginToCredentials(AwsConstants.DeveloperProviderName, coolAppSsoUser.GuidId.ToString());


            var tokenRequest = new GetOpenIdTokenForDeveloperIdentityRequest
            {
                IdentityPoolId = AwsConstants.AppIdentityPoolId
            };

            tokenRequest.Logins = new Dictionary<string, string>
            {
                { AwsConstants.DeveloperProviderName, coolAppSsoUser.GuidId.ToString() }
            };

            tokenRequest.TokenDuration = (long)TimeSpan.FromDays(1).TotalSeconds;

            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithDeveloperAuthenticatedSsoUserAsync)}:  About to call GetOpenIdTokenForDeveloperIdentityAsync with {nameof(AwsConstants.DeveloperProviderName)}=[{AwsConstants.DeveloperProviderName}], SSO GuidId=[{coolAppSsoUser.GuidId}]");

            GetOpenIdTokenForDeveloperIdentityResponse response;
            using (var cognitoIdentityClient = new AmazonCognitoIdentityClient(AwsConstants.AppDevAccessKey, 
                AwsConstants.AppDevSecretKey, AwsConstants.AppRegionEndpoint))
            {
                response = await cognitoIdentityClient.GetOpenIdTokenForDeveloperIdentityAsync(tokenRequest);
            }
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithDeveloperAuthenticatedSsoUserAsync)}:  Done calling GetOpenIdTokenForDeveloperIdentityAsync. Got response with {nameof(response.IdentityId)}=[{response.IdentityId}], {nameof(response.Token)}={response.Token}");

            CognitoIdentity = response;
            return CognitoIdentity;
        }

        private void AddLoginToCredentials(string developerProviderName, string userId)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(AddLoginToCredentials)}");
            Credentials.AddLogin(AwsConstants.DeveloperProviderName, userId);
        }

        private void ConfigureAws()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(ConfigureAws)}");
            AWSConfigs.RegionEndpoint = AwsConstants.AppRegionEndpoint;
            AWSConfigs.CorrectForClockSkew = true;
            ConfigureLogging();
            InitializeAwsCognitoCredentialsProvider();
        }

        private void ConfigureLogging()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(ConfigureLogging)}");
            var loggingConfig = AWSConfigs.LoggingConfig;
            loggingConfig.LogMetrics = true;
            loggingConfig.LogResponses = ResponseLoggingOption.Always;
            loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
            loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;
        }

        private void InitializeAwsCognitoCredentialsProvider()
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(InitializeAwsCognitoCredentialsProvider)}");
            Credentials = new CognitoAWSCredentials(
                AwsConstants.AwsAccountId, AwsConstants.AppIdentityPoolId, 
                AwsConstants.UnAuthedRoleArn, AwsConstants.AuthedRoleArn, RegionEndpoint.USEast2);
        }
    }
}
