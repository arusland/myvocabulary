using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyVocabulary.StorageProvider;
using Shared.Extensions;

namespace MyVocabulary.Controls
{
    public partial class WordItemControl : UserControl
    {
        #region Fields

        private readonly Brush _SelectedBrush;
        
        #endregion

        #region Ctors

        public WordItemControl(Word word)
        {
            InitializeComponent();

            _SelectedBrush = new SolidColorBrush(Color.FromRgb(195, 212, 252));
            Word = word;
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(141, 163, 193));
            CheckBoxMain.Content = word.WordRaw;
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

        #region Events
        
        public event EventHandler OnChecked;
        
        #endregion

        #region Event Handlers

        private void CheckBoxMain_Checked(object sender, RoutedEventArgs e)
        {
            this.Background = IsChecked ? _SelectedBrush : Brushes.Transparent;
            OnChecked.DoIfNotNull(p => p(this, EventArgs.Empty));
            e.Handled = true;
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CheckBoxMain.IsChecked = !CheckBoxMain.IsChecked;
            e.Handled = true;
        }
        
        #endregion
    }
}
