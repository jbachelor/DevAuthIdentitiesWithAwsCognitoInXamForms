using System.Threading.Tasks;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using SimpleAwsSample.Models;

namespace SimpleAwsSample.Services
{
    public interface IAwsCognitoService
    {
        GetOpenIdTokenForDeveloperIdentityResponse CognitoIdentity { get; set; }
        CognitoAWSCredentials Credentials { get; set; }

        Task<GetOpenIdTokenForDeveloperIdentityResponse> LoginToAwsWithDeveloperAuthenticatedSsoUserAsync(CustomSsoUser coolAppSsoUser);
    }
}