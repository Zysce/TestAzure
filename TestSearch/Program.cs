using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;

namespace TestSearch
{
    class Program
    {
        static void Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            SearchServiceClient serviceClient = CreateSearchServiceClient(configuration);

            string indexName = configuration["SearchIndexName"];

            Console.WriteLine("{0}", "Deleting index\n");
            DeleteIndexIfExists(indexName, serviceClient);

            Console.WriteLine("{0}", "Creating index\n");
            CreateIndex(indexName, serviceClient);

            ISearchIndexClient indexClient = serviceClient.Indexes.GetClient(indexName);

            Console.WriteLine("{0}", "Uploading documents\n");
            UploadDocuments(indexClient);

            ISearchIndexClient indexClientForQueries = CreateSearchIndexClient(indexName, configuration);

            RunQueries(indexClientForQueries);

            Console.WriteLine("{0}", "Complete.  Press any key to end application\n");
            Console.ReadKey();
        }

        private static SearchServiceClient CreateSearchServiceClient(IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string adminApiKey = configuration["SearchServiceAdminApiKey"];

            SearchServiceClient serviceClient = new SearchServiceClient(searchServiceName, new SearchCredentials(adminApiKey));
            return serviceClient;
        }

        private static SearchIndexClient CreateSearchIndexClient(string indexName, IConfigurationRoot configuration)
        {
            string searchServiceName = configuration["SearchServiceName"];
            string queryApiKey = configuration["SearchServiceQueryApiKey"];

            SearchIndexClient indexClient = new SearchIndexClient(searchServiceName, indexName, new SearchCredentials(queryApiKey));
            return indexClient;
        }

        private static void DeleteIndexIfExists(string indexName, SearchServiceClient serviceClient)
        {
            if (serviceClient.Indexes.Exists(indexName))
            {
                serviceClient.Indexes.Delete(indexName);
            }
        }

        private static void CreateIndex(string indexName, SearchServiceClient serviceClient)
        {
            var definition = new Index()
            {
                Name = indexName,
                Fields = FieldBuilder.BuildForType<Hotel>()
            };

            serviceClient.Indexes.Create(definition);
        }

        private static void UploadDocuments(ISearchIndexClient indexClient)
        {
            var hotels = new Hotel[]
            {
                new Hotel()
                {
                    HotelId = "1",
                    HotelName = "Secret Point Motel",
                    Address = new Address()
                    {
                        StreetAddress = "677 5th Ave"
                    },
                    Rooms = new Room[]
                    {
                        new Room()
                        {
                            Description = "Budget Room, 1 Queen Bed (Cityside)",

                        },
                        new Room()
                        {
                            Description = "Budget Room, 1 King Bed (Mountain View)",

                        },
                        new Room()
                        {
                            Description = "Deluxe Room, 2 Double Beds (City View)",

                        }
                    }
                },
                new Hotel()
                {
                    HotelId = "2",
                    HotelName = "Twin Dome Motel",
                    Address = new Address
                    {
                        StreetAddress = "140 University Town Center Dr",

                    },
                    Rooms = new Room[]
                    {
                        new Room()
                        {
                            Description = "Suite, 2 Double Beds (Mountain View)",

                        },
                        new Room()
                        {
                            Description = "Standard Room, 1 Queen Bed (City View)",

                        },
                        new Room()
                        {
                            Description = "Budget Room, 1 King Bed (Waterfront View)",

                        }
                    }
                        },
                new Hotel()
                {
                    HotelId = "3",
                    HotelName = "Triple Landscape Hotel",

                    Address = new Address()
                    {
                        StreetAddress = "3393 Peachtree Rd",

                    },
                    Rooms = new Room[]
                    {
                        new Room()
                        {
                            Description = "Standard Room, 2 Queen Beds (Amenities)",

                        },
                        new Room ()
                        {
                            Description = "Standard Room, 2 Double Beds (Waterfront View)",

                        },
                        new Room()
                        {
                            Description = "Deluxe Room, 2 Double Beds (Cityside)",

                        }
                    }
                }
            };

            var batch = IndexBatch.Upload(hotels);
            
            try
            {
                indexClient.Documents.Index(batch);
            }
            catch (IndexBatchException e)
            {
                // Sometimes when your Search service is under load, indexing will fail for some of the documents in
                // the batch. Depending on your application, you can take compensating actions like delaying and
                // retrying. For this simple demo, we just log the failed document keys and continue.
                Console.WriteLine(
                    "Failed to index some of the documents: {0}",
                    String.Join(", ", e.IndexingResults.Where(r => !r.Succeeded).Select(r => r.Key)));
            }

            Console.WriteLine("Waiting for documents to be indexed\n");
            Thread.Sleep(2000);
        }

        private static void RunQueries(ISearchIndexClient indexClient)
        {
            SearchParameters parameters;
            DocumentSearchResult<Hotel> results;

            Console.WriteLine("Search the entire index for the term 'motel' and return only the HotelName field:\n");

            parameters =
                new SearchParameters()
                {
                    Select = new[] { "HotelName" }
                };

            results = indexClient.Documents.Search<Hotel>("motel", parameters);

            WriteDocuments(results);

            Console.Write("Apply a filter to the index to find hotels with a room cheaper than $100 per night, ");
            Console.WriteLine("and return the hotelId and description:\n");

            parameters =
                new SearchParameters()
                {
                    Filter = "Rooms/any(r: r/BaseRate lt 100)",
                    Select = new[] { "HotelId", "Description" }
                };

            results = indexClient.Documents.Search<Hotel>("*", parameters);

            WriteDocuments(results);

            Console.Write("Search the entire index, order by a specific field (lastRenovationDate) ");
            Console.Write("in descending order, take the top two results, and show only hotelName and ");
            Console.WriteLine("lastRenovationDate:\n");

            parameters =
                new SearchParameters()
                {
                    OrderBy = new[] { "LastRenovationDate desc" },
                    Select = new[] { "HotelName", "LastRenovationDate" },
                    Top = 2
                };

            results = indexClient.Documents.Search<Hotel>("*", parameters);

            WriteDocuments(results);

            Console.WriteLine("Search the entire index for the term 'hotel':\n");

            parameters = new SearchParameters();
            results = indexClient.Documents.Search<Hotel>("hotel", parameters);

            WriteDocuments(results);
        }

        private static void WriteDocuments(DocumentSearchResult<Hotel> searchResults)
        {
            foreach (SearchResult<Hotel> result in searchResults.Results)
            {
                Console.WriteLine(result.Document);
            }

            Console.WriteLine();
        }
    }
}
