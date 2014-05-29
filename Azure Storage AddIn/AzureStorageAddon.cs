using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using System.Net;

namespace Azure_Storage_AddIn
{

    public class AzureStorageAddonImpl : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            // I not a fan of the design currently, but I'll roll with it for now. 
            var provisionResult = new ProvisionAddOnResult("");
            AddonManifest manifest = request.Manifest;
            string devOptions = request.DeveloperOptions;
            DeveloperOptionsImpl _devOptions;

            // aha. so for required properties, let's pull in the properties from the manifest
            // so we can use those as developer options. 
            // it shouldn't matter where the changes are!
            try
            {
                // parse required options here, use developer options class to do so.
                var manifestProperties = manifest.GetProperties();
                // Developer Options will be instantiated first time here (hence, null).
                OperationResult parseManifestResult = ParseManifestResult(manifestProperties, null, out _devOptions);
                if (!parseManifestResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseManifestResult.EndUserMessage;
                    return provisionResult;
                }

                // parse optional developer parameters, this could potentially allow us to override defaults.
                var parseOptionsResult = ParseDevOptions(devOptions, _devOptions, out _devOptions);

                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                // establish MSFT Azure Storage client
                StorageManagementClient client;
                var establishClientResult = EstablishClient(manifest, _devOptions, out client);

                int uniqueIDIfNeeded = 0;
                if (_devOptions.StorageAccountName == null)
                {
                    _devOptions.StorageAccountName = "yupitsnullallright";
                }
                var name_is_available = client.StorageAccounts.CheckNameAvailability(_devOptions.StorageAccountName);
                // as might be the case, this will increment and append a numeral onto the end of the storage account name in order to uniquely qualify it.
                while (!name_is_available.IsAvailable)
                {
                    _devOptions.StorageAccountName = string.Concat(_devOptions.StorageAccountName, uniqueIDIfNeeded++);
                    name_is_available = client.StorageAccounts.CheckNameAvailability(_devOptions.StorageAccountName);
                }
                StorageAccountCreateParameters parameters = CreateStorageAccountParameters(_devOptions);
                OperationResponse m_response = client.StorageAccounts.Create(parameters);
        
                do
                {
                    var verificationResponse = client.StorageAccounts.Get(parameters.Name);
                        
                    if(verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Created))
                    {                       
                        var azureconnectioninfo = client.StorageAccounts.Get(_devOptions.StorageAccountName);
                        var keysForStorageUnit = client.StorageAccounts.GetKeys(_devOptions.StorageAccountName);

                        var connectionInfo = new ConnectionInfo()
                        {
                            PrimaryKey = keysForStorageUnit.PrimaryKey,
                            SecondaryKey = keysForStorageUnit.SecondaryKey,
                            StorageAccountName = azureconnectioninfo.StorageAccount.Name,
                            URI = keysForStorageUnit.Uri.ToString()
                        };
                        provisionResult.ConnectionData = string.Format("http://{0}.blob.core.windows.net", azureconnectioninfo.StorageAccount.Name);
                        // deprovision request of storage account was successful.
                        provisionResult.IsSuccess = true;
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(10d));
                }
                while (true);
            }catch(Exception e)
            {
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        private OperationResult ParseManifestResult(IEnumerable<IAddOnPropertyDefinition> manifestProperties, DeveloperOptionsImpl _devOptions, out DeveloperOptionsImpl out_devOptions)
        {
            _devOptions = new DeveloperOptionsImpl();
            var result = new OperationResult() { IsSuccess = false };
            var progress = "";
            try
            {
                progress += "Parsing manifest...\n";
                out_devOptions = DeveloperOptionsImpl.ParseManifest(manifestProperties, _devOptions);
                result.IsSuccess = true;
                result.EndUserMessage = progress;
                return result;
            }
            catch (ArgumentException e)
            {
                // devOptions must remain the same if error, this will help default back to manifest entries.
                result.EndUserMessage = e.Message;
                // can't assign devOptions, as it should remain the same.
                out_devOptions = _devOptions;
                return result;
            }
        }

        private StorageAccountCreateParameters CreateStorageAccountParameters(DeveloperOptionsImpl developerOptions)
        {
            var parameters = new StorageAccountCreateParameters()
            {
                Description = developerOptions.Description,
                GeoReplicationEnabled = developerOptions.GeoReplicationEnabled,
                // for now. not sure what the label entails.
                Label = developerOptions.StorageAccountName,
                Name = developerOptions.StorageAccountName
            };
            // only one can be used. TODO at a later date, we'll put this into the validate manifest method.
            if(developerOptions.AffinityGroup != null)
            {
                parameters.AffinityGroup = developerOptions.AffinityGroup;
            }
            else if(developerOptions.Location != null)
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
            string connectionData = request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;
            string devOptions = request.DeveloperOptions;
            DeveloperOptionsImpl _devOptions;

            // parse required options here, use developer options class to do so.
            var manifestProperties = manifest.GetProperties();
            // Developer Options will be instantiated first time here.
            OperationResult parseManifestResult = ParseManifestResult(manifestProperties, null, out _devOptions);
            if (!parseManifestResult.IsSuccess)
            {
                deprovisionResult.EndUserMessage = parseManifestResult.EndUserMessage;
                return deprovisionResult;
            }

            // parse optional developer parameters, this could potentially allow us to override defaults.
            var parseOptionsResult = ParseDevOptions(devOptions, _devOptions, out _devOptions);

            if (!parseOptionsResult.IsSuccess)
            {
                deprovisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                return deprovisionResult;
            }
            // set up the credentials for azure
            SubscriptionCloudCredentials creds = CertificateAuthenticationHelper.getCredentials(_devOptions.AzureManagementSubscriptionID, _devOptions.AzureAuthenticationKey);
            // set up the storage management client
            StorageManagementClient client = new StorageManagementClient(creds);

            OperationResponse m_response = client.StorageAccounts.Delete(_devOptions.StorageAccountName);

            do
            {
                var verificationResponse = client.StorageAccounts.Get(_devOptions.StorageAccountName);

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
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";
            StorageManagementClient client = null;
            var manifestProperties = manifest.Properties;

            if (manifestProperties != null && manifestProperties.Any())
            {
                DeveloperOptionsImpl devOptions;
                
                testProgress += "Evaluating required manifest properties...\n";
                if (!ValidateManifest(manifest, out testResult))
                {
                    return testResult;
                }

                var parseManifestResult = ParseManifestResult(manifestProperties, null, out devOptions);
                if(!parseManifestResult.IsSuccess)
                {
                    return parseManifestResult;
                }

                var parseOptionsResult = ParseDevOptions(developerOptions, devOptions, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    return parseOptionsResult;
                }
                testProgress += parseOptionsResult.EndUserMessage;

                try
                {
                    testProgress += "Establishing connection to Azure...\n";
                    // set up the credentials for azure
                    
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    var listOfStorageAccounts = client.StorageAccounts.List();
                    
                    testProgress += string.Format("Number of Accounts: '{0}'" ,listOfStorageAccounts.Count());
                    
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

        private bool ValidateManifest(AddonManifest manifest, out OperationResult testResult)
        {
            testResult = new OperationResult();

            var prop =
                    manifest.Properties.FirstOrDefault(
                        p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (prop == null || !prop.HasValue)
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage = "Missing required property 'requireDevCredentials'. This property needs to be provided as part of the manifest";
                return false;
            }

            if (string.IsNullOrWhiteSpace(manifest.ProvisioningUsername) ||
                string.IsNullOrWhiteSpace(manifest.ProvisioningPassword))
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage = "Missing credentials 'provisioningUsername' & 'provisioningPassword' . These values needs to be provided as part of the manifest";
                return false;
            }

            return true;
        }

        private bool ValidateDevCreds(DeveloperOptionsImpl devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AzureManagementSubscriptionID) || string.IsNullOrWhiteSpace(devOptions.AzureAuthenticationKey));
        }

        private OperationResult ParseDevOptions(string developerOptions, DeveloperOptionsImpl devOptions, out DeveloperOptionsImpl out_devOptions)
        {
            // can't set to null here. we have defaults set up by the manifest.
            var result = new OperationResult() { IsSuccess = false };
            var progress = "";

            // first things first. If developer options are empty and there are no optional parameters, just return! No error.
            if(developerOptions.Length.Equals(0))
            {
                out_devOptions = devOptions;
                result.IsSuccess = true;
                result.EndUserMessage = "No optional parameters found, continuing...";
                return result;
            }
            try
            {
                progress += "Parsing developer options...\n";
                out_devOptions = DeveloperOptionsImpl.Parse(developerOptions, devOptions);
                result.IsSuccess = true;
                result.EndUserMessage = progress;
                return result;
            }
            catch (ArgumentException e)
            {
                // need the assign for the out variable, unfortunately.
                out_devOptions = devOptions;
                // devOptions must remain the same if error, this will help default back to manifest entries.
                result.EndUserMessage = e.Message;
                // can't assign devOptions, as it should remain the same.
                return result;
            }
        }

        private OperationResult EstablishClient(AddonManifest manifest, DeveloperOptionsImpl devOptions, out StorageManagementClient client)
        {
            OperationResult result;

            bool requireCreds;

            var prop =
                manifest.Properties.First(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (bool.TryParse(prop.Value, out requireCreds) && requireCreds)
            {
                if (!ValidateDevCreds(devOptions))
                {
                    client = null;
                    result = new OperationResult()
                    {
                        IsSuccess = false,
                        EndUserMessage =
                            "The add on requires that developer credentials are specified but none were provided."
                    };
                    return result;
                }
            }
           
            // set up the credentials for azure
            SubscriptionCloudCredentials creds = CertificateAuthenticationHelper.getCredentials(devOptions.AzureManagementSubscriptionID, devOptions.AzureAuthenticationKey);
            // set up the storage management client
            client = new StorageManagementClient(creds);
            result = new OperationResult { IsSuccess = true };
            return result;
        }
    }
}
