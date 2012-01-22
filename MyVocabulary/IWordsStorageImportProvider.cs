using System.Collections.Generic;
using MyVocabulary.StorageProvider;

namespace MyVocabulary
{
    interface IWordsStorageImportProvider
    {
        void Delete(IEnumerable<Word> words);
    }
}
