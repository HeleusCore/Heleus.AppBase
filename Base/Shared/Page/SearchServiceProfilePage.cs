using System.Collections.Generic;
using System.Threading.Tasks;
using Heleus.ProfileService;

namespace Heleus.Apps.Shared
{
    public class SearchServiceProfilePage : SearchProfilePage
    {
        readonly protected ServiceNode _serviceNode;

        public SearchServiceProfilePage(ServiceNode serviceNode) : base(new ServiceProfileSearch(serviceNode))
        {
            _serviceNode = serviceNode;
        }
    }
}
