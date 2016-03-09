using System;
using Shared.Helpers;

namespace MyVocabulary.StorageProvider
{
    public class WordLabel
    {
        // Word is labeled with this label if all related words already exist
        public static readonly WordLabel LabelToRemove = new WordLabel(-2, "Ready for delete");

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

        public override bool Equals(object obj)
        {
            WordLabel rhs = obj as WordLabel;

            if (rhs == null)
            {
                return false;
            }

            return this.Id == rhs.Id && Label.Equals(rhs.Label);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode() ^ Label.GetHashCode();
        }
        
        #endregion
        
        #endregion
    }
}
