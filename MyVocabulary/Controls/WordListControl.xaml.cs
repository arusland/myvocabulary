using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyVocabulary.Dialogs;
using MyVocabulary.Helpers;
using MyVocabulary.Interfaces;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary.Controls
{
    internal partial class WordListControl : UserControl
    {
        #region Constants

        private const int INVALIDATE_Count = 50;
        
        #endregion        

        #region Fields

        private readonly IWordListProvider _Provider;
        private readonly WordType _Type;
        private bool _IsModified;
        private int _SelectedCount;
        private bool _IsActive;
        private bool _IsBlocked;
        private bool _LockSelectedCount;
        private bool _LockRefreshActive;
        private WordItemControl _LastCheckedControl;
        private readonly IMessageBox _MessageBox;

        #endregion

        #region Ctors

        public WordListControl(IWordListProvider provider, WordType type, IMessageBox messageBox)
        {
            Checker.NotNull(provider, "provider");
            Checker.NotNull(messageBox, "messageBox");
            //Checker.AreNotEqual(WordType.None, type);

            InitializeComponent();

            TextBlockStatus.Text = string.Empty;
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(154, 191, 229));

            _SelectedCount = 0;
            _Provider = provider;
            _Type = type;
            _MessageBox = messageBox;
            IsModified = true;

            InitControls();
            RefreshFilter();
        }

        #endregion

        #region Properties

        #region Public

        public IEnumerable<Word> SelectedWords
        {
            get
            {
                return AllControls.Where(p => p.IsChecked).Select(p => p.Word);
            }
        }

        public bool IsBlocked
        {
            get { return _IsBlocked; }
            set
            {
                _IsBlocked = value;

                TextBoxFilter.IsEnabled = !_IsBlocked;
                //ButtonFilterClear.IsEnabled =
                //ButtonClose.IsEnabled =
                //ButtonDelete.IsEnabled =
                //ButtonToBadKnown.IsEnabled =
                //ButtonToKnown.IsEnabled =
                //ButtonToUnknown.IsEnabled = !_IsBlocked;

                OnIsBlockedChanged.DoIfNotNull(p => p(this, EventArgs.Empty));
            }
        }

        public WordType Type
        {
            get { return _Type; }
        }

        public bool IsModified
        {
            get { return _IsModified; }
            set
            {
                _IsModified = value;

                OnModified.DoIfNotNull(p => p(this, EventArgs.Empty));

                if (IsModified && _IsActive && !_LockRefreshActive)
                {
                    IsModified = false;
                    LoadItems();
                }
            }
        }

        public int SelectedCount
        {
            get { return _SelectedCount; }
        }

        public IEnumerable<Word> Words
        {
            get
            {
                return _Provider.Get();
            }
        }

        #endregion

        #region Private

        private IEnumerable<WordItemControl> AllControls
        {
            get
            {
                return WrapPanelMain.Children.OfType<WordItemControl>();
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Public

        public void CloseTab()
        {
            if (!IsBlocked && ButtonClose.IsEnabled)
            {
                OnClose.DoIfNotNull(p => p(this, EventArgs.Empty));
            }
        }

        public void DeleteSelected()
        {
            DoOnOperationEvent(Operation.Delete);
        }

        public void EditSelected()
        {
            if (_LastCheckedControl.IsNotNull() && AllControls.Any(p => p.IsChecked && _LastCheckedControl == p) && !TextBoxFilter.IsFocused)
            {
                RenameCommand(_LastCheckedControl);
            }
        }

        public void Activate(bool forceLoad = false)
        {
            _IsActive = true;

            if (IsModified && forceLoad)
            {
                IsModified = false;
                LoadItems();
            }
        }

        public void Deactivate()
        {
            //MessageBox.Show("Deactivate: " + _Type.ToString());
            _IsActive = false;
        }

        public void LoadItems()
        {
            if (!IsBlocked)
            {
                IsBlocked = true;

                _LastCheckedControl = null;

                double oldOffset = ScrollViewerMain.VerticalOffset;
                var selectedWords = SelectedWords.Select(p => p.WordRaw).ToList();

                WrapPanelMain.Children.OfType<WordItemControl>().CallOnEach(p =>
                {
                    p.OnChecked -= Control_OnChecked;
                    p.OnRenameCommand -= Control_OnRenameCommand;
                    p.OnRemoveEnding -= Control_OnRemoveEnding;
                });

                WrapPanelMain.Children.Clear();
                RefreshSelectedCount();

                var items = _Provider.Get().ToList();
                InitProgressBar(items.Count, RS.PROGRESS_STATUS_LoadingWords);

                var fhelper = new FilterHelper(TextBoxFilter.Text);
                int i = 0;

                foreach (var item in items)
                {
                    WrapPanelMain.Children.Add(new WordItemControl(item).Duck(p =>
                    {
                        p.OnChecked += Control_OnChecked;
                        p.OnRenameCommand += Control_OnRenameCommand;
                        p.OnRemoveEnding += Control_OnRemoveEnding;
                        p.Visibility = fhelper.ShowAll || fhelper.Check(p.Word.WordRaw) ? Visibility.Visible : Visibility.Collapsed;

                        if (selectedWords.Any(g => g == item.WordRaw))
                        {
                            p.IsChecked = true;
                        }
                    }));

                    //ScrollViewerMain.ScrollToEnd();

                    if (++i % INVALIDATE_Count == 0)
                    {
                        ProgressBarMain.Value = i;
                        Dispatcher.DoEvents();
                    }
                }

                ScrollViewerMain.ScrollToVerticalOffset(oldOffset);                

                CloseProgressBar();
                IsBlocked = false;
            }
        }

        private void Control_OnRemoveEnding(object sender, OnWordRenameEventArgs e)
        {
            if (!IsBlocked)
            {
                OnRename.DoIfNotNull(p =>
                    {
                        var wasSelected = sender.To<WordItemControl>().IsChecked;

                        p(this, e);

                        if (e.Cancel)
                        {
                            _MessageBox.ShowError(RS.MESSAGEBOX_SuchWordAlreadyExists);
                        }
                        else
                        {
                            AllControls.FirstOrDefault(g => g.Word.WordRaw == e.NewWord).DoIfNotNull(g => g.IsChecked = wasSelected);
                        }
                    });
            }
        }

        private void Control_OnRenameCommand(object sender, EventArgs e)
        {
            RenameCommand(sender.To<WordItemControl>());
        }

        private void InitProgressBar(int count, string text)
        {
            ProgressBarMain.Maximum = count;
            ProgressBarMain.Minimum = 0;
            ProgressBarMain.Value = 0;
            TextBlockLoadingStatus.Text = text;
            ProgressBarMain.Visibility = Visibility.Visible;
            TextBlockLoadingStatus.Visibility = Visibility.Visible;
        }

        private void CloseProgressBar()
        {
            ProgressBarMain.Visibility = Visibility.Collapsed;
            TextBlockLoadingStatus.Visibility = Visibility.Collapsed;
        }
       
        #endregion

        #region Private

        private void RenameCommand(WordItemControl control)
        {
            if (!IsBlocked)
            {
                _LockRefreshActive = true;
                var wasSelected = control.IsChecked;
                var dialog = new WordEditDialog(control.Word.WordRaw);
                dialog.OnRename += dialog_OnRename;

                if (dialog.ShowDialog() == true)
                {
                    if (IsModified)
                    {
                        LoadItems();
                    }
                }

                dialog.OnRename -= dialog_OnRename;

                _LockRefreshActive = false;

                AllControls.FirstOrDefault(p => p.Word.WordRaw == dialog.Word).DoIfNotNull(p => p.IsChecked = wasSelected);
            }
        }

        private void dialog_OnRename(object sender, OnWordRenameEventArgs e)
        {
            OnRename.DoIfNotNull(p => p(this, e));
        }

        private void ClearFilter()
        {
            if (!IsBlocked)
            {
                TextBoxFilter.Text = FilterHelper.WaterMarkText;
                TextBoxFilter.Foreground = Brushes.Gray;
            }
        }

        private void RefreshFilter()
        {
            if (!IsBlocked)
            {
                if (!TextBoxFilter.IsFocused)
                {
                    if (TextBoxFilter.Text.IsNullOrEmpty())
                    {
                        ClearFilter();
                    }
                }
                else
                {
                    TextBoxFilter.Text = string.Empty;
                    TextBoxFilter.Foreground = Brushes.Black;
                }
            }
        }

        private Operation FromButton(Button button)
        {
            if (button == ButtonToKnown)
            {
                return Operation.MakeKnown;
            }
            else if (button == ButtonToBadKnown)
            {
                return Operation.MakeBadKnown;
            }
            else if (button == ButtonToUnknown)
            {
                return Operation.MakeUnknown;
            }
            else if (button == ButtonDelete)
            {
                return Operation.Delete;
            }
            else
            {
                throw new InvalidOperationException("Unsupported Operation: " + button.Content.IfNull(() => string.Empty));
            }
        }

        private void Control_OnChecked(object sender, EventArgs e)
        {
            _LastCheckedControl = sender.To<WordItemControl>();

            if (!_LockSelectedCount)
            {
                RefreshSelectedCount();
            }
        }

        private void Control_OnRename(object sender, OnWordRenameEventArgs e)
        {
            OnRename.DoIfNotNull(p => p(this, e));
        }

        private void RefreshSelectedCount()
        {
            _SelectedCount = AllControls.Where(p => p.IsChecked).Count();
            TextBlockStatus.Text = _SelectedCount > 0 ? string.Format("Selected {0} word(s)", _SelectedCount) : string.Empty;
        }

        private void InitControls()
        {
            ButtonToKnown.Background = Brushes.LightGreen;
            ButtonToBadKnown.Background = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            ButtonToUnknown.Background = new SolidColorBrush(Color.FromRgb(250, 116, 100));

            switch (_Type)
            {
                case WordType.Known:
                    ButtonToKnown.Visibility = Visibility.Collapsed;
                    break;
                case WordType.BadKnown:
                    ButtonToBadKnown.Visibility = Visibility.Collapsed;
                    break;
                case WordType.Unknown:
                    ButtonToUnknown.Visibility = Visibility.Collapsed;
                    break;
                case WordType.Blocked:
                case WordType.None:
                    ButtonClose.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported wordtype: " + _Type.ToString());
            }
        }

        private void DoOnOperationEvent(Operation operation)
        {
            if (!IsBlocked)
            {
                if (OnOperation.IsNotNull())
                {
                    var words = AllControls.Where(p => p.IsChecked).Select(p => p.Word).ToList();

                    if (words.Count > 0)
                    {
                        OnOperation(this, new WordsOperationEventsArgs(words, operation));
                    }
                }

                RefreshSelectedCount();

                if (!AllControls.Any(p => p.IsVisible))
                {
                    ClearFilter();
                }
            }
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler<WordsOperationEventsArgs> OnOperation;

        public event EventHandler OnModified;

        public event EventHandler OnClose;

        public event EventHandler OnIsBlockedChanged;

        public event EventHandler<OnWordRenameEventArgs> OnRename;

        #endregion

        #region Event Handlers

        private void UserControl_GotFocus(object sender, RoutedEventArgs e)
        {
            if (IsModified && _IsActive)
            {
                IsModified = false;
                LoadItems();
            }
        }

        private void HyperLinkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBlocked)
            {
                IsBlocked = true;
                _LockSelectedCount = true;
                int i = 0;
                InitProgressBar(AllControls.Count(), RS.PROGRESS_STATUS_SelectingWords);

                AllControls.CallOnEach(p =>
                {
                    if (p.IsVisible)
                    {
                        p.IsChecked = true;
                    }

                    if (++i % INVALIDATE_Count == 0)
                    {
                        ProgressBarMain.Value = i;
                        Dispatcher.DoEvents();
                    }
                });

                CloseProgressBar();

                _LockSelectedCount = false;
                RefreshSelectedCount();
                _LastCheckedControl = null;
                IsBlocked = false;
            }
        }

        private void HyperLinkDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBlocked)
            {
                IsBlocked = true;
                _LockSelectedCount = true;
                int i = 0;
                InitProgressBar(AllControls.Count(), RS.PROGRESS_STATUS_DeselectingWords);

                AllControls.CallOnEach(p =>
                    {
                        p.IsChecked = false;

                        if (++i % INVALIDATE_Count == 0)
                        {
                            ProgressBarMain.Value = i;
                            Dispatcher.DoEvents();
                        }
                    });

                CloseProgressBar();

                _LockSelectedCount = false;
                RefreshSelectedCount();
                _LastCheckedControl = null;
                IsBlocked = false;
            }
        }

        private void ButtonOperation_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBlocked)
            {
                var op = FromButton(sender.To<Button>());

                DoOnOperationEvent(op);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            CloseTab();
        }        

        private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsBlocked)
            {
                var fhelper = new FilterHelper(TextBoxFilter.Text);

                AllControls.CallOnEach(p =>
                    {
                        p.Visibility = fhelper.ShowAll || fhelper.Check(p.Word.WordRaw) ? Visibility.Visible : Visibility.Collapsed;
                    });
            }
        }

        private void ButtonFilterClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFilter();
        }

        private void TextBoxFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            RefreshFilter();
        }

        #endregion
    }
}
