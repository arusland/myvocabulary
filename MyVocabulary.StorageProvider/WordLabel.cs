using Shared.Helpers;

namespace MyVocabulary.StorageProvider
{
    public class WordLabel
    {
        #region Ctors
        
        public WordLabel(int id, string label)
        {
            Checker.NotNullOrEmpty(label, "label");

            Label = label;
        }

        public WordLabel(string label)
            :this(-1, label)
        {
        }
        
        #endregion

        #region Properties
        
        #region Public

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
        
        #endregion
        
        #endregion
    }
}
