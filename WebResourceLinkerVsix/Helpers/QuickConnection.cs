using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Discovery;
using System.ServiceModel.Description;
using System.Net;

namespace WebResourceLinkerVsix
{
    public class QuickConnection
    {
        public static IOrganizationService Connect(string url, string domain, string username, string password, string organization, out string publicUrl)
        {
            publicUrl = "";

            var credentials = GetCredentials(url, domain, username, password);
            ClientCredentials deviceCredentials = null;
            if (url.IndexOf("dynamics.com", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                deviceCredentials = Microsoft.Crm.Services.Utility.DeviceIdManager.LoadOrRegisterDevice();
            }

            Uri orgUri = null;
            OrganizationServiceProxy sdk = null;

            using (DiscoveryServiceProxy disco = new DiscoveryServiceProxy(new Uri(url), null, credentials, deviceCredentials))
            {
                if (disco != null)
                {
                    OrganizationDetailCollection orgs = DiscoverOrganizations(disco);
                    if (orgs.Count > 0)
                    {
                        var found = orgs.ToList()
                            .Where(a => a.UniqueName.Equals(organization, StringComparison.InvariantCultureIgnoreCase))
                            .Take(1).SingleOrDefault();

                        if (found != null)
                        {
                            orgUri = new Uri(found.Endpoints[EndpointType.OrganizationService]);
                            publicUrl = found.Endpoints[EndpointType.WebApplication];
                        }
                    }
                }
            }

            if (orgUri != null)
            {
                sdk = new OrganizationServiceProxy(orgUri, null, credentials, deviceCredentials);
            }
 
            return sdk;
        }

        private static OrganizationDetailCollection DiscoverOrganizations(DiscoveryServiceProxy service)
        {
            RetrieveOrganizationsRequest request = new RetrieveOrganizationsRequest();
            RetrieveOrganizationsResponse response = (RetrieveOrganizationsResponse)service.Execute(request);

            return response.Details;
        }

        private static ClientCredentials GetCredentials(string url, string domain, string username, string password)
        {
            ClientCredentials credentials = new ClientCredentials();

            var config = ServiceConfigurationFactory.CreateConfiguration<IDiscoveryService>(new Uri(url));

            if (config.AuthenticationType == AuthenticationProviderType.ActiveDirectory)
            {
                NetworkCredential nc = CredentialCache.DefaultNetworkCredentials;
                if (!string.IsNullOrEmpty(domain) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    nc = new System.Net.NetworkCredential(username, password, domain);
                }

                credentials.Windows.ClientCredential = nc;
            }
            else if (config.AuthenticationType == AuthenticationProviderType.Federation
                || config.AuthenticationType == AuthenticationProviderType.LiveId
                || config.AuthenticationType == AuthenticationProviderType.OnlineFederation)
            {
                credentials.UserName.UserName = username;
                credentials.UserName.Password = password;
            }
            else if (config.AuthenticationType == AuthenticationProviderType.None)
            {
            }

            return credentials;
        }
    }
}
