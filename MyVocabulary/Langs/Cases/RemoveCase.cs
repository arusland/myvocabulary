using System;

namespace MyVocabulary.Langs.Cases
{
    /*
     * Remove ending. 'placed' => 'place'    
     */
    public class RemoveCase : ChangeCase
    {
        public RemoveCase(String ending)
        {
            Ending = ending;
        }

        public String Ending
        {
            get;
            private set;
        }
    }
}
