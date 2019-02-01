using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Model;
using Amazon.Runtime;

namespace SimpleAwsSample.Services
{
    public interface IAwsLambdaService
    {
        Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest, AWSCredentials awsCredentials, RegionEndpoint regionEndpoint);
    }
}