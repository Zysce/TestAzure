using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Azure.Management.Batch.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestAzure
{
    class Program
    {
        private const string _path = @"C:\Users\ALEROUGE\Documents\Projects\AzureBatchTest\TestAzure\azureauth.properties";
        private static readonly Region _region = Region.USEast;

        static void Main(string[] args)
        {
            //var credentials = SdkContext.AzureCredentialsFactory
            //.FromFile(_path);

            //var azure = Azure
            //    .Configure()
            //    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
            //    .Authenticate(credentials)
            //    .WithDefaultSubscription();

            //Create(azure);

            Job().GetAwaiter().GetResult();
        }

        private static void Create(IAzure azure)
        {
            int allowedNumberOfBatchAccounts = azure.BatchAccounts.GetBatchAccountQuotaByLocation(_region);

            // ===========================================================
            // List all the batch accounts in subscription.

            var batchAccounts = azure.BatchAccounts.List().ToList();
            int batchAccountsAtSpecificRegion = batchAccounts.Count(x => x.Region == _region);


            var batchAccount = batchAccounts.Single(x => x.Name == "batchacccodetestal");

        }

        private const string _url = "https://batchacccodetestal.eastus.batch.azure.com";
        private const string _accName = "batchacccodetestal";
        private const string _accKey = "U2N742E0ht6Hq/onyfrtlL3atue3gSBzrf5Yp6cPb/NpXWQ2bko4hI2o/YrUforAkONOgq8uOqgg7281tBKOXQ==";
        private static async Task Job()
        {
            var creds = new BatchSharedKeyCredentials(_url, _accName, _accKey);

            using (var batchClient = BatchClient.Open(creds))
            {
                // add a retry policy. The built-in policies are No Retry (default), Linear Retry, and Exponential Retry
                batchClient.CustomBehaviors.Add(RetryPolicyProvider.ExponentialRetryProvider(TimeSpan.FromSeconds(5), 3));

                string jobId = "testjobalHelloWorldJob";

                CloudJob unboundJob = batchClient.JobOperations.CreateJob();
                unboundJob.Id = jobId;

                // For this job, ask the Batch service to automatically create a pool of VMs when the job is submitted.
                unboundJob.PoolInformation = new PoolInformation()
                {
                    AutoPoolSpecification = new AutoPoolSpecification()
                    {
                        AutoPoolIdPrefix = "HelloWorld",
                        PoolSpecification = new PoolSpecification()
                        {
                            TargetDedicatedComputeNodes = 2,
                            CloudServiceConfiguration = new Microsoft.Azure.Batch.CloudServiceConfiguration("6"),
                            VirtualMachineSize = "standard_d1_v2"
                        },
                        KeepAlive = false,
                        PoolLifetimeOption = PoolLifetimeOption.Job
                    }
                };

                // Commit Job to create it in the service
                await unboundJob.CommitAsync();

                // create a simple task. Each task within a job must have a unique ID
                await batchClient.JobOperations.AddTaskAsync(jobId, new CloudTask("task1", "cmd /c echo Hello world from the Batch Hello world sample!"));


                //for (int i = 0; i < inputFiles.Count; i++)
                //{
                //    string taskId = String.Format("Task{0}", i);

                //    // Define task command line to convert each input file.
                //    string appPath = String.Format("%AZ_BATCH_APP_PACKAGE_{0}#{1}%", appPackageId, appPackageVersion);
                //    string inputMediaFile = inputFiles[i].FilePath;
                //    string outputMediaFile = String.Format("{0}{1}",
                //        System.IO.Path.GetFileNameWithoutExtension(inputMediaFile),
                //        ".mp3");
                //    string taskCommandLine = String.Format("cmd /c {0}\\ffmpeg-3.4-win64-static\\bin\\ffmpeg.exe -i {1} {2}", appPath, inputMediaFile, outputMediaFile);

                //    // Create a cloud task (with the task ID and command line)
                //    CloudTask task = new CloudTask(taskId, taskCommandLine);
                //    task.ResourceFiles = new List<ResourceFile> { inputFiles[i] };

                //    // Task output file
                //    List<OutputFile> outputFileList = new List<OutputFile>();
                //    OutputFileBlobContainerDestination outputContainer = new OutputFileBlobContainerDestination(outputContainerSasUrl);
                //    OutputFile outputFile = new OutputFile(outputMediaFile,
                //       new OutputFileDestination(outputContainer),
                //       new OutputFileUploadOptions(OutputFileUploadCondition.TaskSuccess));
                //    outputFileList.Add(outputFile);
                //    task.OutputFiles = outputFileList;
                //    tasks.Add(task);
                //}


                TaskStateMonitor taskStateMonitor = batchClient.Utilities.CreateTaskStateMonitor();

                List<CloudTask> ourTasks = await batchClient.JobOperations.ListTasks(jobId).ToListAsync();

                // Wait for all tasks to reach the completed state.
                // If the pool is being resized then enough time is needed for the nodes to reach the idle state in order
                // for tasks to run on them.
                await taskStateMonitor.WhenAll(ourTasks, TaskState.Completed, TimeSpan.FromMinutes(10));

                // dump task output
                foreach (CloudTask t in ourTasks)
                {
                    Console.WriteLine("Task {0}", t.Id);

                    //Read the standard out of the task
                    NodeFile standardOutFile = await t.GetNodeFileAsync(Constants.StandardOutFileName);
                    string standardOutText = await standardOutFile.ReadAsStringAsync();
                    Console.WriteLine("Standard out:");
                    Console.WriteLine(standardOutText);

                    Console.WriteLine();
                }
            }
        }
    }
}
