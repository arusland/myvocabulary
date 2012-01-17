using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyVocabulary.StorageProvider;

namespace MyVocabulary
{
    internal interface IWordListProvider
    {
        IEnumerable<Word> Get();
    }
}
