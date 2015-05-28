using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    class AzureStorageFactory
    {
        // AI-121
        internal static ConnectionInfo CreateBlobContainer(CloudStorageAccount account, String containerName)
        {
            CloudBlobClient client;
            CloudBlobContainer container;
            bool containerCreated;
            ConnectionInfo info = new ConnectionInfo();


            try
            {
                client = account.CreateCloudBlobClient(); //make a blob service client
            }
             catch (Exception e)
             {
                 info.ErrorMessage = "Error when attempting to create blob client:\n" + e.Message;
                 return info;
            }
            
            
            try
            {
                container = client.GetContainerReference(containerName); //get a reference to a container with the given name
                containerCreated = container.CreateIfNotExists(); //if the container doesn't exist, create it.  We store the boolean result of this so we can handle if it doesn't get created (in case it doesn't also throw an exception here)
            }
            catch (Exception e)
            {
                info.ErrorMessage = "Error when attempting to create blob container:\n" + e.Message;
                return info;

            }

            if (!containerCreated) //check if the container got created. If not:
            { 
                info.ErrorMessage = "Blob container was not successfully created\n";
                return info;
            }

            //if we passed all these steps, it should have successfully created a container
            info.BlobContainerName = containerName;
            info.ErrorMessage = null; //just to ensure the error message is null
            return info;

        }

        internal static ConnectionInfo DeleteBlobContainer(CloudStorageAccount account, String containerName) //pretty much the same as CreateBlobContainer
        {
            CloudBlobClient client;
            CloudBlobContainer container;
            bool containerDeleted;
            ConnectionInfo info = new ConnectionInfo();


            try
            {
                client = account.CreateCloudBlobClient(); //make a blob service client
            }
            catch (Exception e)
            {
                info.ErrorMessage = "Error when attempting to create blob client:\n" + e.Message;
                return info;
            }


            try
            {
                container = client.GetContainerReference(containerName); //get a reference to a container with the given name
                containerDeleted = container.DeleteIfExists(); //delete if it exists
            }
            catch (Exception e)
            {
                info.ErrorMessage = "Error when attempting to delete blob container:\n" + e.Message;
                return info;

            }

            if (!containerDeleted) //check if the container got deleted. If not:
            {
                info.ErrorMessage = "Blob container was not successfully deleted\n";
                return info;
            }

            //if we passed all these steps, it should have successfully deleted the container
            info.BlobContainerName = containerName;
            info.ErrorMessage = null; //just to ensure the error message is null
            return info;

        
        }

        internal static ConnectionInfo CreateQueue(CloudStorageAccount account, String containerName) //gonna have to implement handling of which type of container the user wants to be created
        {
            CloudQueueClient client;
            CloudQueue queue;
            bool queueCreated;
            ConnectionInfo info = new ConnectionInfo();


            try
            {
                client = account.CreateCloudQueueClient(); //make a queue client
            }
            catch (Exception e)
            {
                info.ErrorMessage = "Error when attempting to create queue client:\n" + e.Message;
                return info;
            }


            try
            {
                queue = client.GetQueueReference(containerName); //get a reference to a container with the given name
                queueCreated = queue.CreateIfNotExists(); //if the queue doesn't exist, create it.  We store the boolean result of this so we can handle if it doesn't get created (in case it doesn't also throw an exception here)
            }
            catch (Exception e)
            {
                info.ErrorMessage = "Error when attempting to create queue:\n" + e.Message;
                return info;

            }

            if (!queueCreated) //check if the queue got created. If not:
            {
                info.ErrorMessage = "Queue was not successfully created\n";
                return info;
            }

            //if we passed all these steps, it should have successfully created a container
            info.BlobContainerName = containerName;
            info.ErrorMessage = null; //just to ensure the error message is null
            return info;
        }

        internal static OperationResult DeleteQueue(DeveloperParameters developerParameters)
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            CloudQueueClient client;
            CloudQueue queue;
            bool queueDeleted;

            try
            {
                account = CloudStorageAccount.Parse(""/*need a connection string here*/); //get the account
                result.EndUserMessage += "Account accessed successfully\n";
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            result.EndUserMessage += "Accessing Queue Client\n";
            try
            {
                client = account.CreateCloudQueueClient(); //make a queue client
                result.EndUserMessage += "Successfully accessed Queue Client\n";
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            result.EndUserMessage += "Deleting queue\n";

            try
            {
                queue = client.GetQueueReference(developerParameters.ContainerName); //get a reference to a container with the given name
                queueDeleted = queue.DeleteIfExists(); //if the queue exists, delete it.
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            if (!queueDeleted)
            { //check if the queue got deleted. If not:
                result.EndUserMessage += "Queue was not deleted.  A queue with this name may not already exist\n";
                result.IsSuccess = false;
                return result;
            }

            //if we passed all these steps, it should have successfully created a container
            result.EndUserMessage += "Queue deleted successfully\n";
            result.IsSuccess = true;
            return result;
        }

        internal static OperationResult CreateTable(DeveloperParameters developerParameters)
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            CloudTableClient client;
            CloudTable table;
            bool tableCreated;

            try
            {
                account = CloudStorageAccount.Parse(""/*need a connection string here*/); //get the account
                result.EndUserMessage += "Account accessed successfully\n";
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            result.EndUserMessage += "Accessing Table Client\n";
            try
            {
                client = account.CreateCloudTableClient(); //make a table client
                result.EndUserMessage += "Successfully accessed Table Client\n";
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            result.EndUserMessage += "Creating table\n";

            try
            {
                table = client.GetTableReference(developerParameters.ContainerName); //get a reference to a table with the given name
                tableCreated = table.CreateIfNotExists(); //if the table doesn't exist, create it.  We store the boolean result of this so we can handle if it doesn't get created (in case it doesn't also throw an exception here)
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            if (!tableCreated)
            { //check if the queue got created. If not:
                result.EndUserMessage += "Table was not created.  A table with this name may already exist\n";
                result.IsSuccess = false;
                return result;
            }

            //if we passed all these steps, it should have successfully created a table
            result.EndUserMessage += "Table created successfully\n";
            result.IsSuccess = true;
            return result;
        }

        internal static OperationResult DeleteTable(DeveloperParameters developerParameters)
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            CloudTableClient client;
            CloudTable table;
            bool tableDeleted;

            try
            {
                account = CloudStorageAccount.Parse(""/*need a connection string here*/); //get the account
                result.EndUserMessage += "Account accessed successfully\n";
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            result.EndUserMessage += "Accessing Table Client\n";
            try
            {
                client = account.CreateCloudTableClient(); //make a table client
                result.EndUserMessage += "Successfully accessed Table Client\n";
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            result.EndUserMessage += "Deleting table\n";

            try
            {
                table = client.GetTableReference(developerParameters.ContainerName); //get a reference to a table with the given name
                tableDeleted = table.DeleteIfExists(); //if the table exists, delete it.  We store the boolean result of this so we can handle if it doesn't get created (in case it doesn't also throw an exception here)
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            if (!tableDeleted)
            { //check if the table got deleted. If not:
                result.EndUserMessage += "Table was not deleted.  A table with this name may not already exist\n";
                result.IsSuccess = false;
                return result;
            }

            //if we passed all these steps, it should have successfully deleted
            result.EndUserMessage += "Table deleted successfully\n";
            result.IsSuccess = true;
            return result;
        }


    }
}
