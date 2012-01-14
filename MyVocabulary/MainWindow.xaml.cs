using System.Windows;
using MyVocabulary.Controls;
using MyVocabulary.StorageProvider.Enums;

namespace MyVocabulary
{
    public partial class MainWindow : Window
    {
        #region Ctors

        public MainWindow()
        {
            InitializeComponent();

            TabItemBadKnown.Content = new WordListControl(null, WordType.BadKnown);
            TabItemKnown.Content = new WordListControl(null, WordType.Known);
            TabItemUnknown.Content = new WordListControl(null, WordType.Unknown);
        }
        
        #endregion        
    }
}
