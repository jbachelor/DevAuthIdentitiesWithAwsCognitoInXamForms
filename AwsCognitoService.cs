using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.Runtime;

namespace CoolApp.Services.Aws
{
    public class AwsCognitoService
    {
        public static string DeveloperProviderName = "xxx.fake.zzzzz";
        public static string IdentityPoolIdJJB1 = "us-east-2:12345678-fake-2n33-2ie9-4jgk746a79v5";
        public static RegionEndpoint CoolAppRegionEndpoint = RegionEndpoint.USEast2;
        public static string AwsAccountId = "92FAKE729";
        public static string UnAuthedRoleArn = "arn:aws:iam::68FAKE4824:role/Cognito_1Unauth_Role";
        public static string AuthedRoleArn = "arn:aws:iam::36788FAKE9439:role/Cognito_1Auth_Role";
        public static string CoolAppDevAccessKey = "KSJD98SFAKEFJSKLDJF2";
        public static string CoolAppDevSecretKey = "42kjDKFJLkjds8948928jfkdjFAKEjdf89284924fjkdsfa";

        public GetOpenIdTokenForDeveloperIdentityResponse CognitoIdentity { get; set; }
        public CognitoAWSCredentials Credentials { get; set; }

        public AwsCognitoService()
        {
            ConfigureAws();
        }

        public async Task LoginToAwsWithDeveloperAuthenticatedSsoUserAsync(CoolAppSsoUser coolAppSsoUser)
        {
            var cognitoIdentityClient = new AmazonCognitoIdentityClient(CoolAppDevAccessKey, CoolAppDevSecretKey, CoolAppRegionEndpoint);

            var tokenRequest = new GetOpenIdTokenForDeveloperIdentityRequest
            {
                IdentityPoolId = IdentityPoolIdJJB1
            };

            tokenRequest.Logins = new Dictionary<string, string>
            {
                { DeveloperProviderName, coolAppSsoUser.GuidId.ToString() }
            };

            tokenRequest.TokenDuration = (long)TimeSpan.FromDays(1).TotalSeconds; 

            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithCoolAppSsoUserAsync)}:  About to call GetOpenIdTokenForDeveloperIdentityAsync with {nameof(DeveloperProviderName)}=[{DeveloperProviderName}], SSO GuidId=[{coolAppSsoUser.GuidId}]");
            
            GetOpenIdTokenForDeveloperIdentityResponse response =
                await cognitoIdentityClient.GetOpenIdTokenForDeveloperIdentityAsync(tokenRequest);
            
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithCoolAppSsoUserAsync)}:  Done calling GetOpenIdTokenForDeveloperIdentityAsync. Got response with {nameof(response.IdentityId)}=[{response.IdentityId}], {nameof(response.Token)}={response.Token}");
            
            CognitoIdentity = response;
        }

        private void ConfigureAws()
        {
            AWSConfigs.RegionEndpoint = CoolAppRegionEndpoint;
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
                AwsAccountId, IdentityPoolIdJJB1, UnAuthedRoleArn, AuthedRoleArn, RegionEndpoint.USEast2);
        }
    }
}
