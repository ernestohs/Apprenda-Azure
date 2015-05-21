using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    public class DeveloperParameters
    {
        private string DeveloperAlias { get; set; }
        internal string AffinityGroup { get; private set; }
        internal string Description { get; private set; }
        internal bool GeoReplicationEnabled { get; private set; }
        internal String StorageAccountName { get; set; }
        internal string AzureManagementSubscriptionId { get; private set; }
        internal string AzureAuthenticationKey { get; private set; }
        private string AzureUrl { get; set; }
        internal string Location { get; private set; }
        private string DeveloperId { get; set; }
        internal bool NewStorageAccountFlag { get; private set; }
        internal string ContainerName { get; private set; }

        public static DeveloperParameters Parse(IEnumerable<AddonParameter> parameters, IEnumerable<IAddOnPropertyDefinition> manifestProperties)
        {
            // LINQ is your friend.
            var options = new DeveloperParameters();
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    options = MapToOption(options, param.Key.ToLowerInvariant(), param.Value);
                }
            }
            if (manifestProperties != null)
            {
                foreach (var prop in manifestProperties)
                {
                    options = MapToOption(options, prop.Key.ToLowerInvariant(), prop.Value);
                }
            }
            return options;
        }

        private static DeveloperParameters MapToOption(DeveloperParameters options, String key, String value)
        {
            if("newstorageaccount".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ArgumentException("Tried to pass in a non-boolean value for this option. Please check your options.");
                }
                options.NewStorageAccountFlag = result;
                return options;
            }
            if ("storageaccountname".Equals(key))
            {
                options.StorageAccountName = value;
                return options;
            }
            if ("containername".Equals(key))
            {
                options.ContainerName = value;
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
                    throw new ArgumentException("Tried to pass in a non-boolean value for this option. Please refactor manifest file.");
                }
                options.GeoReplicationEnabled = result;
                return options;
            }
            if ("location".Equals(key))
            {
                options.Location = value;
                return options;
            }
            if ("developerid".Equals(key))
            {
                options.DeveloperId = value;
                return options;
            }
            if ("developeralias".Equals(key))
            {
                options.DeveloperAlias = value;
                return options;
            }
            throw new ArgumentException(string.Format("The option provided '{0}' does not parse, please try your request again.", key));
        }

    }
}
