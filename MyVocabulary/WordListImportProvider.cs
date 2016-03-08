using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using MyVocabulary.StorageProvider.Helpers;
using Shared.Extensions;
using Shared.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace MyVocabulary
{
    internal class WordListImportProvider : IWordListProvider, IWordsStorageImportProvider
    {
        #region Fields

        private readonly List<Word> _Words;
        private readonly IWordListProvider _Provider;

        #endregion

        #region Ctors

        public WordListImportProvider(string[] words, IList<WordLabel> labels, IWordListProvider provider)
        {
            Checker.NotNull(words, "words");
            Checker.NotNull(provider, "provider");

            _Provider = provider;
            _Words = words.OrderBy(p => p).Select(p => new Word(p, WordType.None, labels)).ToList();
        }

        #endregion

        #region IWordListProvider

        public Language Lang
        {
            get
            {
                return _Provider.Lang;
            }
        }

        public IEnumerable<Word> Get()
        {
            return _Words.OrderBy(p => p.WordRaw);
        }

        public IEnumerable<WordLabel> GetLabels()
        {
            return _Provider.GetLabels();
        }

        #endregion

        #region IWordListImportProvider

        public void Delete(IEnumerable<Word> words)
        {
            foreach (var word in words)
            {
                int index = _Words.FindIndex(p => p.WordRaw == word.WordRaw);

                if (index >= 0)
                {
                    _Words.RemoveAt(index);
                }
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

            _Words[indexOld] = new Word(newWord, _Words[indexOld].Type, _Words[indexOld].Labels);

            return true;
        }

        public void Update(IEnumerable<Word> words)
        {
            var editedWords = new List<Word>();

            foreach (var word in words)
            {
                var existingWord = _Words.FirstOrDefault(p => p.WordRaw == word.WordRaw);

                if (existingWord.IsNotNull())
                {
                    editedWords.Add(new Word(word.WordRaw, word.Type, LabelHelper.JoinLabels(existingWord.Labels, word.Labels, _Provider.GetLabels())));
                }
                else
                {
                    editedWords.Add(new Word(word.WordRaw, word.Type, LabelHelper.JoinLabels(new WordLabel[0], word.Labels, _Provider.GetLabels())));
                }
            }

            Delete(words);

            _Words.AddRange(editedWords);
        }

        public void SetLabel(IEnumerable<Word> words, WordLabel label)
        {
            var editedWords = new List<Word>();

            foreach (var word in _Words.Where(p => words.Any(g => g.WordRaw == p.WordRaw)))
            {
                editedWords.Add(new Word(word.WordRaw, word.Type, LabelHelper.JoinLabels(word.Labels, new List<WordLabel> { label }, _Provider.GetLabels())));
            }

            Delete(words);

            _Words.AddRange(editedWords);
        }

        #endregion
    }
}
