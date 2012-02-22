using System.Text.RegularExpressions;
using System.Linq;
using Shared.Extensions;
using RS = MyVocabulary.Properties.Resources;
using MyVocabulary.StorageProvider;

namespace MyVocabulary.Helpers
{
    internal class FilterHelper
    {
        #region Constants

        private const string PREFIX_Label = "label:";
        
        #endregion        

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

        private bool UseLabel
        {
            get;
            set;
        }

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

        public bool Check(Word word)
        {
            if (UseLabel)
            {
                if (word.Labels.Count > 0)
                {
                    if (UseWildCard)
                    {
                        return word.Labels.Any(p => WildCardPattern.IsMatch(p.Label.ToLower()));
                    }
                    else
                        return word.Labels.Any(p => p.Label.ToLower().IndexOf(FilterText) >= 0);
                }
                else
                    return false;
            }
            else
            {
                return Check(word.WordRaw);
            }            
        }
        
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

            UseLabel = FilterText.StartsWith(PREFIX_Label);

            if (UseLabel)
            {
                FilterText = FilterText.Substring(PREFIX_Label.Length);
            }

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
