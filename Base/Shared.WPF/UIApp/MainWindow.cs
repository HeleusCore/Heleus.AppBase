using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WPF;

namespace Heleus.Apps.Shared.WPF
{
    public partial class MainWindow : FormsApplicationPage
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            Instance = this;
            InitializeComponent();

            Forms.Init();
            LoadApplication(new UIApp());

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var scheme = Tr.Get("App.Scheme");
            if (!string.IsNullOrEmpty(scheme))
            {
                SchemeRegistration.RegisterUriScheme(scheme, Tr.Get("App.Name"));
                var args = Environment.GetCommandLineArgs();

                foreach (var arg in args)
                {
                    if (arg.Trim().StartsWith(scheme))
                    {
                        UIApp.PublishSchemeRequest(arg);
                        break;
                    }
                }
            }
        }
    }
}
