using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyVocabulary.Helpers
{
    internal class WordsFinder
    {
        #region Methods
        
        #region Public
        
        public IEnumerable<string> Parse(string rawtext)
        {
            var mc = Regex.Matches(rawtext, @"[\w\'\-]+");

            return mc.Cast<Match>().Select(p => p.Value.ToLower()).Distinct();
        }
        
        #endregion
        
        #endregion
    }
}
