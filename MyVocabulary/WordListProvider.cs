using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyVocabulary.StorageProvider.Enums;
using MyVocabulary.StorageProvider;
using Shared.Helpers;

namespace MyVocabulary
{
    internal class WordListProvider : IWordListProvider
    {
        #region Fields

        private readonly IWordsStorageProvider _Provider;
        private WordType _WordType;
        
        #endregion

        #region Ctors
        
        public WordListProvider(IWordsStorageProvider provider, WordType type)
        {
            Checker.NotNull(provider, "provider");
            Checker.AreNotEqual(WordType.None, type);

            _Provider = provider;
            _WordType = type;
        }
        
        #endregion

        #region IWordListProvider

        public IEnumerable<Word> Get()
        {
            return _Provider.Get(_WordType);
        }

        public IEnumerable<WordLabel> GetLabels()
        {
            return _Provider.GetLabels();
        }

        public Language Lang
        {
            get
            {
                return _Provider.Lang;
            }
        }

        #endregion
    }
}
