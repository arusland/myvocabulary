using System;
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
        private Word _Word;
        private readonly Brush _UnknownBrush;
        private readonly Brush _BlockedBrush;
        
        #endregion

        #region Ctors

        public WordItemControl(Word word)
        {
            Checker.NotNull(word, "word");

            InitializeComponent();

            _SelectedBrush = new SolidColorBrush(Color.FromRgb(195, 212, 252));
            _KnownBrush = Brushes.LightGreen;
            _BadKnownBrush = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            _UnknownBrush = new SolidColorBrush(Color.FromRgb(250, 116, 100));
            _BlockedBrush = Brushes.LightGray;
            BorderMain.BorderBrush = new SolidColorBrush(Color.FromRgb(141, 163, 193));
            Word = word;
            RefreshWord();
            this.ContextMenu = new ContextMenu().Duck(p =>
                {
                    p.Items.Add(new MenuItem().Duck(m =>
                    {
                        m.Header = "Edit...";
                        m.Click += MenuRename_Click;
                    }));

                    if (Word.WordRaw.EndsWith("d"))
                    {
                        p.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = "Remove -d";
                            m.Tag = "d";
                            m.Click += RemoveEnding_Click;
                        }));

                        if (Word.WordRaw.EndsWith("ed"))
                        {
                            p.Items.Add(new MenuItem().Duck(m =>
                            {
                                m.Header = "Remove -ed";
                                m.Tag = "ed";
                                m.Click += RemoveEnding_Click;
                            }));
                        }

                        if (Word.WordRaw.EndsWith("ied"))
                        {
                            p.Items.Add(new MenuItem().Duck(m =>
                            {
                                m.Header = "Form -ied -> -y";
                                m.Tag = "ied|y";
                                m.Click += ReplaceEnding_Click;
                            }));
                        }
                    } 
                    else if (Word.WordRaw.EndsWith("s"))
                    {
                        p.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = "Remove -s";
                            m.Tag = "s";
                            m.Click += RemoveEnding_Click;
                        }));

                        if (Word.WordRaw.EndsWith("es"))
                        {
                            p.Items.Add(new MenuItem().Duck(m =>
                            {
                                m.Header = "Remove -es";
                                m.Tag = "es";
                                m.Click += RemoveEnding_Click;
                            }));
                        }
                        
                        if (Word.WordRaw.EndsWith("ies"))
                        {
                            p.Items.Add(new MenuItem().Duck(m =>
                            {
                                m.Header = "Form -ies -> -y";
                                m.Tag = "ies|y";
                                m.Click += ReplaceEnding_Click;
                            }));
                        }
                    }
                    else if (Word.WordRaw.EndsWith("ly"))
                    {
                        p.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = "Remove -ly";
                            m.Tag = "ly";
                            m.Click += RemoveEnding_Click;
                        }));
                    }
                    else if (Word.WordRaw.EndsWith("ing"))
                    {
                        p.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = "Remove -ing";
                            m.Tag = "ing";
                            m.Click += RemoveEnding_Click;
                        }));

                        p.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = "Form -ing -> -e";
                            m.Tag = "ing|e";
                            m.Click += ReplaceEnding_Click;
                        }));
                    }                    
                    else if (Word.WordRaw.EndsWith("er"))
                    {
                        p.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = "Remove -er";
                            m.Tag = "er";
                            m.Click += RemoveEnding_Click;
                        }));
                    }
                    else if (Word.WordRaw.EndsWith("st"))
                    {
                        p.Items.Add(new MenuItem().Duck(m =>
                        {
                            m.Header = "Remove -st";
                            m.Tag = "st";
                            m.Click += RemoveEnding_Click;
                        }));

                        if (Word.WordRaw.EndsWith("est"))
                        {
                            p.Items.Add(new MenuItem().Duck(m =>
                            {
                                m.Header = "Remove -est";
                                m.Tag = "est";
                                m.Click += RemoveEnding_Click;
                            }));
                        }
                    }
                });
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
            switch(Word.Type)
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
            var ending = sender.To<MenuItem>().Tag.ToString();
            var newWord = Word.WordRaw.Remove(Word.WordRaw.Length - ending.Length);
            var ea = new OnWordRenameEventArgs(newWord, Word.WordRaw);
            OnRemoveEnding.DoIfNotNull(p => p(this, ea));
        }

        private void ReplaceEnding_Click(object sender, RoutedEventArgs e)
        {
            var ending = sender.To<MenuItem>().Tag.ToString().Split('|');
            var newWord = Word.WordRaw.Remove(Word.WordRaw.Length - ending[0].Length) + ending[1];
            var ea = new OnWordRenameEventArgs(newWord, Word.WordRaw);
            OnRemoveEnding.DoIfNotNull(p => p(this, ea));
        }
        
        #endregion
    }
}
