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
        #region Ctors

        public WordItemControl(Word word)
        {
            InitializeComponent();

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
            OnChecked.DoIfNotNull(p => p(this, EventArgs.Empty));
        }
        
        #endregion
    }
}
