using System;

namespace MyVocabulary.Langs.Cases
{
    /*
     * Changes 'digging' => 'dig'
     */
    public class DoubleLetterRemoveEndingCase : RemoveEndingCase
    {
        public DoubleLetterRemoveEndingCase(String ending)
            : base(ending)
        {
        }
    }
}
