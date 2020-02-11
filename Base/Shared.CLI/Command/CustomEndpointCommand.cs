using System;
using System.Collections.Generic;
using Heleus.Base;

namespace Heleus.Apps.Shared
{
    abstract class CustomEndpointCommand : Command
    {
        protected Uri customendpoint { get; private set; }
        protected int customchainid { get; private set; }
        protected ServiceNode ServiceNode { get; private set; }

        protected override List<KeyValuePair<string, string>> GetUsageItems()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(customendpoint), "The custom endoint"),
                new KeyValuePair<string, string>(nameof(customchainid), "The custom chain id")
            };
        }

        protected override bool Parse(ArgumentsParser arguments)
        {
            try
            {
                var endpoint = arguments.String(nameof(customendpoint), null);
                if (!string.IsNullOrEmpty(endpoint))
                    customendpoint = new Uri(endpoint);

                customchainid = arguments.Integer(nameof(customchainid), 0);
            }
            catch (Exception ex)
            {
                Log.HandleException(ex);
                return false;
            }

            var custom = (customendpoint != null && customendpoint != ServiceNodeManager.Current.DefaultEndPoint) ||
                (customchainid > 0 && customchainid != ServiceNodeManager.Current.DefaultChainId);

            ServiceNode = new ServiceNode(customendpoint ?? ServiceNodeManager.Current.DefaultEndPoint, customchainid > 0 ? customchainid : ServiceNodeManager.Current.DefaultChainId, custom, string.Empty, ServiceNodeManager.Current, null);
            ServiceNodeManager.Current.AddServiceNodeForCli(ServiceNode);

            return true;
        }
    }
}
