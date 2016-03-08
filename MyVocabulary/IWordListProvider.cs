using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using System.Collections.Generic;

namespace MyVocabulary
{
    internal interface IWordListProvider
    {
        IEnumerable<Word> Get();

        IEnumerable<WordLabel> GetLabels();

        Language Lang
        {
            get;
        }
    }
}
