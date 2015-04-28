using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;

namespace Apprenda.SaaSGrid.Addons.Azure.ServiceBus
{
    public class AzureStorageAddonImpl : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("");
            AddonManifest manifest = request.Manifest;
            string devOptions = request.DeveloperOptions;

            try
            {
                // parse required options here, use developer options class to do so.
                IEnumerable<IAddOnPropertyDefinition> manifestProperties = manifest.GetProperties();
                // Developer Options will be instantiated first time here (hence, null).
                DeveloperOptions _devOptions;
                OperationResult parseManifestResult = ParseManifestResult(manifestProperties, null, out _devOptions);
                if (!parseManifestResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseManifestResult.EndUserMessage;
                    return provisionResult;
                }

                // parse optional developer parameters, this could potentially allow us to override defaults.
                OperationResult parseOptionsResult = ParseDevOptions(devOptions, _devOptions, out _devOptions);

                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                // establish MSFT Azure Storage client
                StorageManagementClient client;
                OperationResult establishClientResult = EstablishClient(manifest, _devOptions, out client);

                int uniqueIDIfNeeded = 0;
                if (_devOptions.StorageAccountName == null)
                {
                    _devOptions.StorageAccountName = "yupitsnullallright";
                }
                CheckNameAvailabilityResponse name_is_available =
                    client.StorageAccounts.CheckNameAvailability(_devOptions.StorageAccountName);
                // as might be the case, this will increment and append a numeral onto the end of the storage account name in order to uniquely qualify it.
                while (!name_is_available.IsAvailable)
                {
                    _devOptions.StorageAccountName = string.Concat(_devOptions.StorageAccountName, uniqueIDIfNeeded++);
                    name_is_available = client.StorageAccounts.CheckNameAvailability(_devOptions.StorageAccountName);
                }
                StorageAccountCreateParameters parameters = CreateStorageAccountParameters(_devOptions);
                OperationStatusResponse mResponse = client.StorageAccounts.Create(parameters);

                do
                {
                    StorageAccountGetResponse verificationResponse = client.StorageAccounts.Get(parameters.Name);

                    if (verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Created))
                    {
                        StorageAccountGetResponse azureconnectioninfo =
                            client.StorageAccounts.Get(_devOptions.StorageAccountName);
                        StorageAccountGetKeysResponse keysForStorageUnit =
                            client.StorageAccounts.GetKeys(_devOptions.StorageAccountName);

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

        private OperationResult ParseManifestResult(IEnumerable<IAddOnPropertyDefinition> manifestProperties,
            DeveloperOptions _devOptions, out DeveloperOptions out_devOptions)
        {
            _devOptions = new DeveloperOptions();
            var result = new OperationResult {IsSuccess = false};
            string progress = "";
            try
            {
                progress += "Parsing manifest...\n";
                out_devOptions = DeveloperOptions.ParseManifest(manifestProperties, _devOptions);
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

        private StorageAccountCreateParameters CreateStorageAccountParameters(DeveloperOptions developerOptions)
        {
            var parameters = new StorageAccountCreateParameters
            {
                Description = developerOptions.Description,
                //GeoReplicationEnabled = developerOptions.GeoReplicationEnabled,
                // for now. not sure what the label entails.
                Label = developerOptions.StorageAccountName,
                Name = developerOptions.StorageAccountName
            };
            // only one can be used. TODO at a later date, we'll put this into the validate manifest method.
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
            string connectionData = request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;
            string devOptions = request.DeveloperOptions;
            DeveloperOptions _devOptions;

            // parse required options here, use developer options class to do so.
            IEnumerable<IAddOnPropertyDefinition> manifestProperties = manifest.GetProperties();
            // Developer Options will be instantiated first time here.
            OperationResult parseManifestResult = ParseManifestResult(manifestProperties, null, out _devOptions);
            if (!parseManifestResult.IsSuccess)
            {
                deprovisionResult.EndUserMessage = parseManifestResult.EndUserMessage;
                return deprovisionResult;
            }

            // parse optional developer parameters, this could potentially allow us to override defaults.
            OperationResult parseOptionsResult = ParseDevOptions(devOptions, _devOptions, out _devOptions);

            if (!parseOptionsResult.IsSuccess)
            {
                deprovisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                return deprovisionResult;
            }
            // set up the credentials for azure
            SubscriptionCloudCredentials creds =
                CertificateAuthenticationHelper.getCredentials(_devOptions.AzureManagementSubscriptionId,
                    _devOptions.AzureAuthenticationKey);
            // set up the storage management client
            var client = new StorageManagementClient();

            AzureOperationResponse mResponse = client.StorageAccounts.Delete(_devOptions.StorageAccountName);

            do
            {
                StorageAccountGetResponse verificationResponse =
                    client.StorageAccounts.Get(_devOptions.StorageAccountName);

                if (verificationResponse.StorageAccount.Properties.Status.Equals(StorageAccountStatus.Deleting))
                {
                    deprovisionResult.IsSuccess = true;
                    deprovisionResult.EndUserMessage =
                        string.Format(
                            "Deprovision Request Complete, please allow a few minutes for resources to be fully deleted.");
                    break;
                }
                Thread.Sleep(TimeSpan.FromSeconds(10d));
            } while (true);
            return deprovisionResult;
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;
            var testResult = new OperationResult {IsSuccess = false};
            string testProgress = "";
            StorageManagementClient client = null;
            List<AddonProperty> manifestProperties = manifest.Properties;

            if (manifestProperties != null && manifestProperties.Any())
            {
                DeveloperOptions devOptions;

                testProgress += "Evaluating required manifest properties...\n";
                if (!ValidateManifest(manifest, out testResult))
                {
                    return testResult;
                }

                OperationResult parseManifestResult = ParseManifestResult(manifestProperties, null, out devOptions);
                if (!parseManifestResult.IsSuccess)
                {
                    return parseManifestResult;
                }

                OperationResult parseOptionsResult = ParseDevOptions(developerOptions, devOptions, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    return parseOptionsResult;
                }
                testProgress += parseOptionsResult.EndUserMessage;

                try
                {
                    testProgress += "Establishing connection to Azure...\n";
                    // set up the credentials for azure

                    OperationResult establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    StorageAccountListResponse listOfStorageAccounts = client.StorageAccounts.List();

                    testProgress += string.Format("Number of Accounts: '{0}'", listOfStorageAccounts.Count());

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

            AddonProperty prop =
                manifest.Properties.FirstOrDefault(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (prop == null || !prop.HasValue)
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage =
                    "Missing required property 'requireDevCredentials'. This property needs to be provided as part of the manifest";
                return false;
            }

            if (string.IsNullOrWhiteSpace(manifest.ProvisioningUsername) ||
                string.IsNullOrWhiteSpace(manifest.ProvisioningPassword))
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage =
                    "Missing credentials 'provisioningUsername' & 'provisioningPassword' . These values needs to be provided as part of the manifest";
                return false;
            }

            return true;
        }

        private bool ValidateDevCreds(DeveloperOptions devOptions)
        {
            return
                !(string.IsNullOrWhiteSpace(devOptions.AzureManagementSubscriptionId) ||
                  string.IsNullOrWhiteSpace(devOptions.AzureAuthenticationKey));
        }

        private OperationResult ParseDevOptions(string developerOptions, DeveloperOptions devOptions,
            out DeveloperOptions out_devOptions)
        {
            // can't set to null here. we have defaults set up by the manifest.
            var result = new OperationResult {IsSuccess = false};
            string progress = "";

            // first things first. If developer options are empty and there are no optional parameters, just return! No error.
            if (developerOptions.Length.Equals(0))
            {
                out_devOptions = devOptions;
                result.IsSuccess = true;
                result.EndUserMessage = "No optional parameters found, continuing...";
                return result;
            }
            try
            {
                progress += "Parsing developer options...\n";
                out_devOptions = DeveloperOptions.Parse(developerOptions, devOptions);
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

        private OperationResult EstablishClient(AddonManifest manifest, DeveloperOptions devOptions,
            out StorageManagementClient client)
        {
            OperationResult result;

            bool requireCreds;

            AddonProperty prop =
                manifest.Properties.First(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (bool.TryParse(prop.Value, out requireCreds) && requireCreds)
            {
                if (!ValidateDevCreds(devOptions))
                {
                    client = null;
                    result = new OperationResult
                    {
                        IsSuccess = false,
                        EndUserMessage =
                            "The add on requires that developer credentials are specified but none were provided."
                    };
                    return result;
                }
            }

            // set up the credentials for azure
            SubscriptionCloudCredentials creds =
                CertificateAuthenticationHelper.getCredentials(devOptions.AzureManagementSubscriptionId,
                    devOptions.AzureAuthenticationKey);
            // set up the storage management client
            client = new StorageManagementClient(creds);
            result = new OperationResult {IsSuccess = true};
            return result;
        }
    }
}