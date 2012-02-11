using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Helpers;

namespace MyVocabulary.Controls
{
    internal class OnWordAddEventArgs : EventArgs
    {
        #region Ctors

        public OnWordAddEventArgs(string newWord, WordType type, IList<WordLabel> labels)
        {
            Checker.NotNullOrEmpty(newWord, "newWord");

            Cancel = false;
            NewWord = newWord;
            Type = type;
            Labels = new ReadOnlyCollection<WordLabel>(labels);
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

        public ReadOnlyCollection<WordLabel> Labels
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion
    }
}


