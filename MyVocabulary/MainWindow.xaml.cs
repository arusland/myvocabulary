using MyVocabulary.Controls;
using MyVocabulary.Dialogs;
using MyVocabulary.Interfaces;
using MyVocabulary.Langs;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary
{
    public partial class MainWindow : Window, IMessageBox, IWordChecker
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
        private readonly FileSystemWatcher _FileWatcher;
        private DateTime _LastFileTime;
        private readonly WordNormalizerFactory _WordNormalizerFactory;

        #endregion

        #region Ctors

        public MainWindow()
        {
            InitializeComponent();

            _Provider = new XmlWordsStorageProvider();
            _WordNormalizerFactory = new WordNormalizerFactory((IWordChecker)this);
            // TODO: Language must be selectable!
            _Provider.Lang = MyVocabulary.StorageProvider.Enums.Language.English;
            _FileWatcher = new FileSystemWatcher();
            _FileWatcher.Changed += FileWatcher_Changed;

            TabItemBadKnown.Content = CreateListControl(WordType.BadKnown);
            TabItemKnown.Content = CreateListControl(WordType.Known);
            TabItemUnknown.Content = CreateListControl(WordType.Unknown);
            TabItemBlocked.Content = CreateListControl(WordType.Blocked);

            TabControlMain.SelectionChanged += TabControlMain_SelectionChanged;

            _Inited = true;
            RefreshTitle();
            RefreshTabHeader(WordType.Known);
            RefreshTabHeader(WordType.BadKnown);
            RefreshTabHeader(WordType.Unknown);
            RefreshTabHeader(WordType.Blocked);

            this.ContextMenu = new ContextMenu().Duck(p =>
            {
                p.Items.Add(new MenuItem().Duck(m =>
                {
                    m.Header = "Show Blocked Words...";
                    m.Click += MenuShowBlockedTab_Click;
                }));
            });
        }

        #endregion

        #region Properties

        #region Private

        private string VersionString
        {
            get
            {
                var ver = this.GetType().Assembly.GetName().Version;

                return string.Format("{0}.{1}", ver.Major, ver.Minor);
            }
        }

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

        private void AnimateLabel(Label label, Color color, int durationTimeInSeconds)
        {
            LabelMessage.Visibility = Visibility.Visible;
            NameScope.SetNameScope(label, new NameScope());
            label.Background = new SolidColorBrush(color);
            label.RegisterName("Brush", label.Background);

            ColorAnimation highlightAnimation = new ColorAnimation();
            highlightAnimation.Completed += (s, e) =>
            {
                LabelMessage.Visibility = Visibility.Collapsed;
            };
            highlightAnimation.To = Colors.Transparent;
            highlightAnimation.Duration = TimeSpan.FromSeconds(durationTimeInSeconds);

            Storyboard.SetTargetName(highlightAnimation, "Brush");
            Storyboard.SetTargetProperty(highlightAnimation, new PropertyPath(SolidColorBrush.ColorProperty));

            Storyboard sb = new Storyboard();
            sb.Children.Add(highlightAnimation);
            sb.Begin(label);
        }

        private void OnEdit()
        {
            TabControlMain.SelectedItem.To<TabItem>().Content.To<WordListControl>().EditSelected();
        }

        private void OnDelete()
        {
            TabControlMain.SelectedItem.To<TabItem>().Content.To<WordListControl>().DeleteSelected();
        }

        private void OnCloseCurrentTab()
        {
            var control = TabControlMain.SelectedItem.To<TabItem>().Content.To<WordListControl>();

            if (control.Type == WordType.None || control.Type == WordType.Blocked)
            {
                control.CloseTab();
            }
        }

        private void OnCopy()
        {
            var words = TabControlMain.SelectedItem.To<TabItem>().Content.To<WordListControl>().SelectedWords.Select(p => p.WordRaw).ToList();

            var builder = new StringBuilder();

            foreach (var word in words)
            {
                builder.AppendFormat("{0} ", word);
            }

            try
            {
                Clipboard.SetText(builder.ToString().TrimEnd());
            }
            catch (Exception)
            {
                // to skip dummy clipboard exception
            }
        }

        private void ShowBlockedTab()
        {
            var control = TabItemBlocked.Content.To<WordListControl>();
            TabItemBlocked.Visibility = Visibility.Visible;
            TabControlMain.SelectedItem = TabItemBlocked;
            TabControlMain.Focus();
            Dispatcher.DoEvents();
            control.Activate(true);
        }

        private void CloseBlockedTab()
        {
            TabItemBlocked.Visibility = Visibility.Collapsed;
            TabControlMain.SelectedItem = TabItemBadKnown;
            TabControlMain.Focus();
        }

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
                TabControlMain.SelectedItem = TabItemBadKnown;
                TabControlMain.Focus();
            }

            return true;
        }

        private void OpenFileInternal(string filename)
        {
            try
            {
                _Provider.To<XmlWordsStorageProvider>().Load(filename);
                _Filename = filename;

                var normalizer = _WordNormalizerFactory.CreateNormalizer(_Provider.Lang);
                var sw = Stopwatch.StartNew();
                var wordsToRemove = _Provider.Get().Where(p => normalizer.IsPotentialForRemove(p)).ToList();
                sw.Stop();
                Debug.WriteLine("Getting list from provider: " + sw.ElapsedMilliseconds);
                _Provider.SetLabel(wordsToRemove, WordLabel.LabelToRemove);

                GetControlByType(WordType.Unknown).IsModified = true;
                GetControlByType(WordType.BadKnown).IsModified = true;
                GetControlByType(WordType.Known).IsModified = true;
                GetControlByType(WordType.Blocked).IsModified = true;
                RefreshTitle();
                WatchFile(_Filename);
            }
            catch (System.Exception ex)
            {
                ShowErrorBox(ex.Message);
            }
        }

        private void OpenFile(string filename)
        {
            OpenFileInternal(filename);
        }

        private void WatchFile(string filename)
        {
            _FileWatcher.Path = Path.GetDirectoryName(filename);
            _FileWatcher.EnableRaisingEvents = true;
            _LastFileTime = File.GetLastWriteTime(filename);
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
            RefreshTotalCount();
        }

        private void RefreshTotalCount()
        {
            int totalCount = GetControlByType(WordType.Known).Words.Count() + GetControlByType(WordType.BadKnown).Words.Count() +
                GetControlByType(WordType.Unknown).Words.Count();

            TextBlockTotalCount.Text = totalCount > 0 ? string.Format(RS.XAML_TotalCount, totalCount) : string.Empty;
        }

        private void ShowErrorBox(string message)
        {
            MessageBox.Show(message, RS.TITLE_Error, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void OpenDocument()
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

        private void ImportNewWords()
        {
            if (CloseImportTab())
            {
                var dialog = new WordsImportDialog();

                if (dialog.ShowDialog() == true)
                {
                    var list = _Provider.Get().Where(p => p.Type == WordType.Known || p.Type == WordType.Blocked ||
                        ((p.Type == WordType.BadKnown || p.Type == WordType.Unknown) && !dialog.UseOnlyKnownTab)).ToList();
                    var filtered = dialog.Words.Where(p => !list.BinarySearchFound(w => w.WordRaw.CompareTo(p))).ToArray();

                    var control = CreateImportListControl(filtered);
                    TabItemImport.Visibility = Visibility.Visible;
                    TabItemImport.Content = control;
                    TabControlMain.SelectedItem = TabItemImport;
                    Dispatcher.DoEvents();
                    control.Activate(true);
                }
            }
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

            try
            {
                _Provider.Save();
                _LastFileTime = File.GetLastWriteTime(_Filename);
                RefreshTitle();
            }
            catch (System.Exception ex)
            {
                ShowErrorBox(ex.Message);
                return false;
            }

            return true;
        }

        private void RefreshTitle()
        {
            if (_Filename.IsNotNullOrEmpty())
            {
                Title = string.Format("{0} {2} - {1}", RS.TITLE_MainWindow, _Filename, VersionString);

                if (_Provider.IsModified)
                {
                    Title += "*";
                }
            }
            else
            {
                Title = string.Format("{0} {1} - Untitled", RS.TITLE_MainWindow, VersionString);
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
            return new WordListControl(this, CreateProvider(type), type, (IMessageBox)this, _WordNormalizerFactory).Duck(p =>
                {
                    p.OnOperation += ListControl_OnOperation;
                    p.OnModified += ListControl_OnModified;
                    p.OnRename += ListControl_OnRename;
                    p.OnClose += Import_OnClose;
                    p.OnIsBlockedChanged += ListControl_OnIsBlockedChanged;
                    p.OnWordSplit += ListControl_OnWordSplit;
                    p.OnLabelEdit += ListControl_OnLabelEdit;
                });
        }

        private WordListControl CreateImportListControl(string[] words)
        {
            var provider = new WordListImportProvider(words, new List<WordLabel>(), CreateProvider(WordType.Unknown));
            var normalizer = _WordNormalizerFactory.CreateNormalizer(_Provider.Lang);

            var wordsToRemove = provider.Get().Where(p => normalizer.IsPotentialForRemove(p)).ToList();

            provider.SetLabel(wordsToRemove, WordLabel.LabelToRemove);

            var result = new WordListControl((Window)this, provider, WordType.None, (IMessageBox)this,
                _WordNormalizerFactory).Duck(p =>
            {
                p.OnOperation += ListControl_OnOperation;
                p.OnModified += Import_OnModified;
                p.OnIsBlockedChanged += ListControl_OnIsBlockedChanged;
                p.OnClose += Import_OnClose;
                p.OnRename += Import_OnRename;
                p.OnWordSplit += Import_OnWordSplit;
            });

            result.Tag = provider;

            return result;
        }

        #endregion

        #endregion

        #region Event Handlers

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.SynchronizeCall(() =>
                {
                    if (_Filename.IsNotNullOrEmpty())
                    {
                        System.Threading.Thread.Sleep(500);
                        var time = File.GetLastWriteTime(_Filename);

                        if (!time.Equals(_LastFileTime))
                        {
                            _LastFileTime = time;
                            Activate();

                            var result = MessageBox.Show(this, RS.MESSAGEBOX_ReloadChanges,
                                    RS.TITLE_Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                            if (result == MessageBoxResult.Yes)
                            {
                                if (_Provider.IsModified)
                                {
                                    var result2 = MessageBox.Show(this, RS.MESSAGEBOX_AbandonChanges,
                                        RS.TITLE_Warning, MessageBoxButton.YesNo, MessageBoxImage.Warning);

                                    if (result2 == MessageBoxResult.Yes)
                                    {
                                        OpenFile(_Filename);
                                    }
                                }
                                else
                                {
                                    OpenFile(_Filename);
                                }
                            }
                        }
                    }
                });
        }

        private void MenuShowBlockedTab_Click(object sender, RoutedEventArgs e)
        {
            ShowBlockedTab();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!IsAnyTabBlocked)
            {
                switch (e.Key)
                {
                    case Key.C:
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            OnCopy();
                            e.Handled = true;
                        }
                        break;
                    case Key.E:
                        if (Keyboard.Modifiers == ModifierKeys.None)
                        {
                            OnEdit();
                        }
                        break;
                    case Key.S:
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            SaveDocument();
                            e.Handled = true;
                        }
                        break;
                    case Key.O:
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            OpenDocument();
                            e.Handled = true;
                        }
                        break;
                    case Key.I:
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            ImportNewWords();
                            e.Handled = true;
                        }
                        break;
                    case Key.W:
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                        {
                            OnCloseCurrentTab();
                            e.Handled = true;
                        }
                        break;
                    case Key.F8:
                        OnDelete();
                        break;
                }
            }
            else
            {
                e.Handled = true;
            }
        }

        private void Import_OnWordSplit(object sender, OnWordAddEventArgs e)
        {
            var control = sender.To<WordListControl>();
            var provider = control.Tag.To<IWordsStorageImportProvider>();

            provider.Update(new List<Word> { new Word(e.NewWord, e.Type, e.Labels) });

            control.IsModified = true;
        }

        private void Import_OnRename(object sender, OnWordRenameEventArgs e)
        {
            var control = sender.To<WordListControl>();
            var provider = control.Tag.To<IWordsStorageImportProvider>();

            e.Cancel = !provider.Rename(e.OldWord, e.NewWord);

            if (!e.Cancel)
            {
                if (_Provider.Get().Any(p => p.WordRaw == e.NewWord))
                {
                    provider.Rename(e.NewWord, e.OldWord);
                    e.Cancel = true;
                }
                else
                {
                    control.IsModified = true;
                }
            }
        }

        private void Import_OnClose(object sender, EventArgs e)
        {
            var control = sender.To<WordListControl>();

            if (control.Type == WordType.None)
            {
                CloseImportTab();
            }
            else if (control.Type == WordType.Blocked)
            {
                CloseBlockedTab();
            }
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

        private void ListControl_OnWordSplit(object sender, OnWordAddEventArgs e)
        {
            _Provider.Update(new List<Word>() { new Word(e.NewWord, e.Type, e.Labels) });
            GetControlByType(e.Type).IsModified = true;
            RefreshTitle();
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
                RefreshTitle();
            }
        }

        private static string GetTypeString(WordType type)
        {
            switch (type)
            {
                case WordType.Known:
                    return RS.WORD_TYPE_Known;
                case WordType.BadKnown:
                    return RS.WORD_TYPE_BadKnown;
                case WordType.Unknown:
                    return RS.WORD_TYPE_Unknown;
                case WordType.Blocked:
                    return RS.WORD_TYPE_Blocked;
                default:
                    throw new InvalidOperationException("Unsupported wordtype: " + type.ToString());
            }
        }

        private void ListControl_OnOperation(object sender, WordsOperationEventsArgs e)
        {
            var control = sender.To<WordListControl>();
            bool fromImport = TabItemImport.IsVisible && control.Type == WordType.None;
            bool shifted = (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;

            switch (e.Operation)
            {
                case Operation.Delete:
                    if (control.Type == WordType.Blocked || shifted)
                    {
                        if (MessageBox.Show(string.Format(RS.MESSAGEBOX_SureDeleteSelectedWords, e.Words.Count), RS.TITLE_Warning,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            _Provider.Delete(e.Words);
                        }
                        else
                            return;
                    }
                    else
                    {
                        _Provider.Update(e.Words.Select(p => new Word(p.WordRaw, WordType.Blocked, p.Labels)));
                        GetControlByType(WordType.Blocked).IsModified = true;
                    }
                    break;
                case Operation.MakeKnown:
                    _Provider.Update(e.Words.Select(p => new Word(p.WordRaw, WordType.Known, p.Labels)));
                    GetControlByType(WordType.Known).IsModified = true;
                    break;
                case Operation.MakeBadKnown:
                    _Provider.Update(e.Words.Select(p => new Word(p.WordRaw, WordType.BadKnown, p.Labels)));
                    GetControlByType(WordType.BadKnown).IsModified = true;
                    break;
                case Operation.MakeUnknown:
                    _Provider.Update(e.Words.Select(p => new Word(p.WordRaw, WordType.Unknown, p.Labels)));
                    GetControlByType(WordType.Unknown).IsModified = true;
                    break;
                case Operation.SetLabel:
                    if (!fromImport)
                    {
                        var label = e.Arg.To<WordLabel>();

                        _Provider.SetLabel(e.Words, label);
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unsupported Operation: " + e.Operation.ToString());
            }

            if (fromImport)
            {
                var provider = control.Tag.To<IWordsStorageImportProvider>();

                if (e.Operation == Operation.SetLabel)
                {
                    var label = e.Arg.To<WordLabel>();
                    provider.SetLabel(e.Words, label);
                }
                else
                {
                    provider.Delete(e.Words);
                }
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
            OpenDocument();
            return;
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
            ImportNewWords();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Environment.GetCommandLineArgs().Length > 1)
            {
                OpenFile(Environment.GetCommandLineArgs()[1]);
            }
        }

        private void ListControl_OnLabelEdit(object sender, OnLabelEditEventArgs e)
        {
            e.Result = _Provider.UpdateLabel(e.Label);
        }

        #endregion

        #region IMessageBox

        void IMessageBox.ShowError(string message)
        {
            SystemSounds.Beep.Play();
            LabelMessage.Content = message;
            AnimateLabel(LabelMessage, Colors.Red, 8);
        }

        #endregion

        #region IWordChecker

        public bool Exists(string word)
        {
            return _Provider.Exists(word);
        }

        public Word GetByName(string word)
        {
            return _Provider.GetByName(word);
        }

        #endregion
    }
}
