using System.Linq;
using MyVocabulary.Controls;
using Shared.Helpers;

namespace MyVocabulary
{
    internal class ImportWordChecker : IWordChecker
    {
        #region Fields
        
        private readonly IWordChecker _BaseChecker;
        private readonly IWordListProvider _Provider;
        
        #endregion

        #region Ctors
        
        public ImportWordChecker(IWordChecker baseChecker, IWordListProvider provider)
        {
            Checker.NotNull(baseChecker, "baseChecker");
            Checker.NotNull(provider, "provider");

            _BaseChecker = baseChecker;
            _Provider = provider;
        }
        
        #endregion

        #region IWordChecker

        public bool Exists(string word)
        {
            if (_Provider.Get().Any(p => p.WordRaw == word))
            {
                return true;
            }

            return _BaseChecker.Exists(word);
        }

        #endregion
    }
}
