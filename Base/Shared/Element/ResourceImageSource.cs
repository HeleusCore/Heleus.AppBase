using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    sealed class ResourceImageSource : ImageSource
    {
        public static readonly BindableProperty FileProperty = BindableProperty.Create("File", typeof(string), typeof(ResourceImageSource), null, BindingMode.OneWay, null, null, null, null, null);

        public string File
        {
            get
            {
                return (string)base.GetValue(FileProperty);
            }
            set
            {
                base.SetValue(FileProperty, value);
            }
        }

        public ResourceImageSource()
        { 
        }

        public ResourceImageSource(string file)
        {
            File = file;
        }
    }
}
