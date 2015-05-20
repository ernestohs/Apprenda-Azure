using System.Collections.Generic;
using System.Configuration;
using NUnit.Framework;

namespace Apprenda.SaaSGrid.Addons.Azure.Storage
{
    [TestFixture]
    public class StorageTest
    {
        private AddonProvisionRequest ProvisionRequest { get; set; }
        private AddonDeprovisionRequest DeprovisionRequest { get; set; }
        private AddonTestRequest TestRequest { get; set; }

        [SetUp]
        public void SetupManifest()
        {
            ProvisionRequest = new AddonProvisionRequest {Manifest = SetupPropertiesAndParameters()};
            DeprovisionRequest = new AddonDeprovisionRequest {Manifest = SetupPropertiesAndParameters()};
            TestRequest = new AddonTestRequest {Manifest = SetupPropertiesAndParameters()};
        }

        private static AddonManifest SetupPropertiesAndParameters()
        {
            var paramConstructor = new List<AddonParameter>
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
                    Items = paramConstructor.ToArray() as IAddOnParameterDefinition[]
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

        [Test]
        public void ProvisionTest()
        {
            var output = new AzureStorageAddon().Provision(ProvisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
            Assert.That(output.ConnectionData.Length, Is.GreaterThan(0));
        }

        [Test]
        public void DeProvisionTest()
        {
            var output = new AzureStorageAddon().Deprovision(DeprovisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }

        // this is testing the SOC Test Method
        [Test]
        public void SocTest()
        {
            var output = new AzureStorageAddon().Test(TestRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }
    }
}
