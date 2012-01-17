using System.Collections.Generic;
using MyVocabulary.StorageProvider;

namespace MyVocabulary
{
    internal interface IWordListProvider
    {
        IEnumerable<Word> Get();
    }
}
