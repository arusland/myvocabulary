using System.Collections.Generic;
using MyVocabulary.StorageProvider;

namespace MyVocabulary
{
    interface IWordsStorageImportProvider
    {
        void Delete(IEnumerable<Word> words);

        bool Rename(string oldWord, string newWord);

        void Update(IEnumerable<Word> words);

        void SetLabel(IEnumerable<Word> words, WordLabel label);
    }
}
