using System;
using System.ComponentModel;
using Heleus.Apps.Shared;
using Xamarin.Forms;
using Xamarin.Forms.Platform.GTK;
using Xamarin.Forms.Platform.GTK.Renderers;

[assembly: ExportRenderer(typeof(ExtNavigationPage), typeof(Heleus.Apps.Shared.GTK.Renderers.ExtNavigationPageRenderer))]

namespace Heleus.Apps.Shared.GTK.Renderers
{
	public class ExtNavigationPageRenderer : NavigationPageRenderer
	{

    }
}
