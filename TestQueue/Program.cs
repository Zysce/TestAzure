using Microsoft.Azure;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using System;

namespace TestQueue
{
    class Program
    {
        static void Main(string[] args)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting("StorageConnectionString"));

            // Create the queue client.
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a container.
            CloudQueue queue = queueClient.GetQueueReference("myqueue");

            // Create the queue if it doesn't already exist
            queue.CreateIfNotExists();

            // Create a message and add it to the queue.
            CloudQueueMessage message = new CloudQueueMessage("Hello, World");
            queue.AddMessage(message);


            // Peek at the next message
            CloudQueueMessage peekedMessage = queue.PeekMessage();

            // Display message.
            Console.WriteLine(peekedMessage.AsString);

            // Get the message from the queue and update the message contents.
            message = queue.GetMessage();
            message.SetMessageContent2("Updated contents.", false);
            queue.UpdateMessage(message,
                TimeSpan.FromSeconds(60.0),  // Make it invisible for another 60 seconds.
                MessageUpdateFields.Content | MessageUpdateFields.Visibility);

            // Get the next message
            CloudQueueMessage retrievedMessage = queue.GetMessage();

            //Process the message in less than 30 seconds, and then delete the message
            queue.DeleteMessage(retrievedMessage);
        }
    }
}
