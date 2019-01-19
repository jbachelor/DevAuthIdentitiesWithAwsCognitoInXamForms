using System.Threading.Tasks;
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.Lambda.Model;

namespace SimpleAwsSample.Services
{
    public interface IAwsLambdaService
    {
        Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest, CognitoAWSCredentials cognitoCredentials, RegionEndpoint regionEndpoint);
    }
}