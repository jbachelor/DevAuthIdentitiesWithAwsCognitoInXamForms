﻿using System.Threading.Tasks;
using Amazon.CognitoIdentity;
using Amazon.CognitoIdentity.Model;
using SimpleAwsSample.Models;
using static Amazon.CognitoIdentity.CognitoAWSCredentials;

namespace SimpleAwsSample.Services
{
    public interface IAwsCognitoService
    {
        GetOpenIdTokenForDeveloperIdentityResponse CognitoIdentity { get; set; }
        CustomSsoUser SsoUser { get; set; }
        IdentityState UserIdentityState { get; set; }

        Task<IdentityState> RefreshIdentityAsync();
        CognitoAWSCredentials GetCognitoAwsCredentials();
    }
}