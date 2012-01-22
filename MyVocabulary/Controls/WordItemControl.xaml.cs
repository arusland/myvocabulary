using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyVocabulary.Dialogs;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;

namespace MyVocabulary.Controls
{
    internal partial class WordItemControl : UserControl
    {
        #region Fields

        private readonly Brush _SelectedBrush;
        private readonly Brush _KnownBrush;
        private readonly Brush _BadKnownBrush;
        private readonly Brush _UnknownBrush;
        
        #endregion

        #region Ctors

        public WordItemControl(Word word)
        {
            Checker.NotNull(word, "word");

            InitializeComponent();

            _SelectedBrush = new SolidColorBrush(Color.FromRgb(195, 212, 252));
            _KnownBrush = Brushes.LightGreen;
            _BadKnownBrush = Brushes.OrangeRed;
            _UnknownBrush = new SolidColorBrush(Color.FromRgb(225, 45, 45));
            Word = word;
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(141, 163, 193));
            CheckBoxMain.Content = word.WordRaw;
            RefreshBackground();
            this.ContextMenu = new ContextMenu().Duck(p =>
                {
                    p.Items.Add(new MenuItem().Duck(m =>
                    {
                        m.Header = "Edit...";
                        m.Click += Menu_Click;
                    }));
                });
        }        
       
        #endregion

        #region Properties
        
        #region Public

        public Word Word
        {
            get;
            private set;
        }
        
        public bool IsChecked
        {
            get
            {
                return CheckBoxMain.IsChecked == true;
            }
            set
            {
                CheckBoxMain.IsChecked = value;
            }
        }
        
        #endregion        

        #endregion

        #region Methods
        
        #region Private

        private void RefreshBackground()
        {
            this.Background = IsChecked ? _SelectedBrush : GetTypeBrush();
        }
        
        private Brush GetTypeBrush()
        {
            switch(Word.Type)
            {
                case WordType.Known:
                    return _KnownBrush;
                case WordType.BadKnown:
                    return _BadKnownBrush;
                case WordType.Unknown:
                    return _UnknownBrush;
                case WordType.None:
                    return Brushes.Transparent;
                default:
                    throw new InvalidOperationException("Unsupported word type: " + Word.Type.ToString());
            }
        }
        
        #endregion
        
        #endregion

        #region Events
        
        public event EventHandler OnChecked;

        public event EventHandler<OnWordRenameEventArgs> OnRename;
        
        #endregion

        #region Event Handlers

        private void CheckBoxMain_Checked(object sender, RoutedEventArgs e)
        {
            RefreshBackground();
            
            OnChecked.DoIfNotNull(p => p(this, EventArgs.Empty));
            e.Handled = true;
        }        

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CheckBoxMain.IsChecked = !CheckBoxMain.IsChecked;
            e.Handled = true;
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WordEditDialog(Word.WordRaw);
            dialog.OnRename += dialog_OnRename;

            if (dialog.ShowDialog() == true)
            {
                Word = new Word(dialog.Word, Word.Type);
            }

            dialog.OnRename -= dialog_OnRename;
        }

        private void dialog_OnRename(object sender, OnWordRenameEventArgs e)
        {
            OnRename.DoIfNotNull(p => p(this, e));
        }
        
        #endregion
    }
}
