using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;

namespace MyVocabulary.Controls
{
    public partial class WordListControl : UserControl
    {
        #region Fields

        private readonly IWordsStorageProvider _Provider;
        private readonly WordType _Type;
        
        #endregion

        #region Ctors

        public WordListControl(IWordsStorageProvider provider, WordType type)
        {
            InitializeComponent();

            _Provider = provider;
            _Type = type;

            var items = new List<Word>() 
            { 
                new Word("hello", WordType.Known), 
                new Word("world", WordType.Known) ,
                new Word("just", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("dog", WordType.Known) 
            };

            foreach (var item in items)
            {
                WrapPanelMain.Children.Add(new WordItemControl(item));
            }

            InitControls();

            //for (int i = 0; i < 5000;i++)
            //{
            //    WrapPanelMain.Children.Add(new WordItemControl(new Word("kisa-" + i.ToString(), WordType.Learned)));
            //}
        }

        #endregion

        #region Methods
        
        #region Private
        
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
        
        #endregion
        
        #endregion
    }
}
