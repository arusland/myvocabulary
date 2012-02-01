
namespace MyVocabulary.Helpers
{
    internal sealed class ParseOptions
    {
        #region Ctors
        
        public ParseOptions(bool onlyLatin, bool byLine)
        {
            OnlyLatin = onlyLatin;
            ByLine = byLine;
        }
        
        #endregion

        #region Properties
        
        #region Public
        
        public bool OnlyLatin
        {
            get;
            private set;
        }

        public bool ByLine
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion
    }
}
