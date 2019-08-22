using System;

namespace TestEventGrid
{
    class Program
    {
        static void Main(string[] args)
        {
            new Publisher().Run();
            Consumer.Run().GetAwaiter().GetResult();
        }
    }
}
