using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading.Tasks;

namespace TestKeyVault
{
    class Program
    {
        static void Main(string[] args)
        {
            Process().GetAwaiter().GetResult();
        }

        private static async Task Process()
        {
            var vaultBaseURL = "";
            var clientId = "";
            var secret = "";
            KeyVaultClient kvClient = new KeyVaultClient(
                async (string authority, string resource, string scope) =>
                {
                    var authContext = new AuthenticationContext(authority);
                    ClientCredential clientCred = new ClientCredential(clientId, secret);
                    AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);
                    if (result == null)
                    {
                        throw new InvalidOperationException("Failed to retrieve access token for Key Vault");
                    }

                    return result.AccessToken;
                }
            );

            // Set and get an example secret
            await kvClient.SetSecretAsync(vaultBaseURL, "test-secret", "test-secret-value-using-adal");
            SecretBundle s = await kvClient.GetSecretAsync(vaultBaseURL, "test-secret");
        }
    }
}
