using System.Windows.Controls;
using MyVocabulary.StorageProvider;
using System.Windows.Media;
using System;

namespace MyVocabulary.Controls
{
    public partial class WordItemControl : UserControl
    {
        #region Ctors

        public WordItemControl(Word word)
        {
            InitializeComponent();
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(141, 163, 193));
            CheckBoxMain.Content = word.WordRaw;
        }
       
        #endregion

        #region Properties
        
        #region Public
        
        public bool IsCheacked
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

        private void CheckBoxMain_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }
        
        #endregion
    }
}
