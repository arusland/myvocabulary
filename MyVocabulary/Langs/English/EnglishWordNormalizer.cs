using MyVocabulary.Controls;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;

namespace MyVocabulary.Langs.English
{
    internal class EnglishWordNormalizer : IWordNormalizer
    {
        private readonly IWordChecker _WordChecker;

        public EnglishWordNormalizer(IWordChecker wordChecker)
        {
            Checker.NotNull(wordChecker, "wordChecker");

            _WordChecker = wordChecker;
        }

        public Language Lang
        {
            get { return Language.English; }
        }

        public IEnumerable<WordChange> GetChanges(Word word)
        {
            List<WordChange> result = new List<WordChange>();

            if (word.WordRaw.EndsWith("d"))
            {
                result.AddRange(GenerateChanges(word, "d"));
                result.AddRange(GenerateChanges(word, "ed"));
                result.AddRange(GenerateChangesDoubleLetter(word, "ed"));
                result.AddRange(GenerateChanges(word, "ied", "y"));
            }
            else if (word.WordRaw.EndsWith("s"))
            {
                result.AddRange(GenerateChanges(word, "s"));
                result.AddRange(GenerateChanges(word, "es"));
                result.AddRange(GenerateChanges(word, "ies", "y"));
                result.AddRange(GenerateChanges(word, "ness"));
                result.AddRange(GenerateChanges(word, "less"));
            }
            else if (word.WordRaw.EndsWith("ly"))
            {
                result.AddRange(GenerateChanges(word, "ly"));
                result.AddRange(GenerateChanges(word, "ly", "e"));
            }
            else if (word.WordRaw.EndsWith("ing"))
            {
                result.AddRange(GenerateChanges(word, "ing"));
                result.AddRange(GenerateChanges(word, "ing", "e"));
                result.AddRange(GenerateChangesDoubleLetter(word, "ing"));
            }
            else if (word.WordRaw.EndsWith("er"))
            {
                result.AddRange(GenerateChanges(word, "er"));
                result.AddRange(GenerateChanges(word, "r"));
            }
            else if (word.WordRaw.EndsWith("st"))
            {
                result.AddRange(GenerateChanges(word, "st"));
                result.AddRange(GenerateChanges(word, "est"));
            }
            else if (word.WordRaw.EndsWith("ion"))
            {
                result.AddRange(GenerateChanges(word, "ion"));
                result.AddRange(GenerateChanges(word, "ion", "e"));
            }

            return result;
        }

        /// <summary>
        /// Change 'digged' -> 'dig'
        /// </summary>
        private IEnumerable<WordChange> GenerateChangesDoubleLetter(Word word, string ending)
        {
            if (word.WordRaw.EndsWith(ending))
            {
                if ((ending.Length + 2) <= word.WordRaw.Length)
                {
                    var index = word.WordRaw.Length - ending.Length - 1;

                    if (word.WordRaw[index] == word.WordRaw[index - 1])
                    {
                        var newEnding = word.WordRaw[index] + ending;
                        var newWord = word.WordRaw.Remove(word.WordRaw.Length - newEnding.Length);

                        if (!_WordChecker.Exists(newWord))
                        {
                            yield return new WordChange(word, ChangeType.RemoveEnd, newWord, ending);

                            yield return new WordChange(word, ChangeType.AddNew, newWord);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Changes 'easiest' -> 'easy'
        /// </summary>
        private IEnumerable<WordChange> GenerateChanges(Word word, string ending, string ending2)
        {
            if (word.WordRaw.EndsWith(ending))
            {
                var newWord = word.WordRaw.Remove(word.WordRaw.Length - ending.Length) + ending2;

                if (!_WordChecker.Exists(newWord))
                {
                    yield return new WordChange(word, ChangeType.RemoveEnd, newWord, ending, ending2);

                    yield return new WordChange(word, ChangeType.AddNew, newWord);
                }
            }
        }

        /// <summary>
        /// Changes 'maker' -> 'make'
        /// </summary>
        /// <param name="word"></param>
        /// <param name="ending"></param>
        /// <returns></returns>
        private IEnumerable<WordChange> GenerateChanges(Word word, String ending)
        {
            if (word.WordRaw.EndsWith(ending))
            {
                var newWord = word.WordRaw.Remove(word.WordRaw.Length - ending.Length);

                if (!_WordChecker.Exists(newWord))
                {
                    yield return new WordChange(word, ChangeType.RemoveEnd, newWord, ending);

                    yield return new WordChange(word, ChangeType.AddNew, newWord);
                }
            }
        }
    }
}
