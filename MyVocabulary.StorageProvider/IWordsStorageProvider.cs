﻿using MyVocabulary.StorageProvider.Enums;
using System.Collections.Generic;

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
        
        bool Exists(string name);

        Word GetByName(string name);

        void Update(IEnumerable<Word> words);

        void SetLabel(IEnumerable<Word> words, WordLabel label);

        WordLabel UpdateLabel(WordLabel label);

        void Delete(IEnumerable<Word> words);

        void DeleteLabels(IEnumerable<WordLabel> labels);

        bool Rename(string oldWord, string newWord);

        void Save();

        Language Lang { get; set; }
    }
}
