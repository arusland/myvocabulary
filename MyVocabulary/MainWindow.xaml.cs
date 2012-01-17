using System.Windows;
using System.Windows.Controls;
using MyVocabulary.Controls;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;

namespace MyVocabulary
{
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly bool _Inited;
        private WordListControl _LastControl;
        private readonly XmlWordsStorageProvider _Provider;
        
        #endregion

        #region Ctors

        public MainWindow()
        {
            InitializeComponent();

            _Provider = new XmlWordsStorageProvider();

            TabItemBadKnown.Content = CreateListControl(WordType.BadKnown);
            TabItemKnown.Content = CreateListControl(WordType.Known);
            TabItemUnknown.Content = CreateListControl(WordType.Unknown);

            TabControlMain.SelectionChanged += TabControlMain_SelectionChanged;

            _Inited = true;            
        }        
        
        #endregion

        #region Methods
        
        #region Private
        
        private WordListProvider CreateProvider(WordType type)
        {
            return new WordListProvider(_Provider, type);
        }

        private WordListControl CreateListControl(WordType type)
        {
            return new WordListControl(CreateProvider(type), type).Duck(p =>
                {
                    p.OnOperation += ListControl_OnOperation;
                });
        }        
        
        #endregion
        
        #endregion

        #region Event Handlers

        private void ListControl_OnOperation(object sender, WordsOperationEventsArgs e)
        {
            MessageBox.Show(string.Format("Count: {0}; Operation: {1}", e.Words.Count, e.Operation));
        }

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
