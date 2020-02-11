namespace Heleus.Apps.Shared
{
    public class ServiceNodeAddedEvent : ServiceNodeEvent
    {
        public ServiceNodeAddedEvent(ServiceNode serviceNode) : base(serviceNode)
        {
        }
    }
}
