using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using System;
using System.Collections.Generic;

namespace MyVocabulary.Langs
{
    internal interface IWordNormalizer
    {
        Language Lang
        {
            get;
        }

        IEnumerable<WordChange> GetChanges(Word word);

        String GetRenameTooltip(Word word);
    }
}
