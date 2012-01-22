using System;
using Shared.Helpers;

namespace MyVocabulary.Controls
{
    internal class OnWordRenameEventArgs : EventArgs
    {
        #region Ctors
        
        public OnWordRenameEventArgs(string newWord, string oldWord)
        {
            Checker.NotNullOrEmpty(newWord, "newWord");
            Checker.NotNullOrEmpty(oldWord, "oldWord");           

            Cancel = false;
            NewWord = newWord;
            OldWord = oldWord;
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

        public string OldWord
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion
    }
}


