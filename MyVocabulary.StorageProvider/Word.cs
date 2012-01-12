using MyVocabulary.StorageProvider.Enums;

namespace MyVocabulary.StorageProvider
{
    public class Word
    {
        #region Ctors
        
        public Word()
        {
        }
        
        #endregion

        #region Properties
        
        #region Public

        public string Word
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
