using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyVocabulary.StorageProvider;
using Shared.Helpers;

namespace MyVocabulary
{
    internal class WordsOperationEventsArgs : EventArgs
    {
        #region Ctors

        public WordsOperationEventsArgs(IList<Word> words, Operation operation)
            :this(words, operation, null)
        {
        }
        
        public WordsOperationEventsArgs(IList<Word> words, Operation operation, object arg)
        {
            Checker.NotNull(words, "words");
            Checker.AreNotEqual(Operation.None, operation);

            Operation = operation;
            Words = new ReadOnlyCollection<Word>(words);
            Arg = arg;
        }
        
        #endregion

        #region Properties
        
        #region Public

        public ReadOnlyCollection<Word> Words
        {
            get;
            private set;
        }
        
        public Operation Operation
        {
            get;
            private set;
        }

        public object Arg
        {
            get;
            private set;
        }

        #endregion
        
        #endregion
    }
}
