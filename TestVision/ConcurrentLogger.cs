﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestVision
{
    public static class ConcurrentLogger
    {
        private readonly static SemaphoreSlim s_printMutex = new SemaphoreSlim(1);
        private readonly static BlockingCollection<string> s_messageQueue = new BlockingCollection<string>();

        public static void WriteLine(string message)
        {
            var timestamp = DateTime.Now;
            // Push the message on the queue
            s_messageQueue.Add(timestamp.ToString("o") + ": " + message);
            // Start a new task that will dequeue one message and print it. The tasks will not
            // necessarily run in order, but since each task just takes the oldest message and
            // prints it, the messages will print in order. 
            Task.Run(async () =>
            {
                // Wait to get access to the queue. 
                await s_printMutex.WaitAsync();
                try
                {
                    string msg = s_messageQueue.Take();
                    Console.WriteLine(msg);
                }
                finally
                {
                    s_printMutex.Release();
                }
            });
        }
    }
}
