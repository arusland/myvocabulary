using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary.Controls
{
    internal partial class WordListControl : UserControl
    {
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

        #endregion

        #region Ctors

        public WordListControl(IWordListProvider provider, WordType type)
        {
            Checker.NotNull(provider, "provider");
            //Checker.AreNotEqual(WordType.None, type);

            InitializeComponent();

            TextBlockStatus.Text = string.Empty;
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(154, 191, 229));

            _SelectedCount = 0;
            _Provider = provider;
            _Type = type;
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

        public void EditSelected()
        {
            if (_LastCheckedControl.IsNotNull() && AllControls.Any(p => p.IsChecked && _LastCheckedControl == p))
            {
                _LockRefreshActive = true;
                _LastCheckedControl.EditWord();

                if (IsModified)
                {
                    LoadItems();
                }
                _LockRefreshActive = false;
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

                WrapPanelMain.Children.OfType<WordItemControl>().CallOnEach(p =>
                {
                    p.OnChecked -= Control_OnChecked;
                    p.OnRename -= Control_OnRename;
                });

                WrapPanelMain.Children.Clear();
                RefreshSelectedCount();

                var text = TextBoxFilter.Text.Trim();
                bool showAll = text.Equals(RS.FILTER_Text) || text.IsEmpty();

                var items = _Provider.Get().ToList();
                InitProgressBar(items.Count);


                int i = 0;

                foreach (var item in items)
                {
                    WrapPanelMain.Children.Add(new WordItemControl(item).Duck(p =>
                    {
                        p.OnChecked += Control_OnChecked;
                        p.OnRename += Control_OnRename;
                        p.Visibility = showAll || p.Word.WordRaw.IndexOf(text) >= 0 ? Visibility.Visible : Visibility.Collapsed;
                    }));

                    //ScrollViewerMain.ScrollToEnd();

                    if (++i % 10 == 0)
                    {
                        ProgressBarMain.Value = i;
                        Dispatcher.DoEvents();
                    }
                }

                CloseProgressBar();
                IsBlocked = false;
            }
        }

        private void InitProgressBar(int count)
        {
            ProgressBarMain.Maximum = count;
            ProgressBarMain.Minimum = 0;
            ProgressBarMain.Value = 0;
            ProgressBarMain.Visibility = Visibility.Visible;
        }

        private void CloseProgressBar()
        {
            ProgressBarMain.Visibility = Visibility.Collapsed;
        }

        public void LoadItems2()
        {
            IsBlocked = true;

            _LastCheckedControl = null;

            var items = _Provider.Get().ToList();

            int min = Math.Min(WrapPanelMain.Children.Count, items.Count);

            var text = TextBoxFilter.Text.Trim();
            bool showAll = text.Equals(RS.FILTER_Text) || text.IsEmpty();
            ProgressBarMain.Maximum = items.Count;
            ProgressBarMain.Minimum = 0;
            ProgressBarMain.Value = 0;

            for (int i = 0; i < min; i++)
            {
                WrapPanelMain.Children[i].To<WordItemControl>().Word = items[i];

                if (i % 10 == 0)
                {
                    ProgressBarMain.Value = i;
                    Dispatcher.DoEvents();
                }
            }

            if (items.Count > min)
            {
                for (int i = min; i < items.Count; i++)
                {
                    WrapPanelMain.Children.Add(new WordItemControl(items[i]).Duck(p =>
                    {
                        p.OnChecked += Control_OnChecked;
                        p.OnRename += Control_OnRename;
                        p.Visibility = showAll || p.Word.WordRaw.IndexOf(text) >= 0 ? Visibility.Visible : Visibility.Collapsed;
                    }));

                    if (i % 10 == 0)
                    {
                        ProgressBarMain.Value = i;
                        Dispatcher.DoEvents();
                    }
                }
            }
            else
            {
                var toRemove = new List<WordItemControl>();

                for (int i = min; i < WrapPanelMain.Children.Count; i++)
                {
                    WrapPanelMain.Children[i].To<WordItemControl>().Duck(p =>
                        {
                            p.OnChecked -= Control_OnChecked;
                            p.OnRename -= Control_OnRename;
                            toRemove.Add(p);
                        });

                    if (i % 10 == 0)
                    {
                        ProgressBarMain.Value = i;
                        Dispatcher.DoEvents();
                    }
                }

                toRemove.CallOnEach(p => WrapPanelMain.Children.Remove(p));
            }

            ProgressBarMain.Value = items.Count;

            RefreshSelectedCount();            
            //Dispatcher.DoEvents();

            IsBlocked = false;
        }

        #endregion

        #region Private

        private void ClearFilter()
        {
            if (!IsBlocked)
            {
                TextBoxFilter.Text = RS.FILTER_Text;
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
            ButtonToUnknown.Background = new SolidColorBrush(Color.FromRgb(221, 75, 57));

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
                InitProgressBar(AllControls.Count());

                AllControls.CallOnEach(p =>
                {
                    if (p.IsVisible)
                    {
                        p.IsChecked = true;
                    }

                    if (++i % 10 == 0)
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
                InitProgressBar(AllControls.Count());

                AllControls.CallOnEach(p =>
                    {
                        p.IsChecked = false;

                        if (++i % 10 == 0)
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

                RefreshSelectedCount();
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBlocked)
            {
                OnClose.DoIfNotNull(p => p(this, EventArgs.Empty));
            }
        }

        private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsBlocked)
            {
                var text = TextBoxFilter.Text.Trim();
                bool showAll = text.Equals(RS.FILTER_Text) || text.IsEmpty();

                AllControls.CallOnEach(p =>
                    {
                        p.Visibility = showAll || p.Word.WordRaw.IndexOf(text) >= 0 ? Visibility.Visible : Visibility.Collapsed;
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
