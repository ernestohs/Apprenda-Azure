using System;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;

namespace Apprenda.SaaSGrid.Addons.Azure.ServiceBus
{
    public class ServiceBusAddon : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("");
            var manifest = request.Manifest;
            var inputDevParams = request.DeveloperParameters;
            try
            {
                // parse required options here, use developer options class to do so.
                var manifestProperties = manifest.GetProperties();
                // Developer Options will be instantiated first time here (hence, null).
                var devParams = DeveloperParameters.Parse(inputDevParams,manifestProperties);
                // establish MSFT Azure Storage client
                var client = EstablishClient(devParams);

                // ok now we need to understand what the developer wants to do.
                // ------------------------------------------------------------
                // logic:
                //    - if the developer wishes to create a storage account, we go that route first
                //    - if a storage account exists, test it (including above)
                //    - create the blob container
                // ------------------------------------------------------------
                var parameters = CreateStorageAccountParameters(devParams);
                var mResponse = client.StorageAccounts.Create(parameters);
                do
                {
                    StorageAccountGetResponse verificationResponse = client.StorageAccounts.Get(parameters.Name);

                    if (verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Created))
                    {
                        StorageAccountGetResponse azureconnectioninfo =
                            client.StorageAccounts.Get(devParams.StorageAccountName);
                        StorageAccountGetKeysResponse keysForStorageUnit =
                            client.StorageAccounts.GetKeys(devParams.StorageAccountName);

                        var connectionInfo = new ConnectionInfo
                        {
                            PrimaryKey = keysForStorageUnit.PrimaryKey,
                            SecondaryKey = keysForStorageUnit.SecondaryKey,
                            StorageAccountName = azureconnectioninfo.StorageAccount.Name,
                            URI = keysForStorageUnit.Uri.ToString()
                        };
                        provisionResult.ConnectionData = connectionInfo.ToString();
                        // deprovision request of storage account was successful.
                        provisionResult.IsSuccess = true;
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(10d));
                } while (true);
            }
            catch (Exception e)
            {
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        private StorageAccountCreateParameters CreateStorageAccountParameters(DeveloperParameters developerOptions)
        {
            var parameters = new StorageAccountCreateParameters
            {
                Description = developerOptions.Description,
                //GeoReplicationEnabled = developerOptions.GeoReplicationEnabled,
                // for now. not sure what the label entails.
                Label = developerOptions.StorageAccountName,
                Name = developerOptions.StorageAccountName
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
                throw new ArgumentException(
                    "Must have a value for either AffinityGroup or Location. Please verify your settings in the manifest file.");
            }
            return parameters;
        }

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = request.Manifest;
            var inputDevParameters = request.DeveloperParameters;
            // parse required options here, use developer options class to do so.
            var manifestProperties = manifest.GetProperties();
            // Developer Options will be instantiated first time here.
            var devParams = DeveloperParameters.Parse(inputDevParameters, manifestProperties);

            // set up the credentials for azure
            var creds = CertificateAuthenticationHelper.GetCredentials(devParams.AzureManagementSubscriptionId,
                    devParams.AzureAuthenticationKey);
            // set up the storage management client
            var client = new StorageManagementClient();

            var mResponse = client.StorageAccounts.Delete(devParams.StorageAccountName);
            if (mResponse.StatusCode.Equals(HttpStatusCode.OK))
            {
                do
                {
                    var verificationResponse =
                        client.StorageAccounts.Get(devParams.StorageAccountName);

                    if (verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Deleting))
                    {
                        deprovisionResult.IsSuccess = true;
                        deprovisionResult.EndUserMessage =
                            "Deprovision Request Complete, please allow a few minutes for resources to be fully deleted.";
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(10d));
                } while (true);
                return deprovisionResult;
            }
            else
            {
                return new OperationResult()
                {
                    EndUserMessage = "Azure Query failed. Please check your syntax and credetials.",
                    IsSuccess = false
                };
            }
        }
        

        public override OperationResult Test(AddonTestRequest request)
        {
            var manifest = request.Manifest;
            var inputDevParams = request.DeveloperParameters;
            var testResult = new OperationResult {IsSuccess = false};
            var testProgress = "";
            var manifestProperties = manifest.Properties;

            if (manifestProperties != null && manifestProperties.Any())
            {
                var devParams = DeveloperParameters.Parse(inputDevParams, manifestProperties);
                try
                {
                    testProgress += "Establishing connection to Azure...\n";
                    // set up the credentials for azure

                    var client = EstablishClient(devParams);
                    
                    var listOfStorageAccounts = client.StorageAccounts.List();

                    testProgress += "Number of Accounts: '{listOfStorageAccounts.Count()}'";

                    testProgress += "Successfully passed all testing criteria!";
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = testProgress;
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = e.Message;
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties (requireDevCredentials)";
            }

            return testResult;
        }

        private static StorageManagementClient EstablishClient(DeveloperParameters devOptions)
        {
            // set up the credentials for azure
            var creds =
                CertificateAuthenticationHelper.GetCredentials(devOptions.AzureManagementSubscriptionId,
                    devOptions.AzureAuthenticationKey);
            // set up the storage management client
            var client = new StorageManagementClient(creds);
            return client;
        }
    }
}