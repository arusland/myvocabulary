using MyVocabulary.StorageProvider;
using System;

namespace MyVocabulary.Langs
{
    public class WordChange
    {
        public WordChange(Word word, ChangeType type, String newWord)
            :this(word, type, newWord, String.Empty)
        {
        }

        public WordChange(Word word, ChangeType type, String newWord, String param)
            :this(word, type, newWord, param, String.Empty)
        {
        }

        public WordChange(Word word, ChangeType type, String newWord, String param, String param2)
        {
            Word = word;
            Type = type;
            NewWord = newWord;
            Param = param;
            Param2 = param2;
        }

        public Word Word
        {
            get;
            private set;
        }

        public String NewWord
        {
            get;
            private set;
        }

        public String Param
        {
            get;
            private set;
        }

        public String Param2
        {
            get;
            private set;
        }        

        public ChangeType Type
        {
            get;
            private set;
        }
    }
}
