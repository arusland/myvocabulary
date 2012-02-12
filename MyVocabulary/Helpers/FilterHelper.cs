using System.Text.RegularExpressions;
using Shared.Extensions;
using RS = MyVocabulary.Properties.Resources;

namespace MyVocabulary.Helpers
{
    internal class FilterHelper
    {
        #region Ctors
        
        public FilterHelper(string text)
        {
            ProcessFilterText(text.Trim());
        }
        
        #endregion

        #region Properties
        
        #region Public
        
        public bool ShowAll
        {
            get;
            private set;
        }

        public static string WaterMarkText
        {
            get
            {
                return RS.FILTER_Text;
            }
        }

        public string FilterText
        {
            get;
            private set;
        }

        #endregion

        #region Private

        private bool UseWildCard
        {
            get;
            set;
        }

        private Regex WildCardPattern
        {
            get;
            set;
        }
        
        #endregion
        
        #endregion

        #region Methods
        
        #region Public
        
        public bool Check(string word)
        {
            if (UseWildCard)
            {
                return WildCardPattern.IsMatch(word);
            }
            else
                return word.IndexOf(FilterText) >= 0;
        }
        
        #endregion

        #region Private
        
        private void ProcessFilterText(string text)
        {
            FilterText = text.ToLower().Trim();

            if (Regex.IsMatch(FilterText, @"[\*\?]"))
            {
                var pat = Regex.Replace(FilterText, @"\*+", @"[\w \-]+");
                pat = Regex.Replace(pat, @"\?", @"[\w \-]");
                WildCardPattern = new Regex("^" + pat + "$");
                UseWildCard = true;
            }
            else
            {
                UseWildCard = false;
            }

            ShowAll = FilterText.Equals(WaterMarkText.ToLower()) || FilterText.IsEmpty();
        }
        
        #endregion
        
        #endregion
    }
}
