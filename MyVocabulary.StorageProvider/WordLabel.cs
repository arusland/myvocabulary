using System;
using Shared.Helpers;

namespace MyVocabulary.StorageProvider
{
    public class WordLabel
    {
        #region Ctors
        
        public WordLabel(int id, string label)
        {
            //Checker.NotNullOrEmpty(label, "label");

            Label = label;
            Id = id;
        }

        public WordLabel(string label)
            :this(-1, label)
        {
        }
        
        #endregion

        #region Properties
        
        #region Public

        public bool IsNew
        {
            get
            {
                return Id < 0;
            }
        }

        public int Id
        {
            get;
            private set;
        }

        public string Label
        {
            get;
            private set;
        }
        
        #endregion
        
        #endregion

        #region Methods
        
        #region Public
        
        public void SetLabel(string label)
        {
            Checker.NotNullOrEmpty(label, "label");

            Label = label;
        }

        public override string ToString()
        {
            return Label;
        }

        public bool EqualsName(WordLabel label)
        {
            return String.Compare(Label, label.Label, true) == 0;
        }
        
        #endregion
        
        #endregion
    }
}
