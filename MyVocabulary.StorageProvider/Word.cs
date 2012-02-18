using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MyVocabulary.StorageProvider.Enums;
using Shared.Helpers;

namespace MyVocabulary.StorageProvider
{
    public class Word
    {
        #region Ctors
        
        public Word(string word, WordType type, IList<WordLabel> labels)
        {
            Checker.NotNullOrEmpty(word, "word");
            Checker.NotNull(labels, "labels");

            WordRaw = word;
            Type = type;
            Labels = new ReadOnlyCollection<WordLabel>(labels.OrderBy(p => p.Label).ToList());
        }
        
        #endregion

        #region Properties
        
        #region Public

        public ReadOnlyCollection<WordLabel> Labels
        {
            get;
            private set;
        }

        public string WordRaw
        {
            get;
            private set;
        }

        public WordType Type
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion

        #region Methods
        
        #region Public

        public override string ToString()
        {
            return WordRaw;
        }
        
        #endregion
        
        #endregion
    }
}
