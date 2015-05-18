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
                    provisionResult.EndUserMessage += "Getting into the 'if' statement\n ";
                    if (verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Created))
                    {
                        provisionResult.EndUserMessage += "We're in the 'if' statement\n";
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
                        provisionResult.EndUserMessage += "Connection Data: " + connectionInfo.ToString();
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

            //provisionResult.ConnectionData = "Some Connection Data, yo";
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
            var hardcodeAuth = "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";
            testProgress += "Getting credentials...\n";
            // set up the credentials for azure
            testProgress += "Sub ID is: " + manifestprops["AzureManagementSubscriptionID"] + "\n";
            //testProgress += "Auth key is: " + manifestprops["AzureAuthenticationKey"] + "\n";
            testProgress += "Auth key is:" + hardcodeAuth + "\n";
            var creds = Azure.CertificateAuthenticationHelper.getCredentials(manifestprops["AzureManagementSubscriptionID"], hardcodeAuth);
            // set up the storage management client
            var client = new StorageManagementClient(creds);
            testProgress += "Successfully returned credentials.\n";
            return client;
        }
    }
}