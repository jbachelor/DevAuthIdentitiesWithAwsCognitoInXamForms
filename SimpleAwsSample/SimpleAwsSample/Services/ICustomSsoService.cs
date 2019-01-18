using SimpleAwsSample.Models;

namespace SimpleAwsSample.Services
{
    public interface ICustomSsoService
    {
        CustomSsoUser LoginToCustomSso(string username, string password);
    }
}