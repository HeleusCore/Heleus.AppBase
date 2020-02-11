namespace Heleus.Apps.Shared
{
    public class ServiceNodeEvent
    {
        public readonly ServiceNode ServiceNode;

        public ServiceNodeEvent(ServiceNode serviceNode)
        {
            ServiceNode = serviceNode;
        }
    }

    public class ServiceNodeDefaultChangedEvent : ServiceNodeEvent
    {
        public ServiceNodeDefaultChangedEvent(ServiceNode serviceNode) : base(serviceNode)
        {
        }
    }

    public class ServiceNodeChangedEvent : ServiceNodeEvent
    {
        public ServiceNodeChangedEvent(ServiceNode serviceNode) : base(serviceNode)
        {
        }
    }

    public class ServiceNodesLoadedEvent
    {
    }
}
