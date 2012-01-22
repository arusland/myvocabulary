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
            var mc = Regex.Matches(rawtext, @"[a-zA-Z]+(\-[a-zA-Z]+)*");
            var rgx = new Regex(@"[a-zA-Z]");

            return mc.Cast<Match>().Select(p => p.Value.ToLower()).Where(p => p.Length > 1 && rgx.IsMatch(p)).Distinct();
        }
        
        #endregion
        
        #endregion
    }
}
