using Shared.Helpers;
using MyVocabulary.StorageProvider.Enums;

namespace MyVocabulary.StorageProvider
{
    public class Word
    {
        #region Ctors
        
        public Word(string word, WordType type)
        {
            Checker.NotNullOrEmpty(word, "word");
            Checker.AreNotEqual(WordType.None, type);

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
