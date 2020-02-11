using System;
using System.Collections.Generic;
using System.IO;
using Heleus.Base;
using Heleus.Chain;

namespace Heleus.Apps.Shared
{
    abstract class ServiceAccountKeyCommand : CustomEndpointCommand
    {
        protected ServiceAccountKeyStore serviceaccountfile { get; private set; }
        protected string serviceaccountpassword { get; private set; }

        protected override List<KeyValuePair<string, string>> GetUsageItems()
        {
            var items = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(serviceaccountfile), "The path to the service account key"),
                new KeyValuePair<string, string>(nameof(serviceaccountpassword), "The password for the service account key")
            };

            items.AddRange(base.GetUsageItems());

            return items;
        }

        protected override bool Parse(ArgumentsParser arguments)
        {
            if (!base.Parse(arguments))
                return false;

            try
            {
                var filepath = arguments.String(nameof(serviceaccountfile), null);
                if (!File.Exists(filepath))
                {
                    SetError($"Service Account key ({filepath}) not found");
                    return false;
                }

                var keyData = File.ReadAllText(filepath);
                serviceaccountfile = KeyStore.Restore<ServiceAccountKeyStore>(keyData);

                var password = arguments.String(nameof(serviceaccountpassword), null);
                if (!serviceaccountfile.DecryptKeyAsync(password, false).Result)
                {
                    SetError("Service Account password is wrong");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.HandleException(ex);
                return false;
            }

            if (serviceaccountfile?.DecryptedKey == null)
            {
                SetError("Invalid Service Account key");
                return false;
            }

            ServiceNode.ImportServiceAccount(serviceaccountfile, null).Wait();

            return true;
        }
    }
}
