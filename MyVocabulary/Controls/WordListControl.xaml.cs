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
        #region Ctors

        public WordListControl()
        {
            InitializeComponent();
            var items = new List<Word>() 
            { 
                new Word("hello", WordType.Learned), 
                new Word("world", WordType.Learned) ,
                new Word("just", WordType.Learned) ,
                new Word("cat", WordType.Learned) ,
                new Word("cat", WordType.Learned) ,
                new Word("cat", WordType.Learned) ,
                new Word("cat", WordType.Learned) ,
                new Word("cat", WordType.Learned) ,
                new Word("cat", WordType.Learned) ,
                new Word("cat", WordType.Learned) ,
                new Word("dog", WordType.Learned) 
            };

            foreach (var item in items)
            {
                WrapPanelMain.Children.Add(new WordItemControl(item));
            }
        }

        #endregion
    }
}
