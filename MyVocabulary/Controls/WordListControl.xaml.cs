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

                if (IsModified && _IsActive)
                {
                    IsModified = false;
                    LoadItems();
                }

                OnModified.DoIfNotNull(p => p(this, EventArgs.Empty));
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

        public void Activate()
        {
            _IsActive = true;
            if (IsModified)
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
            IsBlocked = true;

            WrapPanelMain.Children.OfType<WordItemControl>().CallOnEach(p =>
                {
                    p.OnChecked -= Control_OnChecked;
                    p.OnRename -= Control_OnRename;
                });

            WrapPanelMain.Children.Clear();
            RefreshSelectedCount();

            var text = TextBoxFilter.Text.Trim();
            bool showAll = text.Equals(RS.FILTER_Text) || text.IsEmpty();

            int i = 0;

            foreach (var item in _Provider.Get())
            {
                WrapPanelMain.Children.Add(new WordItemControl(item).Duck(p =>
                {
                    p.OnChecked += Control_OnChecked;
                    p.OnRename += Control_OnRename;
                    p.Visibility = showAll || p.Word.WordRaw.IndexOf(text) >= 0 ? Visibility.Visible : Visibility.Collapsed;
                }));

                ScrollViewerMain.ScrollToEnd();

                if (++i % 10 == 0)
                {
                    Dispatcher.DoEvents();
                }
            }

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
            ButtonToBadKnown.Background = Brushes.OrangeRed;
            ButtonToUnknown.Background = new SolidColorBrush(Color.FromRgb(225, 45, 45));

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

        private void HyperLinkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBlocked)
            {
                _LockSelectedCount = true;

                AllControls.CallOnEach(p =>
                {
                    if (p.IsVisible)
                    {
                        p.IsChecked = true;
                        Dispatcher.DoEvents();
                    }
                });

                _LockSelectedCount = false;
                RefreshSelectedCount();
            }
        }

        private void HyperLinkDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            if (!IsBlocked)
            {
                _LockSelectedCount = true;

                AllControls.CallOnEach(p =>
                    {
                        p.IsChecked = false;
                        Dispatcher.DoEvents();
                    });

                _LockSelectedCount = false;
                RefreshSelectedCount();
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
