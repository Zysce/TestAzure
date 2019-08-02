using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using System;
using System.Threading.Tasks;

namespace TestBlob
{
    class Program
    {
        private static string _cn = "DefaultEndpointsProtocol=https;AccountName=teststoragealplop;AccountKey=boTZo0oHF+c877RD9slvxj7TlYSTgTaZbrinF8QUSiH8NQ/4AAJX20kRalGlHOi3qS/h/16fni0verlzwhTQiQ==;EndpointSuffix=core.windows.net";

        static void Main(string[] args)
        {
            ProcessAsync().GetAwaiter().GetResult();
        }

        private static async Task ProcessAsync()
        {
            CloudStorageAccount storageAccount;
            if (CloudStorageAccount.TryParse(_cn, out storageAccount))
            {
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                // Create a container called 'quickstartblobs' and 
                // append a GUID value to it to make the name unique.
                CloudBlobContainer cloudBlobContainer =
                    cloudBlobClient.GetContainerReference("blobtest");

                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null); // Loop while the continuation token is not null.
            }
            else
            {
                // Otherwise, let the user know that they need to define the environment variable.
                Console.WriteLine(
                    "A connection string has not been defined in the system environment variables. " +
                    "Add an environment variable named 'STORAGE_CONNECTION_STRING' with your storage " +
                    "connection string as a value.");
                Console.WriteLine("Press any key to exit the application.");
                Console.ReadLine();
            }
        }
    }
}
