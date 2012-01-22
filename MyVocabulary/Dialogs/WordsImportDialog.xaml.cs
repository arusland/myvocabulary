using System.Linq;
using System.Windows;
using System.Windows.Media;
using MyVocabulary.Helpers;

namespace MyVocabulary.Dialogs
{
    public partial class WordsImportDialog : Window
    {
        #region Fields

        private string[] _Words;
        
        #endregion

        #region Ctors

        public WordsImportDialog()
        {
            InitializeComponent();
            //BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(43, 145, 175));
        }
        
        #endregion

        #region Properties
        
        #region Public
        
        public string[] Words
        {
            get
            {
                return _Words ?? new string[0];
            }            
        }
        
        #endregion
        
        #endregion

        #region Event Handlers

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonProcess_Click(object sender, RoutedEventArgs e)
        {
            var words = new WordsFinder().Parse(TextBoxMain.Text).ToArray();

            if (words.Length > 0)
            {
                _Words = words;
                DialogResult = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxMain.Focus();
        }
        
        #endregion
    }
}
