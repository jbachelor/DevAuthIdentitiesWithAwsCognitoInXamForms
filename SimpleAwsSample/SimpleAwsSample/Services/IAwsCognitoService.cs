using System.Threading.Tasks;
using Amazon.Runtime;
using SimpleAwsSample.Models;

namespace SimpleAwsSample.Services
{
    public interface IAwsCognitoService
    {
        AWSCredentials AwsCredentials { get; }

        Task LoginToAwsCognitoAndGetCredentialsAsync(string userIdFromSsoSystem);
        void Logout();
    }
}