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
        internal static OperationResult CreateBlobContainer(DeveloperParameters developerParameters)
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            CloudBlobClient client;
            CloudBlobContainer container;
            bool containerCreated;
            result.EndUserMessage += "Attempting to access Storage account\n";

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

            result.EndUserMessage += "Accessing Blob Client\n";
            try
            {
                client = account.CreateCloudBlobClient(); //make a blob service client
                result.EndUserMessage += "Successfully accessed Blob Client\n";
            }
             catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }
            
            result.EndUserMessage += "Creating blob container\n";
            
            try
            {
            container = client.GetContainerReference(developerParameters.ContainerName); //get a reference to a container with the given name
            containerCreated = container.CreateIfNotExists(); //if the container doesn't exist, create it.  We store the boolean result of this so we can handle if it doesn't get created (in case it doesn't also throw an exception here)
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }
            
            if(!containerCreated){ //check if the container got created. If not:
                result.EndUserMessage += "Container was not created.  A container with this name may already exist\n";
                result.IsSuccess = false;
                return result;
            }

            //if we passed all these steps, it should have successfully created a container
            result.EndUserMessage += "Blob created successfully\n";
            result.IsSuccess = true;
            return result;

        }

        internal static OperationResult DeleteBlobContainer(DeveloperParameters developerParameters) //pretty much the same as CreateBlobContainer
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            CloudBlobClient client;
            CloudBlobContainer container;
            bool containerDeleted;

            result.EndUserMessage += "Attempting to access Storage account\n";
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

            result.EndUserMessage += "Accessing Blob Client\n";
            try
            {
                client = account.CreateCloudBlobClient(); //make a blob service client
                result.EndUserMessage += "Successfully accessed Blob Client\n";
            }
             catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }
            
            result.EndUserMessage += "Deleting blob container\n";
            
            try
            {
            container = client.GetContainerReference(developerParameters.ContainerName); //get a reference to a container with the given name
            containerDeleted = container.DeleteIfExists(); //if the container exists, delete it.  We store the boolean result of this so we can handle if it doesn't get deleted (in case it doesn't also throw an exception here)
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }
            
            if(!containerDeleted){ //check if the container got deleted. If not:
                result.EndUserMessage += "Container was not deleted.  A container with this name may not currently exist\n";
                result.IsSuccess = false;
                return result;
            }

            //if we passed all these steps, it should have successfully deleted the container
            result.EndUserMessage += "Blob deleted successfully\n";
            result.IsSuccess = true;
            return result;

        
        }

        internal static OperationResult CreateQueue(DeveloperParameters developerParameters)
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            CloudQueueClient client;
            CloudQueue queue;
            bool queueCreated;

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

            result.EndUserMessage += "Creating queue\n";

            try
            {
                queue = client.GetQueueReference(developerParameters.ContainerName); //get a reference to a queue with the given name
                queueCreated = queue.CreateIfNotExists(); //if the queue doesn't exist, create it.  We store the boolean result of this so we can handle if it doesn't get created (in case it doesn't also throw an exception here)
            }
            catch (Exception e)
            {
                result.EndUserMessage += e.Message;
                result.IsSuccess = false;
                return result;
            }

            if (!queueCreated)
            { //check if the queue got created. If not:
                result.EndUserMessage += "Queue was not created.  A queue with this name may already exist\n";
                result.IsSuccess = false;
                return result;
            }

            //if we passed all these steps, it should have successfully created a queue
            result.EndUserMessage += "Queue created successfully\n";
            result.IsSuccess = true;
            return result;
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
