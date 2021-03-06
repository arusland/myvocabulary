﻿using Microsoft.WindowsAPICodePack.Taskbar;
using MyVocabulary.Dialogs;
using MyVocabulary.Helpers;
using MyVocabulary.Interfaces;
using MyVocabulary.Langs;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary.Controls
{
    internal partial class WordListControl : UserControl, IWordNormalizer
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
        private readonly Window _MainWindow;
        private IWordNormalizer _WordNormalizer;
        private readonly WordNormalizerFactory _WordNormalizerFactory;

        #endregion

        #region Ctors

        public WordListControl(Window mainWindow, IWordListProvider provider, WordType type, IMessageBox messageBox, 
            WordNormalizerFactory wordNormalizerFactory)
        {
            Checker.NotNull(provider, "provider");
            Checker.NotNull(messageBox, "messageBox");
            Checker.NotNull(mainWindow, "mainWindow");
            Checker.NotNull(wordNormalizerFactory, "wordNormalizerFactory");

            InitializeComponent();

            LabelsVisible = false;
            _MainWindow = mainWindow;
            _WordNormalizerFactory = wordNormalizerFactory;
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

        public MyVocabulary.Langs.IWordNormalizer WordNormalizer
        {
            get
            {
                if (_WordNormalizer == null || _WordNormalizer.Lang != _Provider.Lang)
                {
                    _WordNormalizer = _WordNormalizerFactory.CreateNormalizer(_Provider.Lang);
                }

                return _WordNormalizer;
            }
        }

        private bool LabelsVisible
        {
            get
            {
                return GridLabels.Visibility == Visibility.Visible;
            }
            set
            {
                GridLabels.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

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
            _IsActive = false;
        }

        public void LoadItems()
        {
            if (!IsBlocked)
            {
                IsBlocked = true;
                LabelsVisible = false;

                _LastCheckedControl = null;

                double oldOffset = ScrollViewerMain.VerticalOffset;
                var selectedWords = SelectedWords.Select(p => p.WordRaw).ToList();

                WrapPanelMain.Children.OfType<WordItemControl>().CallOnEach(p =>
                {
                    p.OnChecked -= Control_OnChecked;
                    p.OnRenameCommand -= Control_OnRenameCommand;
                    p.OnRemoveEnding -= Control_OnRemoveEnding;
                    p.OnWordSplit -= Control_OnWordSplit;
                });

                WrapPanelMain.Children.Clear();
                WrapPanelLabels.Children.Clear();
                RefreshSelectedCount();
                var items = _Provider.Get().ToList();
                InitProgressBar(items.Count, RS.PROGRESS_STATUS_LoadingWords);
                var labels = new List<WordLabel>();

                Dispatcher.DoEvents();

                var fhelper = new FilterHelper(TextBoxFilter.Text);
                int i = 0;

                foreach (var item in items)
                {
                    WrapPanelMain.Children.Add(new WordItemControl(item, (IWordNormalizer)this).Duck(p =>
                    {
                        p.OnChecked += Control_OnChecked;
                        p.OnRenameCommand += Control_OnRenameCommand;
                        p.OnRemoveEnding += Control_OnRemoveEnding;
                        p.OnWordSplit += Control_OnWordSplit;
                        var isVisible = fhelper.ShowAll || fhelper.Check(p.Word);
                        p.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;

                        if (isVisible && selectedWords.Any(g => g == item.WordRaw))
                        {
                            p.IsChecked = true;
                        }
                    }));

                    //ScrollViewerMain.ScrollToEnd();
                    foreach (var label in item.Labels)
                    {
                        if (!labels.Any(p => p.Label == label.Label))
                        {
                            labels.Add(label);
                        }
                    }

                    if (++i % INVALIDATE_Count == 0)
                    {
                        SetProgressValue(i);
                        Dispatcher.DoEvents();
                    }
                }

                foreach (var label in labels)
                {
                    AddLabelToList(label);
                }

                ScrollViewerMain.ScrollToVerticalOffset(oldOffset);

                CloseProgressBar();
                LabelsVisible = labels.Count > 0;
                IsBlocked = false;
            }
        }

        private void AddLabelToList(WordLabel label)
        {
            var textBlock = new TextBlock()
                {
                    Margin = new Thickness(3)
                };

            textBlock.Inlines.Add(new Hyperlink().Duck(p =>
                {
                    p.Inlines.Add(label.Label);
                    p.Click += HyperLinkSelectLabel_Click;
                    p.Tag = label;
                }));

            WrapPanelLabels.Children.Add(textBlock);
        }

        private void SetProgressValue(int value)
        {
            ProgressBarMain.Value = value;

            if (TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.SetProgressValue(value, (int)ProgressBarMain.Maximum, _MainWindow);
            }
        }

        private void InitProgressBar(int count, string text)
        {
            ProgressBarMain.Maximum = count;
            ProgressBarMain.Minimum = 0;
            ProgressBarMain.Value = 0;
            TextBlockLoadingStatus.Text = text;
            ProgressBarMain.Visibility = Visibility.Visible;
            TextBlockLoadingStatus.Visibility = Visibility.Visible;

            if (TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Normal);
            }
        }

        private void CloseProgressBar()
        {
            ProgressBarMain.Visibility = Visibility.Collapsed;
            TextBlockLoadingStatus.Visibility = Visibility.Collapsed;

            if (TaskbarManager.IsPlatformSupported)
            {
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
            }
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
                        IsModified = false;
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
                RefreshFilterColor();
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
                    RefreshFilterColor();
                }
            }
        }

        private void RefreshFilterColor()
        {
            if (TextBoxFilter.Text == FilterHelper.WaterMarkText)
            {
                TextBoxFilter.Foreground = Brushes.Gray;
            }
            else if (TextBoxFilter.Text.StartsWith("label:"))
            {
                TextBoxFilter.Foreground = Brushes.Blue;
            }
            else
            {
                TextBoxFilter.Foreground = Brushes.Black;
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
            ButtonToUnknown.Background = new SolidColorBrush(Color.FromRgb(252, 144, 131));

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

        private void DoOnOperationEvent(Operation operation, object arg = null)
        {
            if (!IsBlocked)
            {
                if (OnOperation.IsNotNull())
                {
                    var words = SelectedWords.ToList();

                    if (words.Count > 0)
                    {
                        OnOperation(this, new WordsOperationEventsArgs(words, operation, arg));
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

        public event EventHandler<OnWordAddEventArgs> OnWordSplit;

        public event EventHandler<OnLabelEditEventArgs> OnLabelEdit;

        #endregion

        #region Event Handlers

        private void HyperLinkSelectLabel_Click(object sender, RoutedEventArgs e)
        {
            var label = sender.To<Hyperlink>().Tag.To<WordLabel>();
            TextBoxFilter.Text = string.Format("label:{0}", label.Label);
        }

        private void Control_OnWordSplit(object sender, OnWordAddEventArgs e)
        {
            OnWordSplit.DoIfNotNull(p =>
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
            RefreshFilterColor();

            if (!IsBlocked)
            {
                var fhelper = new FilterHelper(TextBoxFilter.Text);

                AllControls.CallOnEach(p =>
                    {
                        p.Visibility = fhelper.ShowAll || fhelper.Check(p.Word) ? Visibility.Visible : Visibility.Collapsed;

                        if (!p.IsVisible && p.IsChecked)
                        {
                            p.IsChecked = false;
                        }
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

        private void HyperLinkSetLabel_Click(object sender, RoutedEventArgs e)
        {
            var labels = _Provider.GetLabels().ToList();

            ContextMenu menu = new ContextMenu();

            foreach (var label in labels)
            {
                menu.Items.Add(new MenuItem().Duck(p =>
                {
                    p.Header = label.Label;
                    p.Tag = label;
                    p.Click += MenuSetLabel_Click;
                }));
            }

            if (labels.Count > 0)
            {
                menu.Items.Add(new Separator());
            }
            menu.Items.Add(new MenuItem().Duck(p =>
            {
                p.Header = RS.MENU_AddNewLabel;
                p.Click += MenuAddNewLabel_Click;
            }));
            menu.IsOpen = true;
        }

        private void MenuSetLabel_Click(object sender, RoutedEventArgs e)
        {
            var label = sender.To<MenuItem>().Tag.To<WordLabel>();

            if (SelectedWords.Any())
            {
                DoOnOperationEvent(Operation.SetLabel, label);
            }
            else
            {
                _MessageBox.ShowError("You should select word(s).");
            }
        }

        private void MenuAddNewLabel_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new LabelEditDialog(new WordLabel(string.Empty));
            dialog.OnLabelEdit += dialog_OnLabelEdit;

            if (dialog.ShowDialog() == true)
            {

            }
        }

        private void dialog_OnLabelEdit(object sender, OnLabelEditEventArgs e)
        {
            OnLabelEdit.DoIfNotNull(p => p(this, e));
        }

        #endregion

        #region IWordNormalizer methods


        public IEnumerable<WordChange> GetChanges(Word word)
        {
            return WordNormalizer.GetChanges(word);
        }

        public Language Lang
        {
            get { return WordNormalizer.Lang; }
        }

        public string GetRenameTooltip(Word word)
        {
            return WordNormalizer.GetRenameTooltip(word);
        }

        public bool IsPotentialForRemove(Word word)
        {
            return WordNormalizer.IsPotentialForRemove(word);
        }

        #endregion
    }
}
