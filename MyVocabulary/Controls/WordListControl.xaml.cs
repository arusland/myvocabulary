using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;

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
        
        #endregion

        #region Ctors

        public WordListControl(IWordListProvider provider, WordType type)
        {
            Checker.NotNull(provider, "provider");
            Checker.AreNotEqual(WordType.None, type);

            InitializeComponent();

            TextBlockStatus.Text = string.Empty;

            _SelectedCount = 0;
            _Provider = provider;
            _Type = type;
            IsModified = true;
           
            InitControls();
        }

        #endregion

        #region Properties
        
        #region Public

        public WordType Type
        {
            get { return _Type; }
        }

        public bool IsModified
        {
            get { return _IsModified; }
            set { _IsModified = value; }
        }

        public int SelectedCount
        {
            get { return _SelectedCount; }
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
            WrapPanelMain.Children.Clear();
            foreach (var item in _Provider.Get())
            {
                WrapPanelMain.Children.Add(new WordItemControl(item).Duck(p =>
                {
                    p.OnChecked += Control_OnChecked;
                }));
            }
        }
        
        #endregion
        
        #region Private

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
            var ctrl = sender.To<WordItemControl>();

            if (ctrl.IsChecked)
            {
                _SelectedCount++;
            }
            else
            {
                _SelectedCount--;
            }

            TextBlockStatus.Text = _SelectedCount > 0 ? string.Format("Selected {0} word(s)", _SelectedCount) : string.Empty;
        }
        
        private void InitControls()
        {
            switch(_Type)
            {
                case WordType.Known:
                    ButtonToKnown.Visibility = Visibility.Collapsed;
                    break;
                case WordType.BadKnown:
                    ButtonToBadKnown.Visibility = Visibility.Collapsed;
                    break;
                case WordType.Unknown:
                    ButtonToBadKnown.Visibility = Visibility.Collapsed;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported wordtype: " + _Type.ToString());
            }
        }

        private void DoOnOperationEvent(Operation operation)
        {
            if (OnOperation.IsNotNull())
            {
                var words = AllControls.Where(p => p.IsChecked).Select(p => p.Word).ToList();
                OnOperation(this, new WordsOperationEventsArgs(words, operation));
            }
        }
        
        #endregion

        #endregion

        #region Events

        public event EventHandler<WordsOperationEventsArgs> OnOperation;
        
        #endregion

        #region Event Handlers

        private void HyperLinkSelectAll_Click(object sender, RoutedEventArgs e)
        {
            AllControls.CallOnEach(p => p.IsChecked = true);
        }

        private void HyperLinkDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            AllControls.CallOnEach(p => p.IsChecked = false);
        }

        private void ButtonOperation_Click(object sender, RoutedEventArgs e)
        {
            var op = FromButton(sender.To<Button>());

            DoOnOperationEvent(op);
        }
        
        #endregion
    }
}
