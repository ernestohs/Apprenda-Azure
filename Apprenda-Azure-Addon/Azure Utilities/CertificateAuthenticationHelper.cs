using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Azure;

namespace Apprenda.SaaSGrid.Addons.Azure
{
    public static class CertificateAuthenticationHelper
    {
        // hate to hardcode this, but testing to see if there is a bug in validation
        ///private const String subsId = "0b26f491-54b9-4a23-a048-821f3cb4d841";
        private const String authkey = "MIIC6jCCAdagAwIBAgIQ8ODp0yPBALxDyFY+9UiTcDAJBgUrDgMCHQUAMBExDzANBgNVBAMTBm15Y2VydDAeFw0xNTA0MjcxNjM2MjVaFw0zOTEyMzEyMzU5NTlaMBExDzANBgNVBAMTBm15Y2VydDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAKjFBjmD7HPt4+VQz/BkYdSNWhhDGYc+aKKCXiptG87E19eBOsDhfc4JamzTttNBmMkLxkNHNXNHhLZVWk/3bi8LJZpLlcvTfblmZesJFLrxa9EsEiJzpXennpn91XeCgIaCb4xEAYDoII0B30Gl8IFVux69WOPkgjdyNQDT1FRwqOWsnRrIZuZAMeVOV8Rwz7e8p2WswmTDnvua4nyZio06jLrFb0k6APHdOtd9mJrzgTdAYRF5RB1j//9ntmzdxpmjy2O8T4EqA91t0VP1EQZZtuTSC5sWAjG+M+oGiZCzNjQ7WaWjONFJXFmwr5lboAKE17nlSUdU2eWQbWMhtzkCAwEAAaNGMEQwQgYDVR0BBDswOYAQ1e7XKbrCtmMx/kJK3/dD9KETMBExDzANBgNVBAMTBm15Y2VydIIQ8ODp0yPBALxDyFY+9UiTcDAJBgUrDgMCHQUAA4IBAQCTLth+PM3xD+/xSRZGmAYygjpTXbjQIJtNd4TU+K5f6JwQfIi9QhMTo1hJMtHo1apFhHdwBsdRpA6eiJu5ySQoqAGRHubAfe7HaOoMM4vf/62HRAen4PJZ50/afyK0tbnOm3zMxyOVkiSTG7wvTUmGyiPdxnf7xfbYaz3AJSOYMHteJqJMdphVvsVVS0duKSdb2FgExDEF9hSkK6WOzqGQLWjMIjgQ720DN1NVSsbsiBrT2ac/kgpg8shFJB9IOz/OsGKnZ5y75BcgWCXcS60qWnM26850g+HAzzXW7RX7fEDF0K4K0jSmn/JW9HfazB98AYjo47UhjfeZs5w8XFuN";
        public static SubscriptionCloudCredentials getCredentials(string subscriptionId, string base64encodedcert)
        {
            return new CertificateCloudCredentials(subscriptionId, new X509Certificate2(Convert.FromBase64String(base64encodedcert)));
        }
    }
}
