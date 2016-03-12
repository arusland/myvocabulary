using System;

namespace MyVocabulary.Langs.Cases
{
    /*
     * Changes 'digging' => 'dig'
     */
    public class DoubleLetterRemoveCase : RemoveCase
    {
        public DoubleLetterRemoveCase(String ending)
            : base(ending)
        {
        }
    }
}
