using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace TestStorageAcc
{
    class Program
    {
        private static string _cn = "DefaultEndpointsProtocol=https;AccountName=teststoragealplop;AccountKey=boTZo0oHF+c877RD9slvxj7TlYSTgTaZbrinF8QUSiH8NQ/4AAJX20kRalGlHOi3qS/h/16fni0verlzwhTQiQ==;EndpointSuffix=core.windows.net";
        static void Main(string[] args)
        {
            var acc = CloudStorageAccount.Parse(_cn);

            var tableClient = acc.CreateCloudTableClient();
            var table = tableClient.GetTableReference("Test");

            InsertRow(table, "plop").GetAwaiter().GetResult();
            InsertRow(table, "toto").GetAwaiter().GetResult();
            InsertRow(table, "toto1").GetAwaiter().GetResult();
            InsertRow(table, "toto2").GetAwaiter().GetResult();
            InsertRow(table, "plop2").GetAwaiter().GetResult();

            Query(table).GetAwaiter().GetResult();
        }

        public static async Task InsertRow(CloudTable t, string name)
        {
            var obj = new TestEntity(name);
            var insOp = TableOperation.Insert(obj);
            var b = new TableBatchOperation();
            await t.ExecuteAsync(insOp);
        }

        public static async Task Query(CloudTable t)
        {
            var q = new TableQuery<TestEntity>().Where(TableQuery.GenerateFilterCondition("Name", QueryComparisons.Equal, "toto1"));
            var tok = new TableContinuationToken();
            var res = await t.ExecuteQuerySegmentedAsync(q, tok);

            foreach (var item in res.Results)
            {
                Console.WriteLine(item.Name);
            }
        }

        private static CloudBlobContainer GetBlobContainerReference(string connectionString, string containerName)
        {
            //Get the Microsoft Azure Storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            //Create the Blob service client.
            CloudBlobClient blobclient = storageAccount.CreateCloudBlobClient();

            //Returns a reference to a Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer object with the specified name.
            CloudBlobContainer blobcontainer = blobclient.GetContainerReference(containerName);


            return blobcontainer;
        }

        private static async Task MoveBlobBetweenContainers(CloudBlockBlob srcBlob, CloudBlobContainer destContainer)
        {
            CloudBlockBlob destBlob;


            //Copy source blob to destination container
            using (MemoryStream memoryStream = new MemoryStream())
            {
                await srcBlob.DownloadToStreamAsync(memoryStream);

                //put the time stamp
                var newBlobName = srcBlob.Name.Split('.')[0] + "_" + DateTime.Now.ToString("ddMMyyyy", CultureInfo.InvariantCulture) + "." + srcBlob.Name.Split('.')[1];

                destBlob = destContainer.GetBlockBlobReference(newBlobName);

                //copy source blob content to destination blob
                await destBlob.StartCopyAsync(srcBlob);

            }
            //remove source blob after copy is done.
            await srcBlob.DeleteAsync();
        }

        private static async Task LeaseBlob(CloudBlockBlob b)
        {
            var leaseId = await b.AcquireLeaseAsync(null);
            var acc = new AccessCondition { LeaseId = leaseId };
            try
            {
                await b.RenewLeaseAsync(acc);
            }
            finally
            {

                await b.ReleaseLeaseAsync(new AccessCondition { LeaseId = leaseId });
            }
        }

        private static async Task SetMetaAndProps(CloudBlockBlob b)
        {
            b.Properties.ContentLanguage = "fr-FR";
            await b.SetPropertiesAsync();

            b.Metadata["b"] = "v";
            await b.SetMetadataAsync();
        }
    }

    public class TestEntity : TableEntity
    {
        public TestEntity()
        {
        }

        public TestEntity(string name)
        {
            Name = name;
            PartitionKey = "toto";
            RowKey = Guid.NewGuid().ToString();
        }

        public string Name { get; set; }
    }
}
