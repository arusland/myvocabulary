using System;
using System.Collections.Generic;
using MyVocabulary.StorageProvider;
using Shared.Helpers;

namespace MyVocabulary
{
    internal class WordsOperationEventsArgs : EventArgs
    {
        #region Ctors
        
        public WordsOperationEventsArgs(IList<Word> words, Operation operation)
        {
            Checker.NotNull(words, "words");
            Checker.AreNotEqual(Operation.None, operation);

            Operation = operation;
            Words = words;
        }
        
        #endregion

        #region Properties
        
        #region Public

        public IList<Word> Words
        {
            get;
            private set;
        }
        
        public Operation Operation
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion
    }
}
