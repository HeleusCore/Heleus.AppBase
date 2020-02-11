using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Heleus.Apps.Shared
{
    public class OpenFile : IDisposable
    {
        public readonly string Name;
        public readonly Stream Stream;

        public bool Valid => Stream != null;

        public OpenFile()
        {
        }

        public OpenFile(string name, Stream stream)
        {
            Name = name;
            Stream = stream;
        }

        public virtual void Dispose()
        {
            Stream?.Dispose();
        }
    }
}
