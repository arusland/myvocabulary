using MyVocabulary.Langs.Cases;
using System;
using System.Collections.Generic;

namespace MyVocabulary.Extensions
{
    public static class ChangeChangeListExtensions
    {
        public static List<ChangeCase> Remove(this List<ChangeCase> list, String ending)
        {
            list.Add(new RemoveCase(ending));

            return list;
        }

        public static List<ChangeCase> RemoveEndings(this List<ChangeCase> list, params String[] endings)
        {
            foreach(String ending in endings)
            {
                list.Add(new RemoveCase(ending));
            }

            return list;
        }

        public static List<ChangeCase> Replace(this List<ChangeCase> list, String ending, String replacementEnding)
        {
            list.Add(new ReplaceCase(ending, replacementEnding));

            return list;
        }

        public static List<ChangeCase> RemoveWithDoubleLetter(this List<ChangeCase> list, String ending)
        {
            list.Add(new DoubleLetterRemoveCase(ending));

            return list;
        }
    }
}
