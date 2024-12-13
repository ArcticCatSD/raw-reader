using System.Configuration;
using System.Data;
using System.Windows;

namespace RawReader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            base.OnStartup(e);
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception)e.ExceptionObject;
            _ = MessageBox.Show(ex.ToString(), "What The Heck!!!", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
