using System;
using System.Windows;
using MyVocabulary.Helpers;

namespace MyVocabulary
{
    public partial class App : Application
    {
        #region Event Handlers

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = false;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // try to associate extension
                string appPath = this.GetType().Assembly.Location;
                FileAssociateHelper.AssociateFile("myvoc", "My Vocabulary File", appPath, appPath, 0);
            }
            catch (Exception)
            {
                // go ahead anyway
            }
        }
        
        #endregion
    }
}
