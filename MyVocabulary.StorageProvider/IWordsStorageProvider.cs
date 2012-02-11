﻿using System.Collections.Generic;
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

        void Delete(IEnumerable<Word> words);

        bool Rename(string oldWord, string newWord);

        void Save();
    }
}
