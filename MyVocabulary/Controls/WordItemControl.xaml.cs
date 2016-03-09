using MyVocabulary.Langs;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MyVocabulary.Controls
{
    internal partial class WordItemControl : UserControl
    {
        #region Fields

        private readonly Brush _SelectedBrush;
        private readonly Brush _KnownBrush;
        private readonly Brush _BadKnownBrush;
        private readonly Brush _UnknownBrush;
        private readonly Brush _BlockedBrush;
        private Word _Word;
        private readonly IWordChecker _WordChecker;
        private readonly IWordNormalizer _WordNormalizer;
        private volatile bool _InvalidCache = true;

        #endregion

        #region Ctors

        public WordItemControl(Word word, IWordChecker wordChecker, IWordNormalizer wordNormalizer)
        {
            Checker.NotNull(word, "word");
            Checker.NotNull(wordChecker, "wordChecker");
            Checker.NotNull(wordNormalizer, "wordNormalizer");

            InitializeComponent();

            _WordChecker = wordChecker;
            _WordNormalizer = wordNormalizer;
            _SelectedBrush = new SolidColorBrush(Color.FromRgb(195, 212, 252));
            _KnownBrush = Brushes.LightGreen;
            _BadKnownBrush = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            _UnknownBrush = new SolidColorBrush(Color.FromRgb(252, 144, 131));
            _BlockedBrush = Brushes.LightGray;
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(141, 163, 193));
            Word = word;

            if (word.Labels.Count > 0)
            {
                CheckBoxMain.FontWeight = FontWeights.Bold;
            }
            RefreshWord();
            InitContextMenu();
        }

        #endregion

        #region Properties

        #region Public

        public Word Word
        {
            get
            {
                return _Word;
            }
            set
            {
                var oldWord = _Word;
                _Word = value;

                if (oldWord.IsNull() || oldWord.WordRaw != _Word.WordRaw || oldWord.Type != _Word.Type)
                {
                    RefreshWord();
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                return CheckBoxMain.IsChecked == true;
            }
            set
            {
                CheckBoxMain.IsChecked = value;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Private

        private void InitContextMenu()
        {
            this.ContextMenu = new ContextMenu().Duck(p =>
            {
                p.Items.Add(new MenuItem().Duck(m =>
                {
                    m.Header = "Edit...";
                    m.Click += MenuRename_Click;
                }));
            });

            this.ContextMenu.Opened += ContextMenu_Opened;
        }

        private void RefreshWord()
        {
            CheckBoxMain.Content = Word.WordRaw;
            RefreshBackground();
        }

        private void RefreshBackground()
        {
            this.Background = IsChecked ? _SelectedBrush : GetTypeBrush();
        }

        private Brush GetTypeBrush()
        {
            switch (Word.Type)
            {
                case WordType.Known:
                    return _KnownBrush;
                case WordType.BadKnown:
                    return _BadKnownBrush;
                case WordType.Unknown:
                    return _UnknownBrush;
                case WordType.Blocked:
                    return _BlockedBrush;
                case WordType.None:
                    return Brushes.Transparent;
                default:
                    throw new InvalidOperationException("Unsupported word type: " + Word.Type.ToString());
            }
        }

        #endregion

        #endregion

        #region Events

        public event EventHandler OnChecked;

        public event EventHandler OnRenameCommand;

        public event EventHandler<OnWordRenameEventArgs> OnRemoveEnding;

        public event EventHandler<OnWordAddEventArgs> OnWordSplit;

        #endregion

        #region Event Handlers

        private void CheckBoxMain_Checked(object sender, RoutedEventArgs e)
        {
            RefreshBackground();

            OnChecked.DoIfNotNull(p => p(this, EventArgs.Empty));
            e.Handled = true;
        }

        private void UserControl_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CheckBoxMain.IsChecked = !CheckBoxMain.IsChecked;
            e.Handled = true;
        }

        private void MenuRename_Click(object sender, RoutedEventArgs e)
        {
            OnRenameCommand.DoIfNotNull(p => p(this, EventArgs.Empty));
        }

        private void RemoveEnding_Click(object sender, RoutedEventArgs e)
        {
            var newWord = sender.To<MenuItem>().Tag.ToString();
            var ea = new OnWordRenameEventArgs(newWord, Word.WordRaw);
            OnRemoveEnding.DoIfNotNull(p => p(this, ea));
        }

        private void ContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            var removing = ContextMenu.Items.OfType<MenuItem>().Where(p => p.Tag.IsNotNull()).ToList();

            foreach (var menu in removing)
            {
                ContextMenu.Items.Remove(menu);
            }

            this.ContextMenu.Duck(p =>
            {
                var changes = _WordNormalizer.GetChanges(Word);
                changes.CallOnEach(c => WordChangeHandler(p, c));
            });
        }

        private void WordChangeHandler(ContextMenu menu, WordChange change)
        {
            switch (change.Type)
            {
                case ChangeType.AddNew:
                    menu.Items.Add(new MenuItem().Duck(m =>
                    {
                        m.Header = string.Format("Add \"{0}\"", change.NewWord);
                        m.Tag = change.NewWord;
                        m.Click += SplitMenu_Click;
                    }));
                    break;
                case ChangeType.RemoveEnd:

                    if (String.IsNullOrEmpty(change.Param2))
                    {
                        menu.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = string.Format("Remove -{0}", change.Param);
                            m.Tag = change.NewWord;
                            m.Click += RemoveEnding_Click;
                        }));
                    }
                    else
                    {
                        menu.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = string.Format("Remove -{0} -> {1}", change.Param, change.Param2);
                            m.Tag = change.NewWord;
                            m.Click += RemoveEnding_Click;
                        }));
                    }
                    break;
                default:
                    throw new InvalidOperationException("Unsupported change type: " + change.Type);
            }
        }

        private void SplitMenu_Click(object sender, RoutedEventArgs e)
        {
            var newWord = sender.To<MenuItem>().Tag.ToString();
            var ea = new OnWordAddEventArgs(newWord, Word.Type, Word.Labels);
            OnWordSplit.DoIfNotNull(p => p(this, ea));
        }

        private void UserControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_InvalidCache)
            {
                _InvalidCache = false;

                var result = new StringBuilder();

                if (Word.Labels.Any())
                {
                    result.Append("Labels:");

                    foreach (var label in Word.Labels)
                    {
                        result.AppendFormat("{0}  {1}", Environment.NewLine, label.Label);
                    }
                }

                String tooltip = _WordNormalizer.GetRenameTooltip(Word);

                if (!tooltip.IsNullOrEmpty())
                {
                    if (result.Length > 0)
                    {
                        result.Append(Environment.NewLine);
                    }
                    result.Append(tooltip);
                }

                this.ToolTip = result.Length > 0 ? result.ToString() : null;
            }
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _InvalidCache = true;
        }

        #endregion
    }
}
