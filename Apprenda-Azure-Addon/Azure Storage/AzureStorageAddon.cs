using System;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using OperationStatusResponse = Microsoft.Azure.OperationStatusResponse;
using SubscriptionCloudCredentials = Microsoft.Azure.SubscriptionCloudCredentials;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    public class AzureStorageAddonImpl : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("");
            var manifest = request.Manifest;
            try
            {
                var devOptions = DeveloperParameters.Parse(request.DeveloperParameters, request.Manifest.GetProperties());
                // establish MSFT Azure Storage client
                var filler = "";

                var client = EstablishClient(manifest, devOptions, ref filler);

                var uniqueIdIfNeeded = 0;
                if (devOptions.StorageAccountName == null)
                {
                    throw new ArgumentNullException(devOptions.StorageAccountName);
                }
                var nameIsAvailable = client.StorageAccounts.CheckNameAvailability(devOptions.StorageAccountName);
                // as might be the case, this will increment and append a numeral onto the end of the storage account name in order to uniquely qualify it.
                while (!nameIsAvailable.IsAvailable)
                {
                    devOptions.StorageAccountName = string.Concat(devOptions.StorageAccountName, uniqueIdIfNeeded++);
                    nameIsAvailable = client.StorageAccounts.CheckNameAvailability(devOptions.StorageAccountName);
                }
                StorageAccountCreateParameters parameters = CreateStorageAccountParameters(devOptions);
                OperationStatusResponse mResponse = client.StorageAccounts.Create(parameters);

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
                            Uri = keysForStorageUnit.Uri.ToString()
                        };
                        provisionResult.ConnectionData = connectionInfo.ToString();
                        provisionResult.IsSuccess = true;
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(10d));
                }
                while (true);
            }
            catch (Exception e)
            {
                provisionResult.IsSuccess = false;
                provisionResult.EndUserMessage = e.Message + "\n We're in an error\n";
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
                throw new ArgumentException("Must have a value for either AffinityGroup or Location. Please verify your settings in the manifest file.");
            }
            return parameters;
        }

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var devOptions = DeveloperParameters.Parse(request.DeveloperParameters, request.Manifest.GetProperties());
            // set up the credentials for azure
            SubscriptionCloudCredentials creds = CertificateAuthenticationHelper.getCredentials(devOptions.AzureManagementSubscriptionId, devOptions.AzureAuthenticationKey);
            // set up the storage management client
            var client = new StorageManagementClient(creds);


            // AI-121 & AI-122
            // we're going to have to implement some additional handling here, including parsing of the connection data
            // i strongly recommend we look at putting this in json


            // then if requested, delete the storage account name
            var mResponse = client.StorageAccounts.Delete(devOptions.StorageAccountName);

            do
            {
                var verificationResponse = client.StorageAccounts.Get(devOptions.StorageAccountName);

                if (verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Deleting))
                {
                    deprovisionResult.IsSuccess = true;
                    deprovisionResult.EndUserMessage = string.Format("Deprovision Request Complete, please allow a few minutes for resources to be fully deleted.");
                    break;
                }
                Thread.Sleep(TimeSpan.FromSeconds(10d));
            }
            while (true);
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

                    var client = EstablishClient(manifest, devOptions, ref testProgress);

                    var listOfStorageAccounts = client.StorageAccounts.List();

                    testProgress += string.Format("Number of Accounts: '{0}'", listOfStorageAccounts.Count());

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

        private static StorageManagementClient EstablishClient(AddonManifest manifest, DeveloperParameters devOptions, ref string testProgress)
        {
            testProgress += "Parsing manifest...\n";
            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            testProgress += "Getting credentials...\n";
            // set up the credentials for azure
            testProgress += "Sub ID is: " + manifestprops["AzureManagementSubscriptionID"] + "\n";
            testProgress += "Auth key is: " + manifestprops["AzureAuthenticationKey"] + "\n";
            var creds = Azure.CertificateAuthenticationHelper.getCredentials(manifestprops["AzureManagementSubscriptionID"], manifestprops["AzureAuthenticationKey"]);
            // set up the storage management client
            var client = new StorageManagementClient(creds);
            testProgress += "Successfully returned credentials.\n";
            return client;
        }
    }
}
