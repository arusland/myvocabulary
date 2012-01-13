using MyVocabulary.StorageProvider.Enums;

namespace MyVocabulary.StorageProvider
{
    public class Word
    {
        #region Ctors
        
        public Word(string word, WordType type)
        {
            WordRaw = word;
            Type = type;
        }
        
        #endregion

        #region Properties
        
        #region Public

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
    }
}
