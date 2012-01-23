using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyVocabulary.Controls;
using MyVocabulary.Dialogs;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary
{
    public partial class MainWindow : Window
    {
        #region Constants

        private const string DIALOG_Filter = "My Vocabulary Files (*.myvoc)|*.myvoc|All Files (*.*)|*.*";
        private const string DEFAULT_Extension = "*.myvoc";
        
        #endregion        

        #region Fields

        private readonly bool _Inited;
        private WordListControl _LastControl;
        private readonly IWordsStorageProvider _Provider;
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
            RefreshTabHeader(WordType.Known);
            RefreshTabHeader(WordType.BadKnown);
            RefreshTabHeader(WordType.Unknown);
        }

        #endregion

        #region Properties
        
        #region Private
        
        private bool IsAnyTabBlocked
        {
            get
            {
                return TabControlMain.Items.Cast<TabItem>().Where(p => p.Content.IsNotNull()).Select(p => p.Content.To<WordListControl>()).Any(p => p.IsBlocked);
            }
        }
        
        #endregion
        
        #endregion

        #region Methods

        #region Private

        private bool CloseImportTab()
        {
            if (TabItemImport.IsVisible)
            {
                var control = TabItemImport.Content.To<WordListControl>();

                if (control.Words.Any())
                {
                    var result = MessageBox.Show(RS.MESSAGEBOX_SureToCloseImport, RS.TITLE_Warning, MessageBoxButton.OKCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Cancel)
                    {
                        return false;
                    }
                }

                TabItemImport.Content = null;
                control.Tag = null;
                TabItemImport.Visibility = Visibility.Collapsed;
                TabControlMain.SelectedIndex = 0;
            }

            return true;
        }

        private void OpenFile(string filename)
        {
            try
            {
                _Provider.To<XmlWordsStorageProvider>().Load(filename);
                _Filename = filename;
                GetControlByType(WordType.Unknown).IsModified = true;
                GetControlByType(WordType.BadKnown).IsModified = true;
                GetControlByType(WordType.Known).IsModified = true;
                RefreshTitle();
            }
            catch (System.Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void RefreshControls()
        {
            bool isBlocked = IsAnyTabBlocked;

            TabControlMain.Items.Cast<TabItem>().Where(p => p.Content.IsNotNull()).CallOnEach(p =>
                {
                    var control = p.Content.To<WordListControl>();

                    if (!control.IsBlocked || !isBlocked)
                    {
                        p.IsEnabled = !isBlocked;
                    }
                });

           // TabControlMain.IsEnabled = 
            ButtonSave.IsEnabled =
            ButtonOpen.IsEnabled =
            ButtonImport.IsEnabled = !isBlocked;
        }

        private void RefreshTabHeader(WordType type)
        {
            var control = GetControlByType(type);
            var tab = GetTabByType(control.Type);
            var count = control.Words.Count();

            tab.Header = count > 0 ? string.Format("{0} ({1})", GetTypeString(control.Type), count) : GetTypeString(control.Type);
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, RS.TITLE_Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool SaveDocument()
        {
            if (_Filename.IsNullOrEmpty())
            {
                var dialog = new System.Windows.Forms.SaveFileDialog()
                {
                    Filter = DIALOG_Filter,
                    DefaultExt = DEFAULT_Extension
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                {
                    return false;
                }

                _Filename = dialog.FileName;
                _Provider.To<XmlWordsStorageProvider>().SetPath(_Filename);
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
            return GetTabByType(type).Content.To<WordListControl>();
        }

        private TabItem GetTabByType(WordType type)
        {
            return TabControlMain.Items.OfType<TabItem>().Where(p => p.Content.IsNotNull()).Single(p => p.Content.To<WordListControl>().Type == type);
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
                    p.OnModified += ListControl_OnModified;
                    p.OnRename += ListControl_OnRename;
                    p.OnIsBlockedChanged += ListControl_OnIsBlockedChanged;
                });
        }        

        private WordListControl CreateImportListControl(string[] words)
        {
            var provider = new WordListImportProvider(words);

            var result = new WordListControl(provider, WordType.None).Duck(p =>
            {
                p.OnOperation += ListControl_OnOperation;
                p.OnModified += Import_OnModified;
                p.OnIsBlockedChanged += ListControl_OnIsBlockedChanged;
                p.OnClose += Import_OnClose;
                p.OnRename += Import_OnRename;
            });

            result.Tag = provider;

            return result;
        }                

        #endregion

        #endregion

        #region Event Handlers

        private void Import_OnRename(object sender, OnWordRenameEventArgs e)
        {
            var control = sender.To<WordListControl>();
            var provider = control.Tag.To<IWordsStorageImportProvider>();

            e.Cancel = !provider.Rename(e.OldWord, e.NewWord);

            if (!e.Cancel)
            {
                control.IsModified = true;
            }
        }

        private void Import_OnClose(object sender, EventArgs e)
        {
            CloseImportTab();
        }        

        private void Import_OnModified(object sender, EventArgs e)
        {
            var control = sender.To<WordListControl>();

            TabItemImport.Header = string.Format("{0} ({1})", RS.TAB_HEADER_Import, control.Words.Count());
        }        

        private void ListControl_OnModified(object sender, EventArgs e)
        {
            var control = sender.To<WordListControl>();
            RefreshTabHeader(control.Type);
        }

        private void ListControl_OnIsBlockedChanged(object sender, EventArgs e)
        {
            RefreshControls();
        }
        
        private void ListControl_OnRename(object sender, OnWordRenameEventArgs e)
        {
            e.Cancel = !_Provider.Rename(e.OldWord, e.NewWord);

            if (!e.Cancel)
            {
                var control = sender.To<WordListControl>();
                control.IsModified = true;
            }
        }

        private static string GetTypeString(WordType type)
        {
            switch(type)
            {
                case WordType.Known:
                    return RS.WORD_TYPE_Known;
                case WordType.BadKnown:
                    return RS.WORD_TYPE_BadKnown;
                case WordType.Unknown:
                    return RS.WORD_TYPE_Unknown;
                default:
                    throw new InvalidOperationException("Unsupported wordtype: " + type.ToString());
            }
        }

        private void ListControl_OnOperation(object sender, WordsOperationEventsArgs e)
        {
            var control = sender.To<WordListControl>();
            bool fromImport = TabItemImport.IsVisible && control.Type == WordType.None;

            switch (e.Operation)
            {
                case Operation.Delete:
                    if (MessageBox.Show(string.Format(RS.MESSAGEBOX_SureDeleteSelectedWords, e.Words.Count), RS.TITLE_Warning,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        if (fromImport)
                        {
                            var provider = control.Tag.To<IWordsStorageImportProvider>();
                            provider.Delete(e.Words);
                            return;
                        }
                        else
                        {
                            _Provider.Delete(e.Words);
                        }
                    }
                    else
                    {
                        return;
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

            if (fromImport)
            {
                var provider = control.Tag.To<IWordsStorageImportProvider>();
                provider.Delete(e.Words);
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

                if (result == MessageBoxResult.Yes)
                {
                    if (!SaveDocument())
                    {
                        return;
                    }
                }
            }

            var dialog = new System.Windows.Forms.OpenFileDialog()
                {
                    Filter = DIALOG_Filter,
                    DefaultExt = DEFAULT_Extension
                };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                return;
            }

            OpenFile(dialog.FileName);
        }        

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsAnyTabBlocked)
            {
                e.Cancel = true;
                return;
            }

            if (TabItemImport.IsVisible && TabItemImport.Content.To<WordListControl>().Words.Any())
            {
                var result = MessageBox.Show(RS.MESSAGEBOX_HaveWordsInImportTab, RS.TITLE_Warning, MessageBoxButton.OKCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (_Provider.IsModified)
            {
                var result = MessageBox.Show(RS.MESSAGEBOX_DocumentModified, RS.TITLE_Warning, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == MessageBoxResult.Yes)
                {
                    if (!SaveDocument())
                    {
                        e.Cancel = true;
                        return;
                    }
                }
            }
        }

        private void ButtonImport_Click(object sender, RoutedEventArgs e)
        {
            if (CloseImportTab())
            {
                var dialog = new WordsImportDialog();

                if (dialog.ShowDialog() == true)
                {
                    TabItemImport.Visibility = Visibility.Visible;
                    TabItemImport.Content = CreateImportListControl(dialog.Words);
                    TabControlMain.SelectedItem = TabItemImport;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                OpenFile(Environment.GetCommandLineArgs()[1]);
            }
        }

        #endregion
    }
}
