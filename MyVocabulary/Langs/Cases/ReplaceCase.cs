using System;

namespace MyVocabulary.Langs.Cases
{
    /*
     * Replace ending. 'placing' => 'place'
     */
    public class ReplaceCase : RemoveCase
    {
        public ReplaceCase(String ending, String replacementEnding)
            : base(ending)
        {
            ReplacementEnding = replacementEnding;
        }

        public String ReplacementEnding
        {
            get;
            private set;
        }
    }
}
