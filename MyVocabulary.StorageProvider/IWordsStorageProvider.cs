using System.Collections.Generic;
using MyVocabulary.StorageProvider.Enums;

namespace MyVocabulary.StorageProvider
{
    public interface IWordsStorageProvider
    {
        bool IsModified
        {
            get;
        }

        IEnumerable<Word> Get();

        IEnumerable<WordLabel> GetLabels();

        IEnumerable<Word> Get(WordType type);

        IEnumerable<Word> Find(string text);

        void Update(IEnumerable<Word> words);

        void SetLabel(IEnumerable<Word> words, WordLabel label);

        WordLabel UpdateLabel(WordLabel label);

        void Delete(IEnumerable<Word> words);

        void DeleteLabels(IEnumerable<WordLabel> labels);

        bool Rename(string oldWord, string newWord);

        void Save();
    }
}
