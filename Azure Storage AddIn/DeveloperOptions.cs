using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;

namespace Azure_Storage_AddIn
{
    class DeveloperOptionsImpl
    {
        internal string DeveloperAlias { get; set; }
        internal string AffinityGroup { get; set; }
        internal string Description { get; set; }
        internal bool GeoReplicationEnabled { get; set; }
        internal String StorageAccountName { get; set; }
        internal String AzureManagementSubscriptionID { get; set; }
        internal String AzureAuthenticationKey { get; set; }
        internal String AzureUrl { get; set; }
        internal String Location { get; set; }
        internal String RequireDevCredentials { get; set; }
        internal String DeveloperID { get; set; }


        public static DeveloperOptionsImpl Parse(String developerOptions, DeveloperOptionsImpl options)
        {
            if (!string.IsNullOrWhiteSpace(developerOptions))
            {
                var optionPairs = developerOptions.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var optionPair in optionPairs)
                {
                    var optionPairParts = optionPair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (optionPairParts.Length == 2)
                    {
                        options = MapToOption(options, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Unable to parse developer options which should be in the form of 'option=value&nextOption=nextValue'. The option '{0}' was not properly constructed",
                                optionPair));
                    }
                }
            }
            else
            {
                throw new ArgumentException("Developer Options String is empty.");
            }

            return options;
        }

        private static DeveloperOptionsImpl MapToOption(DeveloperOptionsImpl options, String key, String value)
        {
            if("storageaccountname".Equals(key))
            {
                options.StorageAccountName = value;
                return options;
            }
            if ("azuremanagementsubscriptionid".Equals(key))
            {
                options.AzureManagementSubscriptionID = value;
                return options;
            }
            if("azureauthenticationkey".Equals(key))
            {
                options.AzureAuthenticationKey = value;
                return options;
            }
            if("azureurl".Equals(key))
            {
                options.AzureUrl = value;
                return options;
            }
            if("description".Equals(key))
            {
                options.Description = value;
                return options;
            }
            if("affinitygroup".Equals(key))
            {
                options.AffinityGroup = value;
                return options;
            }
            if("georeplicationenabled".Equals(key))
            {
                bool result; 
                if(!bool.TryParse(value, out result))
                {
                    throw new ArgumentException("Tried to pass in a non-boolean value for this option. Please refactor manifest file.");
                }
                options.GeoReplicationEnabled = result;
                return options;
            }
            if("location".Equals(key))
            {
                options.Location = value;
                return options;
            }
            if ("requiredevcredentials".Equals(key))
            {
                options.RequireDevCredentials = value;
                return options;
            }
            if("developerid".Equals(key))
            {
                options.DeveloperID = value;
                return options;
            }
            if("developeralias".Equals(key))
            {
                options.DeveloperAlias = value;
                return options;
            }
            throw new ArgumentException(string.Format("The option provided '{0}' does not parse, please try your request again.", key));
        }

        internal static DeveloperOptionsImpl ParseManifest(IEnumerable<IAddOnPropertyDefinition> manifestProperties, DeveloperOptionsImpl _devOptions)
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
