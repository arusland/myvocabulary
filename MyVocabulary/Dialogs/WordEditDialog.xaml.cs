using System;
using System.Windows;
using System.Windows.Input;
using MyVocabulary.Controls;
using Shared.Extensions;
using RS = MyVocabulary.Properties.Resources;

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
            HandleOkButton();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBoxWord.Text = Word;
            TextBoxWord.Focus();
            TextBoxWord.SelectAll();
        }

        private void TextBoxWord_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleOkButton();
            }
        }

        #endregion

        #region Methods
        
        #region Private

        private void HandleOkButton()
        {
            var text = TextBoxWord.Text.Trim().ToLower();

            if (text.IsNotEmpty())
            {
                if (text != Word)
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
                else
                {
                    MessageBox.Show(RS.MESSAGEBOX_YouMustChangeWord, RS.TITLE_Warning, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }
        
        #endregion
        
        #endregion

        #region Events

        public event EventHandler<OnWordRenameEventArgs> OnRename;
        
        #endregion
    }
}
