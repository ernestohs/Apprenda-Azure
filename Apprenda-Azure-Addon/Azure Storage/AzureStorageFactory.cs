using System;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    class AzureStorageFactory
    {
        // AI-121
        internal static OperationResult CreateBlobContainer(DeveloperParameters developerParameters)
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            result.EndUserMessage += "Parsing Connection String, attempting to connect to Storage\n";

            try
            {
                account =  //not sure how correct this connection string is, especially around the "AccountKey" part
                    CloudStorageAccount.Parse("DefaultEndpointsProtocol=[http];" + "AccountName={0};" + developerParameters.StorageAccountName + "AccountKey={1}" + developerParameters.AzureAuthenticationKey);
            }
            catch (Exception e)
            {
                result.EndUserMessage += ("Exception:\n" + e.Message);
                result.IsSuccess = false;
                return result;
            }
            result.EndUserMessage += "\nConnection String Parsed\n";

            
            CloudBlobClient client;
            result.EndUserMessage += "\nCreating Blob Client\n";
            try
            {
                client = account.CreateCloudBlobClient();
            }
            catch (Exception e)
            {
                result.EndUserMessage += ("Exception:\n" + e.Message);
                result.IsSuccess = false;
                return result;
            }
            result.EndUserMessage = "\nCreated Blob Client\n";


            CloudBlobContainer container;
            result.EndUserMessage = "\nCreating Blob Container\n";
            try
            {
                container = client.GetContainerReference("mhow"/*gonna need the container name here*/);
                container.CreateIfNotExists();
            }
            catch (Exception e)
            {
                result.EndUserMessage += ("Exception:\n" + e.Message);
                result.IsSuccess = false;
                return result;
            }
            result.EndUserMessage += "\nCreated Blob Container\n";


            result.EndUserMessage += "\nSuccess!\n";
            result.IsSuccess = true;
            return result;
        }

        internal static OperationResult DeleteBlobContainer(DeveloperParameters developerParameters)
        {
            OperationResult result = new OperationResult();
            CloudStorageAccount account;
            result.EndUserMessage += "Parsing Connection String, attempting to connect to Storage\n";

            try
            {
                account =  //not sure how correct this connection string is, especially around the "AccountKey" part
                    CloudStorageAccount.Parse("DefaultEndpointsProtocol=[http];" + "AccountName={0};" + developerParameters.StorageAccountName + "AccountKey={1}" + developerParameters.AzureAuthenticationKey);
            }
            catch (Exception e)
            {
                result.EndUserMessage += ("Exception:\n" + e.Message);
                result.IsSuccess = false;
                return result;
            }
            result.EndUserMessage += "\nConnection String Parsed\n";


            CloudBlobClient client;
            result.EndUserMessage += "\nCreating Blob Client\n";
            try
            {
                client = account.CreateCloudBlobClient();
            }
            catch (Exception e)
            {
                result.EndUserMessage += ("Exception:\n" + e.Message);
                result.IsSuccess = false;
                return result;
            }
            result.EndUserMessage = "\nCreated Blob Client\n";


            CloudBlobContainer container;
            result.EndUserMessage = "\nDeleting Blob Container\n";
            try
            {
                container = client.GetContainerReference("mhowa"/*gonna need the container name here*/);
                container.DeleteIfExists();
            }
            catch (Exception e)
            {
                result.EndUserMessage += ("Exception:\n" + e.Message);
                result.IsSuccess = false;
                return result;
            }
            result.EndUserMessage += "\nDeleted Blob Container\n";


            result.EndUserMessage += "\nSuccess!\n";
            result.IsSuccess = true;
            return result;
        }

        internal static OperationResult CreateQueue(DeveloperParameters developerParameters)
        {
            throw new NotImplementedException();
        }

        internal static OperationResult DeleteQueue(DeveloperParameters developerParameters)
        {
            throw new NotImplementedException();
        }

        internal static OperationResult CreateTable(DeveloperParameters developerParameters)
        {
            throw new NotImplementedException();
        }

        internal static OperationResult DeleteTable(DeveloperParameters developerParameters)
        {
            throw new NotImplementedException();
        }
    }
}
