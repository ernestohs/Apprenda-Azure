using System;
using System.Configuration;
using Apprenda.SaaSGrid.Addons.Azure;
using NUnit.Framework;

namespace Azure_Utilities.Test
{
    [TestFixture]
    public class UtilitiesTest
    {
        private string badFormatSubscriptionID = "";
        private string badFormatBaseEncodedCert = "";

        [SetUp]
        public void SetupCredentialsTest()
        {
            badFormatSubscriptionID = "oasidnfaodifna;odifna;odifand;ofiandfosainfoaiurgeoivfnaorivnar";
            badFormatBaseEncodedCert = "or2ifnwosnfvslfnbsougoaingfa;oineo;gtinearoigfnarg;oiarhg;oeairhga;oiregnalkna";
        }

        [Test]
        public void GetCredentialsTestNulls()
        {
            Assert.Throws(typeof (ArgumentException), new TestDelegate(delegate
            {
                CertificateAuthenticationHelper.GetCredentials("", "");
            }));
        }

        [Test]
        public void GetCredentialsWithCrapData()
        {
            Assert.Throws(typeof(FormatException), new TestDelegate(delegate
            {
                CertificateAuthenticationHelper.GetCredentials(badFormatSubscriptionID, badFormatBaseEncodedCert);
            }));
        }

        [TearDown]
        public void TearDownCredentialsTest()
        {
            
        }
    }
}
