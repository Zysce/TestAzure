using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestCosmosDb
{
    class Program
    {
        static string _url = "https://cosmosdbtestal.documents.azure.com:443/";
        static string _key = "BtMYlyBZ7dCzoGQJ3EPftPLsfMRwHxqs8HlUP9k5bTN9K2HNBTv6gEiuPypJgMdrcEtsvdyKvzytTYUEpM7sIQ==";
        static string _dbName = "dbtest";
        static string _containerName = "containertest";
        static Uri _uriDb = UriFactory.CreateDocumentCollectionUri(_dbName, _containerName);
        static void Main(string[] args)
        {
            var client = new DocumentClient(new Uri(_url), _key);

            Insert(client, "toto").GetAwaiter().GetResult();
            Insert(client, "toto4").GetAwaiter().GetResult();
            Insert(client, "to5to").GetAwaiter().GetResult();
            Insert(client, "t8oto").GetAwaiter().GetResult();

            Query(client).GetAwaiter().GetResult();
        }

        static async Task Insert(DocumentClient c, string name)
        {
            var doc = new TestEntity(name);
            await c.CreateDocumentAsync(_uriDb, doc);
        }

        static async Task Query(DocumentClient c)
        {
            var queryOptions = new FeedOptions
            {
                MaxItemCount = 50,
                EnableCrossPartitionQuery = true,
                MaxDegreeOfParallelism = -1,
                MaxBufferedItemCount = -1
            };

            var query = c
                .CreateDocumentQuery<TestEntity>(
                   _uriDb, queryOptions)
                .Where(f => f.Name.StartsWith("toto"))
                .AsDocumentQuery();

            var results = new List<TestEntity>();
            try
            {
                while (query.HasMoreResults)
                {
                    var items = await query.ExecuteNextAsync<TestEntity>();
                    foreach (var item in items)
                    {
                        Console.WriteLine(JsonConvert.SerializeObject(item));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

    public class TestEntity
    {
        public TestEntity()
        {
        }

        public TestEntity(string name)
        {
            Name = name;
            Partition = "42";
        }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("partition")]
        public string Partition { get; set; }
    }
}
