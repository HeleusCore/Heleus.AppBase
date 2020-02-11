using System;
using Xamarin.Forms;

namespace Heleus.Apps.Shared
{
    // Dummy class for Xamarin.Forms.Application
    public class Application
    {
        public Application Current;

        protected Page MainPage;
        protected event EventHandler<ModalPoppedEventArgs> ModalPopped;

        protected virtual void OnStart()
        {
        }

        protected virtual void OnResume()
        {
        }

        protected virtual void OnSleep()
        {

        }

        public void PopModal()
        {
            ModalPopped?.Invoke(null, null);
        }
    }
}
