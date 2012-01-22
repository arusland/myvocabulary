using System;
using System.Windows;
using System.Windows.Media;

namespace MyVocabulary
{
    public partial class WordsImportDialog : Window
    {
        #region Ctors

        public WordsImportDialog()
        {
            InitializeComponent();
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(43, 145, 175));
        }
        
        #endregion

        #region Event Handlers

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonProcess_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
            DialogResult = true;
        }
        
        #endregion
    }
}
