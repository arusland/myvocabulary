using MyVocabulary.StorageProvider;

namespace MyVocabulary.Controls
{
    internal interface IWordChecker
    {
        bool Exists(string word);

        Word GetByName(string newWord);
    }
}
