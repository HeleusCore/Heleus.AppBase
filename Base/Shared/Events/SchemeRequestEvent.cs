using System;

namespace Heleus.Apps.Shared
{
    public class SchemeRequestEvent
    {
        public readonly Uri Uri;

        public SchemeRequestEvent(Uri uri)
        {
            Uri = uri;
        }
    }
}
