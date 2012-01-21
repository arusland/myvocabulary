using System.Windows;
using System.Linq;
using System.Windows.Controls;
using MyVocabulary.Controls;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using System;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary
{
    public partial class MainWindow : Window
    {
        #region Fields

        private readonly bool _Inited;
        private WordListControl _LastControl;
        private readonly XmlWordsStorageProvider _Provider;
        private string _Filename;

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
            RefreshTitle();
        }

        #endregion

        #region Methods

        #region Private

        private void ShowError(string message)
        {
            MessageBox.Show(message, RS.TITLE_Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool SaveDocument()
        {
            if (_Filename.IsNullOrEmpty())
            {
                var dialog = new System.Windows.Forms.SaveFileDialog();

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;
                }

                _Filename = dialog.FileName;
                _Provider.SetPath(_Filename);
            }

            _Provider.Save();
            RefreshTitle();

            return true;
        }

        private void RefreshTitle()
        {
            if (_Filename.IsNotNullOrEmpty())
            {
                Title = string.Format("{0} - {1}", RS.TITLE_MainWindow, _Filename);

                if (_Provider.IsModified)
                {
                    Title += "*";
                }
            }
            else
            {
                Title = RS.TITLE_MainWindow;
            }
        }

        private WordListControl GetControlByType(WordType type)
        {
            return TabControlMain.Items.OfType<TabItem>().Select(p => p.Content.To<WordListControl>()).Single(p => p.Type == type);
        }

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
            var control = sender.To<WordListControl>();

            switch (e.Operation)
            {
                case Operation.Delete:
                    if (MessageBox.Show(string.Format("Are you sure to delete selected {0} word(s)?", e.Words.Count), "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _Provider.Delete(e.Words);
                    }
                    break;
                case Operation.MakeKnown:
                    _Provider.Update(e.Words.Select(p => new Word(p.WordRaw, WordType.Known)));
                    GetControlByType(WordType.Known).IsModified = true;
                    break;
                case Operation.MakeBadKnown:
                    _Provider.Update(e.Words.Select(p => new Word(p.WordRaw, WordType.BadKnown)));
                    GetControlByType(WordType.BadKnown).IsModified = true;
                    break;
                case Operation.MakeUnknown:
                    _Provider.Update(e.Words.Select(p => new Word(p.WordRaw, WordType.Unknown)));
                    GetControlByType(WordType.Unknown).IsModified = true;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported Operation: " + e.Operation.ToString());
            }

            control.IsModified = true;
            RefreshTitle();
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

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            SaveDocument();
        }        

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            if (_Provider.IsModified)
            {
                var result = MessageBox.Show(RS.MESSAGEBOX_DocumentModified, RS.TITLE_Warning, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    return;
                }

                if (!SaveDocument())
                {
                    return;
                }
            }

            var dialog = new System.Windows.Forms.OpenFileDialog();

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            _Filename = dialog.FileName;
            
            try
            {
                _Provider.Load(_Filename);
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        #endregion
    }
}
