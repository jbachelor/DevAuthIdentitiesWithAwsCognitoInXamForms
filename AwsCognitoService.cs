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
        public static string DeveloperProviderName = "xxx.yyyyy.zzzzz";
        public static string IdentityPoolIdJJB1 = "us-east-2:12345678-012d-2n33-2ie9-4jgk746a79v5";
        public static RegionEndpoint CoolAppRegionEndpoint = RegionEndpoint.USEast2;
        public static string AwsAccountId = "927784729";
        public static string UnAuthedRoleArn = "arn:aws:iam::6823764824:role/Cognito_1Unauth_Role";
        public static string AuthedRoleArn = "arn:aws:iam::367884619439:role/Cognito_1Auth_Role";
        public static string CoolAppDevAccessKey = "KSJD98S9898SFJSKLDJF2";
        public static string CoolAppDevSecretKey = "42kjDKFJLkjds8948928jfkdjsjlkasjdf89284924fjkdsfa";

        public GetOpenIdTokenForDeveloperIdentityResponse CognitoIdentity { get; set; }
        public CognitoAWSCredentials Credentials { get; set; }

        public AwsCognitoService()
        {
            ConfigureAws();
        }

        public async Task LoginToAwsWithDeveloperAuthenticatedSsoUserAsync()
        {
            var cognitoIdentityClient = new AmazonCognitoIdentityClient(CoolAppDevAccessKey, CoolAppDevSecretKey, CoolAppRegionEndpoint);

            var tokenRequest = new GetOpenIdTokenForDeveloperIdentityRequest
            {
                IdentityPoolId = IdentityPoolIdJJB1
            };

            tokenRequest.Logins = new Dictionary<string, string>
            {
                { DeveloperProviderName, Globals.COOLAPP_SSO_USER.GuidId.ToString() }
            };

            tokenRequest.TokenDuration = (long)TimeSpan.FromDays(1).TotalSeconds; 

            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(LoginToAwsWithCoolAppSsoUserAsync)}:  About to call GetOpenIdTokenForDeveloperIdentityAsync with {nameof(DeveloperProviderName)}=[{DeveloperProviderName}], SSO GuidId=[{Globals.COOLAPP_SSO_USER.GuidId}]");
            
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
