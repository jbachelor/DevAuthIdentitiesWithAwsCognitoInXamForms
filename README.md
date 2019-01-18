# DevAuthIdentitiesWithAwsCognitoInXamForms
A small code experiment... Grown into a sample app (using [Prism](https://prismlibrary.github.io/)... which I love dearly)... on establishing a user in an AWS Identity Pool using Developer Authenticated Identities (custom SSO login).

## Special thanks!
I'd like to profusely thank [kneekey23](https://github.com/kneekey23) and [steveataws](https://github.com/steveataws) for their help and advice as I struggled through learning how to implement this functionality. Thank you for your time, knowledge, effort, and assistance! I am in your debt!

## Purpose of this code
Users of this app login to a company's custom SSO server (a total fake in this app, but BOY is it fast!), and the app receives a unique id (the *GuidId* property you see in the code of the *coolAppSsoUser* object) and session token (the *Token* property you see in the code of the *coolAppSsoUser* object) from the SSO server.

The idea is that the user would log in with the custom SSO provider, and behind the scenes we are now trying to either create a new or get an existing Amazon Cognito Identity from an Identity Pool, using the data received from the SSO server (in particular, the user's unique id and the SSO session token... though I haven't found out how or where to use the session token yet). The SSO user object is passed into the *LoginToAwsWithDeveloperAuthenticatedSsoUserAsync* method, and the AWS Cognito Identity is created (if it does not already exist) and retrieved.

From here, the goal is to now have the user able to trigger Lambda functions, API Gateway calls, etc.

## Concerns and To-Dos
* I have not found out how/where to use the session token retrieved from the custom SSO server.
* I'm not entirely certain that these are considered "authenticated" or "unauthenticated" Cognito users. I believe they should be of the "authenticated" variety, but I'm not sure how to verify that just yet.
* There is a fourth step AWS outlines (see screenshot below), "GetCredentialsForIdentity", which I am not sure how to implement yet.
* Should the AwsCognitoService in the app inherit from [CognitoAWSCredentials](https://github.com/aws/aws-sdk-net/blob/master/sdk/src/Services/CognitoIdentity/Custom/CognitoAWSCredentials.cs)?

## Where does this code come from?
Following the steps outlined in the [AWS documentation](https://docs.aws.amazon.com/cognito/latest/developerguide/authentication-flow.html), specifically under the heading **Developer Authenticated Identities Authflow**, then the sub-heading **Enhanced Authflow**.

![Screenshot of the aws suggested auth flow for developer authenticated identities](DevAuthIdsEnhancedFlow.png "Developer Authenticated Identity Authflow - Enhanced Authflow")

## Resources
* [AWS .NET Developer Center](https://aws.amazon.com/developer/language/net/)
* https://docs.aws.amazon.com/cognito/latest/developerguide/authentication-flow.html
* https://docs.aws.amazon.com/mobile/sdkforxamarin/developerguide/setup.html
* https://docs.aws.amazon.com/cognito/latest/developerguide/developer-authenticated-identities.html
* [Deep Dive on User Sign-up and Sign-in with Amazon Cognito](https://www.youtube.com/watch?v=KWjgiNgDfwI)
* [AWS re:Invent 2017: Identity Management for Your Users and Apps: A Deep Dive on Amaz (SID332)](https://www.youtube.com/watch?time_continue=16&v=jLQjQpUYw6g)
* [Prism Library](https://prismlibrary.github.io/)
