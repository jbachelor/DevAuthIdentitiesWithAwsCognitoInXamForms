using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Runtime;

namespace SimpleAwsSample.Services
{
    public class AwsLambdaService : IAwsLambdaService
    {
        public async Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest, AWSCredentials awsCredentials,
            RegionEndpoint regionEndpoint)
        {
            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(InvokeAsync)}:  Calling lambda function with payload:  {invokeRequest.Payload}");

            InvokeResponse invokeResponse = null;

            using (AmazonLambdaClient lambdaClient = new AmazonLambdaClient(awsCredentials, regionEndpoint))
            {
                invokeResponse = await lambdaClient.InvokeAsync(invokeRequest);
            }

            return invokeResponse;
        }
    }
}
