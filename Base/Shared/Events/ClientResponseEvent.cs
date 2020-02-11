using System;
using Heleus.Network.Client;

namespace Heleus.Apps.Shared
{
    public class ClientResponseEvent
    {
        public readonly HeleusClientResponse Result;

        protected ClientResponseEvent(HeleusClientResponse result)
        {
            Result = result;
        }
    }
}
