using System;

namespace MyVocabulary.Langs.Cases
{
    /*
     * Remove ending. 'placed' => 'place'    
     */
    public class RemoveEndingCase : ChangeCase
    {
        public RemoveEndingCase(String ending)
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
