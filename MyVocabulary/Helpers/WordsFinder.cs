using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyVocabulary.Helpers
{
    internal class WordsFinder
    {
        #region Methods
        
        #region Public
        
        public IEnumerable<string> Parse(string rawtext, ParseOptions options)
        {
            var rgxDigit = new Regex(@"^[\d\-]+$");

            if (options.ByLine)
            {
                return Regex.Split(rawtext, @"[\r\n]+").Select(p => p.Trim().ToLower())
                    .Where(p => p.Length > 1 && !rgxDigit.IsMatch(p))
                    .Distinct();
            }
            else
            {
                var rgxEnc = new Regex(options.OnlyLatin ? @"[a-zA-Z]+(\-[a-zA-Z]+)*" : @"[\w]+(\-[\w]+)*");
                var mc = rgxEnc.Matches(rawtext);

                return mc.Cast<Match>().Select(p => p.Value.ToLower())
                    .Where(p => p.Length > 1 && !rgxDigit.IsMatch(p))
                    .Distinct();
            }
        }
        
        #endregion
        
        #endregion
    }
}
