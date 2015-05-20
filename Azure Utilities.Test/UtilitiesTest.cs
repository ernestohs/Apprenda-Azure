using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons.Azure;
using Microsoft.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using NUnit.Framework;

namespace Azure_Utilities.Test
{
    [TestFixture]
    public class UtilitiesTest
    {
        private string AzureAccessToken { get; set; }
        private string AzureSubscriptionId { get; set; }

        [SetUp]
        public void SetupCredentialsTest()
        {
            // use this to setup Azure
            AzureSubscriptionId = ConfigurationManager.AppSettings["subscriptionId"];
            AzureAccessToken = ConfigurationManager.AppSettings["authKey"];
        }

        [Test]
        public void GetCredentialsTestNulls()
        {
            Assert.Throws(typeof (ArgumentNullException), new TestDelegate(delegate
            {
                CertificateAuthenticationHelper.GetCredentials("", "");
            }));
        }

        [Test]
        public void GetCredentialsWithCrapData()
        {
            var badFormatSubscriptionID = "oasidnfaodifna;odifna;odifand;ofiandfosainfoaiurgeoivfnaorivnar";
            var badFormatBaseEncodedCert = "or2ifnwosnfvslfnbsougoaingfa;oineo;gtinearoigfnarg;oiarhg;oeairhga;oiregnalkna";
            Assert.Throws(typeof(FormatException), new TestDelegate(delegate
            {
                CertificateAuthenticationHelper.GetCredentials(badFormatSubscriptionID, badFormatBaseEncodedCert);
            }));
        }

        [Test]
        public void GetLegitCredentials()
        {
            var credential = CertificateAuthenticationHelper.GetCredentials(AzureSubscriptionId, AzureAccessToken);
            Assert.That(credential, Is.TypeOf<CertificateCloudCredentials>());
        }

        [TearDown]
        public void TearDownCredentialsTest()
        {
            
        }
    }
}
