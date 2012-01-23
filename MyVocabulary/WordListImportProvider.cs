using System.Collections.Generic;
using System.Linq;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Helpers;

namespace MyVocabulary
{
    internal class WordListImportProvider : IWordListProvider, IWordsStorageImportProvider
    {
        #region Fields

        private readonly List<Word> _Words;
        
        #endregion

        #region Ctors
        
        public WordListImportProvider(string[] words)
        {
            Checker.NotNull(words, "words");

            _Words = words.OrderBy(p => p).Select(p => new Word(p, WordType.None)).ToList();
        }
        
        #endregion

        #region IWordListProvider

        public IEnumerable<Word> Get()
        {
            return _Words;
        }

        #endregion

        #region IWordListImportProvider

        public void Delete(IEnumerable<Word> words)
        {
            foreach (var word in words)
            {
                int index = _Words.FindIndex(p => p.WordRaw == word.WordRaw);

                _Words.RemoveAt(index);
            }
        }

        public bool Rename(string oldWord, string newWord)
        {
            if (_Words.FindIndex(p => p.WordRaw == newWord) >= 0)
            {
                return false;
            }

            int indexOld = _Words.FindIndex(p => p.WordRaw == oldWord);

            if (indexOld < 0)
            {
                return false;
            }

            _Words[indexOld] = new Word(newWord, _Words[indexOld].Type);

            return true;
        }

        #endregion
    }
}
