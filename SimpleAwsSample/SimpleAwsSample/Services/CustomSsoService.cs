using System;
using SimpleAwsSample.Models;

namespace SimpleAwsSample.Services
{
    public class CustomSsoService : ICustomSsoService
    {
        public CustomSsoUser LoginToCustomSso(string username, string password)
        {
            return new CustomSsoUser
            {
                GuidId = Guid.NewGuid(),
            };
        }
    }
}
