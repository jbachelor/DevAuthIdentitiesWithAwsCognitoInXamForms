using System;
using Amazon;

namespace SimpleAwsSample.Models
{
    public static class AwsConstants
    {
        // AWS Info -- Replace with valid values from your AWS account
                   
        public const string LambdaToUpperFunctionArn = "";
        public const string DeveloperProviderName = "";
        public const string AppIdentityPoolId = "";
        public const string AwsAccountId = "";
        public const string UnAuthedRoleArn = "";
        public const string AuthedRoleArn = "";
        public const string AppDevAccessKey = "";
        public const string AppDevSecretKey = "";
        public const string AwsCognitoIdentityProviderKey = "cognito-identity.amazonaws.com";
        public static RegionEndpoint AppRegionEndpoint = RegionEndpoint.USEast2;
    }
}
