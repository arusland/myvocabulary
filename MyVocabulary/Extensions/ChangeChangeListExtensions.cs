using MyVocabulary.Langs.Cases;
using System;
using System.Collections.Generic;

namespace MyVocabulary.Extensions
{
    public static class ChangeChangeListExtensions
    {
        public static List<ChangeCase> Remove(this List<ChangeCase> list, String ending)
        {
            list.Add(new RemoveEndingCase(ending));

            return list;
        }

        public static List<ChangeCase> RemoveEndings(this List<ChangeCase> list, params String[] endings)
        {
            foreach(String ending in endings)
            {
                list.Add(new RemoveEndingCase(ending));
            }

            return list;
        }

        public static List<ChangeCase> ReplaceEnding(this List<ChangeCase> list, String ending, String replacementEnding)
        {
            list.Add(new ReplaceEndingCase(ending, replacementEnding));

            return list;
        }

        public static List<ChangeCase> RemoveEndingWithDoubleLetter(this List<ChangeCase> list, String ending)
        {
            list.Add(new DoubleLetterRemoveEndingCase(ending));

            return list;
        }
    }
}
