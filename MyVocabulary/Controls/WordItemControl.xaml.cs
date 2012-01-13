using System.Windows.Controls;
using MyVocabulary.StorageProvider;

namespace MyVocabulary.Controls
{
    public partial class WordItemControl : UserControl
    {
        public WordItemControl(Word word)
        {
            InitializeComponent();
            TextBlockMain.Text = word.WordRaw;
        }
    }
}
