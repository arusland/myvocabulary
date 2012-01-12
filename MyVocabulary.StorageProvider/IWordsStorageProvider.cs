using System.Collections.Generic;
using MyVocabulary.StorageProvider.Enums;

namespace MyVocabulary.StorageProvider
{
    public interface IWordsStorageProvider
    {
        IEnumerable<Word> Get();

        IEnumerable<Word> Get(WordType type);

        IEnumerable<Word> Find(string text);

        void Update(IEnumerable<Word> words);

        void Delete(IEnumerable<Word> words);
    }
}
