using System;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;

namespace SimpleAwsSample.Services
{
    public class AwsLambdaService : IAwsLambdaService
    {
        public async Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest, CognitoAWSCredentials cognitoCredentials, 
            RegionEndpoint regionEndpoint)
        {
            InvokeResponse invokeResponse = null;

            using(AmazonLambdaClient lambdaClient = new AmazonLambdaClient(cognitoCredentials, regionEndpoint))
            {
                invokeResponse = await lambdaClient.InvokeAsync(invokeRequest);
            }

            return invokeResponse;
        }
    }
}
