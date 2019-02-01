using System.Threading.Tasks;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using Amazon.Runtime;
using SimpleAwsSample.Models;
using static Amazon.CognitoIdentity.CognitoAWSCredentials;

namespace SimpleAwsSample.Services
{
    public interface IAwsCognitoService
    {
        CustomSsoUser SsoUser { get; set; }
        AWSCredentials AwsCredentials { get; set; }

        Task<IdentityState> RefreshIdentityAsync();
        void Logout();
    }
}