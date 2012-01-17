using Shared.Extensions;
using System.Windows;
using MyVocabulary.Controls;
using MyVocabulary.StorageProvider.Enums;
using System.Windows.Controls;

namespace MyVocabulary
{
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly bool _Inited;
        private WordListControl _LastControl;
        
        #endregion

        #region Ctors

        public MainWindow()
        {
            InitializeComponent();

            TabItemBadKnown.Content = new WordListControl(null, WordType.BadKnown);
            TabItemKnown.Content = new WordListControl(null, WordType.Known);
            TabItemUnknown.Content = new WordListControl(null, WordType.Unknown);

            TabControlMain.SelectionChanged += TabControlMain_SelectionChanged;

            _Inited = true;            
        }        
        
        #endregion

        #region Event Handlers

        private void TabControlMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Inited)
            {
                if (_LastControl.IsNotNull())
                {
                    _LastControl.Deactivate();
                }

                _LastControl = TabControlMain.SelectedItem.To<TabItem>().Content.To<WordListControl>();
                _LastControl.Activate();
            }
        }
        
        #endregion
    }
}
