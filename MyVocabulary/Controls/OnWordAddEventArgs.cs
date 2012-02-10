using System;
using MyVocabulary.StorageProvider.Enums;
using Shared.Helpers;

namespace MyVocabulary.Controls
{
    internal class OnWordAddEventArgs : EventArgs
    {
        #region Ctors

        public OnWordAddEventArgs(string newWord, WordType type)
        {
            Checker.NotNullOrEmpty(newWord, "newWord");

            Cancel = false;
            NewWord = newWord;
            Type = type;
        }
        
        #endregion

        #region Properties
        
        #region Public
        
        public bool Cancel
        {
            get;
            set;
        }

        public string NewWord
        {
            get;
            private set;
        }

        public WordType Type
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion
    }
}


