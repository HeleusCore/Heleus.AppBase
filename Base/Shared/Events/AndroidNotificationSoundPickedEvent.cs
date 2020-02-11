namespace Heleus.Apps.Shared
{
    public class AndroidNotificationSoundPickedEvent
    {
        public readonly string Uri;

        public AndroidNotificationSoundPickedEvent(string uri)
        {
            Uri = uri;
        }
    }
}
