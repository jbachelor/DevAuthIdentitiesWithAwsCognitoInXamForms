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
            ConfigureAws();
        }

        public async Task<GetOpenIdTokenForDeveloperIdentityResponse> LoginToAwsWithDeveloperAuthenticatedSsoUserAsync(CustomSsoUser coolAppSsoUser)
        {
            AddLoginToCredentials(AwsConstants.DeveloperProviderName, coolAppSsoUser.GuidId.ToString());

            var cognitoIdentityClient = new AmazonCognitoIdentityClient(AwsConstants.AppDevAccessKey, AwsConstants.AppDevSecretKey, AwsConstants.AppRegionEndpoint);

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

            GetOpenIdTokenForDeveloperIdentityResponse response =
                await cognitoIdentityClient.GetOpenIdTokenForDeveloperIdentityAsync(tokenRequest);

            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithDeveloperAuthenticatedSsoUserAsync)}:  Done calling GetOpenIdTokenForDeveloperIdentityAsync. Got response with {nameof(response.IdentityId)}=[{response.IdentityId}], {nameof(response.Token)}={response.Token}");

            CognitoIdentity = response;
            return CognitoIdentity;
        }

        private void AddLoginToCredentials(string developerProviderName, string userId)
        {
            Credentials.AddLogin(AwsConstants.DeveloperProviderName, userId);
        }

        private void ConfigureAws()
        {
            AWSConfigs.RegionEndpoint = AwsConstants.AppRegionEndpoint;
            AWSConfigs.CorrectForClockSkew = true;
            ConfigureLogging();
            InitializeAwsCognitoCredentialsProvider();
        }

        private void ConfigureLogging()
        {
            var loggingConfig = AWSConfigs.LoggingConfig;
            loggingConfig.LogMetrics = true;
            loggingConfig.LogResponses = ResponseLoggingOption.Always;
            loggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
            loggingConfig.LogTo = LoggingOptions.SystemDiagnostics;
        }

        private void InitializeAwsCognitoCredentialsProvider()
        {
            Credentials = new CognitoAWSCredentials(
                AwsConstants.AwsAccountId, AwsConstants.AppIdentityPoolId, 
                AwsConstants.UnAuthedRoleArn, AwsConstants.AuthedRoleArn, RegionEndpoint.USEast2);
        }
    }
}
