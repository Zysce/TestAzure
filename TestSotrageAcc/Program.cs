using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
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
