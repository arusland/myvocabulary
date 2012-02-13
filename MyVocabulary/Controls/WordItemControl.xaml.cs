using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;

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

        #endregion

        #region Ctors

        public WordItemControl(Word word, IWordChecker wordChecker)
        {
            Checker.NotNull(word, "word");
            Checker.NotNull(wordChecker, "wordChecker");

            InitializeComponent();

            _WordChecker = wordChecker;
            _SelectedBrush = new SolidColorBrush(Color.FromRgb(195, 212, 252));
            _KnownBrush = Brushes.LightGreen;
            _BadKnownBrush = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            _UnknownBrush = new SolidColorBrush(Color.FromRgb(252, 144, 131));
            _BlockedBrush = Brushes.LightGray;
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(141, 163, 193));
            Word = word;
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

        private void MakeRenameMenu(ContextMenu menu, string ending)
        {
            if (Word.WordRaw.EndsWith(ending))
            {
                var newWord = Word.WordRaw.Remove(Word.WordRaw.Length - ending.Length);

                if (!_WordChecker.Exists(newWord))
                {
                    if (menu.Items.Count == 1)
                    {
                        menu.Items.Add(new Separator());
                    }

                    menu.Items.Add(new MenuItem().Duck(m =>
                    {
                        m.Header = string.Format("Remove -{0}", ending);
                        m.Tag = newWord;
                        m.Click += RemoveEnding_Click;
                    }));

                    menu.Items.Add(new MenuItem().Duck(m =>
                    {
                        m.Header = string.Format("Add \"{0}\"", newWord);
                        m.Tag = newWord;
                        m.Click += SplitMenu_Click;
                    }));
                }
            }
        }

        private void MakeDoubleLetterMenu(ContextMenu menu, string ending)
        {
            if (Word.WordRaw.EndsWith(ending))
            {
                if ((ending.Length + 2) <= Word.WordRaw.Length)
                {
                    var index = Word.WordRaw.Length - ending.Length - 1;

                    if (Word.WordRaw[index] == Word.WordRaw[index - 1])
                    {
                        var newEnding = Word.WordRaw[index] + ending;
                        var newWord = Word.WordRaw.Remove(Word.WordRaw.Length - newEnding.Length);

                        if (!_WordChecker.Exists(newWord))
                        {
                            if (menu.Items.Count == 1)
                            {
                                menu.Items.Add(new Separator());
                            }

                            menu.Items.Add(new MenuItem().Duck(m =>
                            {
                                m.Header = string.Format("Remove -{0}", newEnding);
                                m.Tag = newWord;
                                m.Click += RemoveEnding_Click;
                            }));

                            menu.Items.Add(new MenuItem().Duck(m =>
                            {
                                m.Header = string.Format("Add \"{0}\"", newWord);
                                m.Tag = newWord;
                                m.Click += SplitMenu_Click;
                            }));
                        }
                    }
                }
            }
        }

        private void MakeRenameMenu(ContextMenu menu, string ending, string ending2)
        {
            if (Word.WordRaw.EndsWith(ending))
            {
                var newWord = Word.WordRaw.Remove(Word.WordRaw.Length - ending.Length) + ending2;

                if (!_WordChecker.Exists(newWord))
                {
                    if (menu.Items.Count == 1)
                    {
                        menu.Items.Add(new Separator());
                    }

                    menu.Items.Add(new MenuItem().Duck(m =>
                    {
                        m.Header = string.Format("Remove -{0} -> {1}", ending, ending2);
                        m.Tag = newWord;
                        m.Click += RemoveEnding_Click;
                    }));

                    menu.Items.Add(new MenuItem().Duck(m =>
                    {
                        m.Header = string.Format("Add \"{0}\"", newWord);
                        m.Tag = newWord;
                        m.Click += SplitMenu_Click;
                    }));
                }
            }
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
                if (Word.WordRaw.EndsWith("d"))
                {
                    MakeRenameMenu(p, "d");
                    MakeRenameMenu(p, "ed");
                    MakeDoubleLetterMenu(p, "ed");
                    MakeRenameMenu(p, "ied", "y");
                }
                else if (Word.WordRaw.EndsWith("s"))
                {
                    MakeRenameMenu(p, "s");
                    MakeRenameMenu(p, "es");
                    MakeRenameMenu(p, "ies", "y");
                    MakeRenameMenu(p, "ness");
                    MakeRenameMenu(p, "less");
                }
                else if (Word.WordRaw.EndsWith("ly"))
                {
                    MakeRenameMenu(p, "ly");
                    MakeRenameMenu(p, "ly", "e");
                }
                else if (Word.WordRaw.EndsWith("ing"))
                {
                    MakeRenameMenu(p, "ing");
                    MakeRenameMenu(p, "ing", "e");
                }
                else if (Word.WordRaw.EndsWith("er"))
                {
                    MakeRenameMenu(p, "er");
                }
                else if (Word.WordRaw.EndsWith("st"))
                {
                    MakeRenameMenu(p, "st");
                    MakeRenameMenu(p, "est");
                }
                else if (Word.WordRaw.EndsWith("ion"))
                {
                    MakeRenameMenu(p, "ion", "e");
                }                
            });
        }

        private void SplitMenu_Click(object sender, RoutedEventArgs e)
        {
            var newWord = sender.To<MenuItem>().Tag.ToString();
            var ea = new OnWordAddEventArgs(newWord, Word.Type, Word.Labels);
            OnWordSplit.DoIfNotNull(p => p(this, ea));
        }

        #endregion
    }
}
