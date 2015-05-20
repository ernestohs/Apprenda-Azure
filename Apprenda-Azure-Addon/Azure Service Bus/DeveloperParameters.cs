using System;
using System.Collections.Generic;
using System.Linq;

namespace Apprenda.SaaSGrid.Addons.Azure
{
    internal class DeveloperParameters
    {
        private string DeveloperAlias { get; set; }
        internal string AffinityGroup { get; set; }
        internal string Description { get; set; }
        internal bool GeoReplicationEnabled { get; set; }
        internal string StorageAccountName { get; set; }
        internal string AzureManagementSubscriptionId { get; set; }
        internal string AzureAuthenticationKey { get; set; }
        private string AzureUrl { get; set; }
        internal string Location { get; set; }
        private string RequireDevCredentials { get; set; }
        private string DeveloperID { get; set; }


        public static DeveloperParameters Parse(IEnumerable<AddonParameter> inputAddonParameters, IEnumerable<IAddOnPropertyDefinition> manifestProperties)
        {
            var options = new DeveloperParameters();
            // add values from manifest first
            options = ParseManifest(manifestProperties, options);
            // now add values from developer parameters
            return inputAddonParameters.Aggregate(options, (current, addonParameter) => MapToOption(current, addonParameter.Key, addonParameter.Value));
        }

        private static DeveloperParameters MapToOption(DeveloperParameters options, string key, string value)
        {
            if ("storageaccountname".Equals(key))
            {
                options.StorageAccountName = value;
                return options;
            }
            if ("azuremanagementsubscriptionid".Equals(key))
            {
                options.AzureManagementSubscriptionId = value;
                return options;
            }
            if ("azureauthenticationkey".Equals(key))
            {
                options.AzureAuthenticationKey = value;
                return options;
            }
            if ("azureurl".Equals(key))
            {
                options.AzureUrl = value;
                return options;
            }
            if ("description".Equals(key))
            {
                options.Description = value;
                return options;
            }
            if ("affinitygroup".Equals(key))
            {
                options.AffinityGroup = value;
                return options;
            }
            if ("georeplicationenabled".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ArgumentException(
                        "Tried to pass in a non-boolean value for this option. Please refactor manifest file.");
                }
                options.GeoReplicationEnabled = result;
                return options;
            }
            if ("location".Equals(key))
            {
                options.Location = value;
                return options;
            }
            if ("requiredevcredentials".Equals(key))
            {
                options.RequireDevCredentials = value;
                return options;
            }
            if ("developerid".Equals(key))
            {
                options.DeveloperID = value;
                return options;
            }
            if ("developeralias".Equals(key))
            {
                options.DeveloperAlias = value;
                return options;
            }
            throw new ArgumentException(
                string.Format("The option provided '{0}' does not parse, please try your request again.", key));
        }

        // change this to private
        private static DeveloperParameters ParseManifest(IEnumerable<IAddOnPropertyDefinition> manifestProperties,
            DeveloperParameters _devOptions)
        {
            if (manifestProperties != null)
            {
                foreach (IAddOnPropertyDefinition i in manifestProperties)
                {
                    if (i.Value != null && i.Key != null)
                    {
                        _devOptions = MapToOption(_devOptions, i.Key.Trim().ToLowerInvariant(), i.Value.Trim());
                    }
                }
                return _devOptions;
            }
            return _devOptions;
        }
    }
}