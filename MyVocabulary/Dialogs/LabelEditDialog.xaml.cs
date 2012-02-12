using System;
using System.Windows;
using System.Windows.Input;
using MyVocabulary.Controls;
using MyVocabulary.StorageProvider;
using Shared.Extensions;
using Shared.Helpers;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary.Dialogs
{
    internal partial class LabelEditDialog : Window
    {
        #region Ctors

        public LabelEditDialog(WordLabel label)
        {
            Checker.NotNull(label, "label");

            InitializeComponent();

            Label = label;
        }
        
        #endregion
        
        #region Properties
        
        #region Public
        
        public WordLabel Label
        {
            get;
            private set;
        }
        
        #endregion

        #endregion

        #region Methods
        
        #region Private

        private void HandleOkButton()
        {
            var text = TextBoxWord.Text.Trim();

            if (text.IsNotEmpty())
            {
                if (text != Label.Label)
                {
                    var ea = new OnLabelEditEventArgs(new WordLabel(Label.Id, text));
                    OnLabelEdit.DoIfNotNull(p => p(this, ea));

                    if (ea.Result.IsNotNull())
                    {
                        Label = ea.Result;
                        DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show(RS.MESSAGEBOX_SuchLabelAlreadyExists, RS.TITLE_Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show(RS.MESSAGEBOX_YouMustChangeLabel, RS.TITLE_Warning, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
        }
        
        #endregion
        
        #endregion

        #region Events

        public event EventHandler<OnLabelEditEventArgs> OnLabelEdit;
        
        #endregion

        #region Event Handlers

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
            TextBoxWord.Text = Label.Label;
            TextBoxWord.Focus();
            TextBoxWord.SelectAll();
        }

        private void TextBoxWord_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleOkButton();
                e.Handled = true;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !ButtonOK.IsEnabled;
        }
        
        #endregion
    }
}
