using System;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using Apprenda.Services.Logging;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    public class AzureStorageAddonImpl : AddonBase
    {
        private static readonly ILogger Log = LogManager.Instance().GetLogger(typeof(AzureStorageAddonImpl));

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("");
            var manifest = request.Manifest;
            try
            {
                var devOptions = DeveloperParameters.Parse(request.DeveloperParameters, request.Manifest.GetProperties());
                // establish MSFT Azure Storage client

                var client = EstablishClient(manifest, devOptions);

                
                if (devOptions.StorageAccountName == null)
                {
                    throw new ArgumentNullException(devOptions.StorageAccountName);
                }


                var nameIsAvailable = client.StorageAccounts.CheckNameAvailability(devOptions.StorageAccountName);
                if (nameIsAvailable.IsAvailable && devOptions.NewStorageAccountFlag)//if the name is available, this means that the StorageAccountName should be created because it is unique
                { 
                    var parameters = CreateStorageAccountParameters(devOptions);
                    var mResponse = client.StorageAccounts.Create(parameters);

                    do
                    {
                        var verificationResponse = client.StorageAccounts.Get(parameters.Name);

                        if (verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Created))
                        {
                            var azureconnectioninfo = client.StorageAccounts.Get(devOptions.StorageAccountName);
                            var keysForStorageUnit = client.StorageAccounts.GetKeys(devOptions.StorageAccountName);

                            var connectionInfo = new ConnectionInfo
                            {
                                PrimaryKey = keysForStorageUnit.PrimaryKey,
                                SecondaryKey = keysForStorageUnit.SecondaryKey,
                                StorageAccountName = azureconnectioninfo.StorageAccount.Name,
                                Uri = keysForStorageUnit.Uri.ToString(),
                                BlobContainerName = null  //even if they provide this, we want to save it as null so we can use this as criteria so delete the storage account when deprovisioning takes place
                            };
                            provisionResult.ConnectionData = connectionInfo.ToString();
                            provisionResult.IsSuccess = true;
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));
                    }
                    while (true);
                }

                else if(nameIsAvailable.IsAvailable && !devOptions.NewStorageAccountFlag)  //it appears that the user would like to create a blob, but has input a StorageAccount that already exists
                {
                    provisionResult.EndUserMessage += "Invalid Configuration. The StorageAccountName given does not exist,\n"
                                                   + "so a blob container cannot be created.  Try again with the name of an existing Storage Container (if you wish to create a blob)\n";
                    provisionResult.IsSuccess = false;
                }

                else if (!nameIsAvailable.IsAvailable && devOptions.NewStorageAccountFlag) //this should be invalid; the user wants us to create a new storage account even though the name they gave us is not unique
                {
                    provisionResult.EndUserMessage += "Invalid Configuration. The StorageAccountName given is not available.\n"
                                                   + "An account with this name may already exist.  Try again with a different StorageAccountName\n";
                    provisionResult.IsSuccess = false;
                }

                else if (!nameIsAvailable.IsAvailable && !devOptions.NewStorageAccountFlag) //this means that there is already an account with this name, and the user doesn't want to create a new Storage Account.  They may want to create a blob
                {
                    if (String.IsNullOrEmpty(devOptions.ContainerName)) //if the container name is null or empty, this is an invalid option, because there is nothing for us to do
                    {
                        provisionResult.EndUserMessage += "Invalid Configuration. It seems you have tried to create a blob,\n"
                                                   + "but you have not specified a container name. Please specify a container name, or check your configuration and try again\n";
                        provisionResult.IsSuccess = false;
                    }

                    else //now we create the blob
                    {
                        var keys = client.StorageAccounts.GetKeys(devOptions.StorageAccountName);
                        CloudStorageAccount account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https" +
                                                                                ";AccountName=" + devOptions.StorageAccountName +
                                                                                ";AccountKey=" + keys.PrimaryKey
                                                                                + ";");  //we create the connection string here, which should allow us to connect to the storage account

                        ConnectionInfo info = AzureStorageFactory.CreateBlobContainer(account, devOptions.ContainerName);
                        
                        if (String.IsNullOrEmpty(info.ErrorMessage)) //this means there is no error
                        {
                            provisionResult.IsSuccess = true;
                            info.PrimaryKey = keys.PrimaryKey;
                            info.SecondaryKey = keys.SecondaryKey;
                            info.StorageAccountName = devOptions.StorageAccountName;
                            provisionResult.ConnectionData = info.ToString();
                            provisionResult.EndUserMessage = "Successfully created blob container\n";
                        }
                        else //this means there is an error, so it failed
                        {
                            provisionResult.IsSuccess = false;
                            provisionResult.EndUserMessage = "Could not create blob container:\n" + info.ErrorMessage;
                        }
                    }


                }
           
                
            }
            catch (Exception e)
            {
                provisionResult.IsSuccess = false;
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        private static StorageAccountCreateParameters CreateStorageAccountParameters(DeveloperParameters developerOptions)
        {
            var parameters = new StorageAccountCreateParameters
            {
                Description = developerOptions.Description,
                //GeoReplicationEnabled = developerOptions.GeoReplicationEnabled,
                Label = developerOptions.StorageAccountName,
                Name = developerOptions.StorageAccountName,
                //AccountType = "Standard_LRS"     //hardcoded for now, possibly add a dev option/parameter to change th
            };
            if (developerOptions.AffinityGroup != null)
            {
                parameters.AffinityGroup = developerOptions.AffinityGroup;
            }
            else if (developerOptions.Location != null)
            {
                parameters.Location = developerOptions.Location;
            }
            else
            {
                throw new ArgumentException("Must have a value for either AffinityGroup or Location. Please verify your settings in the manifest file.");
            }
            if (developerOptions.ReplicationType != null)
            {
                parameters.AccountType = developerOptions.ReplicationType;
            }
            else
            {
                throw new ArgumentException("You must specify a Replication Type. Please verify your settings in the manifest file.");
            }
            return parameters;
        }

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var devOptions = DeveloperParameters.Parse(request.DeveloperParameters, request.Manifest.GetProperties());
            ConnectionInfo connectionInfo = ConnectionInfo.Parse(connectionData); //make a new connection info object using the Parse method.
            var creds = CertificateAuthenticationHelper.GetCredentials(devOptions.AzureManagementSubscriptionId, "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");
            // set up the storage management client
            var client = new StorageManagementClient(creds);
            
            // AI-121 & AI-122
            // we're going to have to implement some additional handling here, including parsing of the connection data
            // i strongly recommend we look at putting this in json
            try
            {
                if (!String.IsNullOrEmpty(connectionInfo.BlobContainerName)) //if there is a blob container name in the connection info, this implies a blob container was made previously
                {
                   CloudStorageAccount account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https" +
                                                                            ";AccountName=" + connectionInfo.StorageAccountName +
                                                                            ";AccountKey=" + connectionInfo.PrimaryKey
                                                                            + ";");

                    String error = AzureStorageFactory.DeleteBlobContainer(account, connectionInfo.BlobContainerName).ErrorMessage; //we don't need anything from the storage factory except the error message.  Perhaps we should change the return type of the Factory Delete methods

                    if (String.IsNullOrEmpty(error))
                    {
                        deprovisionResult.IsSuccess = true;
                        deprovisionResult.EndUserMessage = "Successfully deleted blob container";
                    }

                    else
                    {
                        deprovisionResult.IsSuccess = false;
                        deprovisionResult.EndUserMessage = "Could not delete blob container:\n" + error;
                    }

                }

                else //this means that the blob name wasn't provided, so a storage account must have been created instead.
                {
                    
                    // then if requested, delete the storage account name
                    Log.Error("MATTAZURE: Deleting storage account name...\n");
                    var nameIsAvailable = client.StorageAccounts.CheckNameAvailability(connectionInfo.StorageAccountName);
                    Log.Error("MATTAZURE: is available?" + nameIsAvailable.IsAvailable + "\n");
                    if (!nameIsAvailable.IsAvailable) //if the name isn't available, this means that a storage account with this name exists, and we should go ahead and delete it
                    { 
                        var mResponse = client.StorageAccounts.Delete(connectionInfo.StorageAccountName);
                        Log.Error("MATTAZURE: " + mResponse.StatusCode.ToString() + "\n");
                        Log.Error("MATTAZURE: " + mResponse.ToString() + "\n");
                        nameIsAvailable = client.StorageAccounts.CheckNameAvailability(connectionInfo.StorageAccountName); //check if the name is now available. If so, it was deleted
                        if (nameIsAvailable.IsAvailable)
                        {
                            deprovisionResult.EndUserMessage = "Successfully deleted storage account";
                            deprovisionResult.IsSuccess = true;
                        }
                        else
                        {
                            deprovisionResult.EndUserMessage = "Could not delete storage account";
                            deprovisionResult.IsSuccess = false;
                        }
                    }

                    else //this would mean that the storage account didn't exist beforehand, so there is no way for us to delete it
                    {
                        deprovisionResult.EndUserMessage = "The storage account you are trying to delete does not exist.";
                        deprovisionResult.IsSuccess = false;
                    }
                    
                }
            }
            catch (Exception e)
            {
                Log.Error("MATTAZURE: Error:" + e.Message + "\n");
                deprovisionResult.IsSuccess = false;
                deprovisionResult.EndUserMessage = e.Message;
                return deprovisionResult;
            }
            return deprovisionResult;
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            var manifest = request.Manifest;
            var developerParams = request.DeveloperParameters;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";
            var manifestProperties = manifest.Properties;

            if (manifestProperties != null && manifestProperties.Any())
            {
                var devOptions = DeveloperParameters.Parse(developerParams, manifest.GetProperties());
                try
                {
                    testProgress += "Establishing connection to Azure...\n";
                    // set up the credentials for azure

                    var client = EstablishClient(manifest, devOptions);

                    var listOfStorageAccounts = client.StorageAccounts.List();

                    testProgress += string.Format("Number of Accounts: '{0}'\n", listOfStorageAccounts.Count());

                    testProgress += "Successfully passed all testing criteria!";
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = testProgress;
                }
                catch (Exception e)
                {
                    // adding a forced failure here.
                    testResult.IsSuccess = false;
                    testResult.EndUserMessage = testProgress + "\nEXCEPTION: " + e.Message;
                }
            }
            else
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage = "Missing required manifest properties (requireDevCredentials)";
            }

            return testResult;
        }

        private static StorageManagementClient EstablishClient(AddonManifest manifest, DeveloperParameters devOptions)
        {
            var testProgress = "Parsing manifest...\n";
            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            testProgress += "Getting credentials...\n";
            // set up the credentials for azure
            testProgress += "Sub ID is: " + manifestprops["AzureManagementSubscriptionID"] + "\n";
            testProgress += "Auth key is: " + manifestprops["AzureAuthenticationKey"] + "\n";
            var creds = Azure.CertificateAuthenticationHelper.GetCredentials(manifestprops["AzureManagementSubscriptionID"], /*manifestprops["AzureAuthenticationKey"]*/"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");
            // set up the storage management client
            var client = new StorageManagementClient(creds);
            testProgress += "Successfully returned credentials.\n";
            return client;
        }
    }
}
