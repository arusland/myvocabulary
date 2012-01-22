using System;
using RS = MyVocabulary.Properties.Resources;
using System.Windows;
using MyVocabulary.Controls;
using Shared.Extensions;

namespace MyVocabulary.Dialogs
{
    internal partial class WordEditDialog : Window
    {
        #region Ctors

        public WordEditDialog(string word)
        {
            InitializeComponent();

            Word = word.IfNull(() => string.Empty);
        }
        
        #endregion
        
        #region Properties
        
        #region Public
        
        public string Word
        {
            get;
            private set;
        }
        
        #endregion

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var text = TextBoxWord.Text.Trim();

            if (text.IsNotEmpty())
            {
                var ea = new OnWordRenameEventArgs(text, Word);

                OnRename.DoIfNotNull(p => p(this, ea));

                if (!ea.Cancel)
                {
                    Word = text;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show(RS.MESSAGEBOX_SuchWordAlreadyExists, RS.TITLE_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxWord.Text = Word;
        }
        
        #endregion

        #region Events

        public event EventHandler<OnWordRenameEventArgs> OnRename;
        
        #endregion
    }
}
