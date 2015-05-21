using System;
using System.Collections.Generic;
using System.Configuration;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    class FunctionalTest
    {
        public static void Main(string[] args)
        {
            try
            {
                var addonRequest = new AddonProvisionRequest
                {
                    Manifest = SetupPropertiesAndParameters(),
                    DeveloperParameters = addParameters()
                };

                var output = new AzureStorageAddon().Provision(addonRequest);

                Console.Out.WriteLine(output.IsSuccess);
                Console.Out.WriteLine(output.ConnectionData);
                Console.Out.WriteLine(output.EndUserMessage);
                Console.Out.Write("Working. Press any key to exit.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
                Console.Out.WriteLine(e.Data);
                Console.Out.WriteLine(e.StackTrace);
            }
        }

        private static List<AddonParameter> addParameters()
        {
           return new List<AddonParameter>
            {
                new AddonParameter()
                {
                    Key = "NewStorageAccount",
                    Value = ConfigurationManager.AppSettings["NewStorageAccount"]
                },
                new AddonParameter()
                {
                    Key = "StorageAccountName",
                    Value = ConfigurationManager.AppSettings["StorageAccountName"]
                },
                new AddonParameter()
                {
                    Key = "ContainerName",
                    Value = ConfigurationManager.AppSettings["ContainerName"]
                }
            };
        } 



        private static AddonManifest SetupPropertiesAndParameters()
        {

            var manifest = new AddonManifest()
            {
                AllowUserDefinedParameters = true,
                Author = "Chris Dutra",
                DeploymentNotes = "",
                Description = "",
                DeveloperHelp = "",
                IsEnabled = true,
                ManifestVersionString = "2.0",
                Name = "Azure Storage",
                // we'll handle parameters below.
                Parameters = new ParameterList
                {
                    AllowUserDefinedParameters = "true",
                    Items = addParameters().ToArray() as IAddOnParameterDefinition[]
                },
                Properties = new List<AddonProperty>
                {
                    new AddonProperty()
                    {
                        Key = "AzureManagementSubscriptionId",
                        Value = ConfigurationManager.AppSettings["AzureManagementSubscriptionId"]
                    },
                    new AddonProperty()
                    {
                        Key = "AzureAuthenticationKey",
                        Value = ConfigurationManager.AppSettings["AzureAuthenticationKey"]
                    },
                    new AddonProperty()
                    {
                        Key = "AzureURL",
                        Value = ConfigurationManager.AppSettings["AzureURL"]
                    },
                    new AddonProperty()
                    {
                        Key = "GeoReplicationEnabled",
                        Value = ConfigurationManager.AppSettings["GeoReplicationEnabled"]
                    }
                },
                ProvisioningLocation = "US East",
                ProvisioningPassword = "",
                ProvisioningPasswordHasValue = false,
                ProvisioningUsername = "",
                Vendor = "Microsoft",
                Version = "6.0"
            };
            return manifest;
        }
    }
}
