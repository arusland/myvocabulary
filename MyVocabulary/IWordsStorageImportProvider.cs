using System.Collections.Generic;
using MyVocabulary.StorageProvider;

namespace MyVocabulary
{
    interface IWordsStorageImportProvider
    {
        void Delete(IEnumerable<Word> words);

        bool Rename(string oldWord, string newWord);
    }
}
