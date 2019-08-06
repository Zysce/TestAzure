using StackExchange.Redis;
using System;

namespace TestRedis
{
    class Program
    {
        private static string _cn = "testcacheal.redis.cache.windows.net:6380,password=J4gpZnB2FOVAShlBfquLWaTHBRxrOQHGosBVL7A4LU8=,ssl=True,abortConnect=False";
        private static Lazy<ConnectionMultiplexer> _connection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect(_cn));

        static void Main(string[] args)
        {
            IDatabase cache = _connection.Value.GetDatabase();

            cache.StringSet("strkey", "val");
            var v = cache.StringGet("strkey");
            _connection.Value.Close();
        }
    }
}
