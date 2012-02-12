using System;
using MyVocabulary.StorageProvider;
using Shared.Helpers;

namespace MyVocabulary.Controls
{
    internal class OnLabelEditEventArgs : EventArgs
    {
        #region Ctors

        public OnLabelEditEventArgs(WordLabel label)
        {
            Checker.NotNull(label, "label");

            Label = label;
        }
        
        #endregion

        #region Properties
        
        #region Public
        
        public WordLabel Result
        {
            get;
            set;
        }

        public WordLabel Label
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion
    }
}


