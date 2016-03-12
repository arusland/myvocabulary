using MyVocabulary.Controls;
using MyVocabulary.Extensions;
using MyVocabulary.Langs.Cases;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyVocabulary.Langs.English
{
    internal class EnglishWordNormalizer : IWordNormalizer
    {
        private readonly IWordChecker _WordChecker;
        private readonly List<ChangeCase> _ChangeCases = new List<ChangeCase>()
            .RemoveEndings("d", "ed", "s", "es", "less", "ness", "ing", "er", "r", "st", "est", "ion", "ly")
            .ReplaceEnding("ied", "y")
            .ReplaceEnding("ies", "y")
            .ReplaceEnding("ly", "e")
            .ReplaceEnding("y", "e")
            .ReplaceEnding("ing", "e")
            .ReplaceEnding("ion", "e")
            .RemoveEndingWithDoubleLetter("ed")
            .RemoveEndingWithDoubleLetter("ing");

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

            foreach (ChangeCase cs in _ChangeCases)
            {
                var dcs = cs as DoubleLetterRemoveEndingCase;

                if (dcs != null)
                {
                    result.AddRange(GenerateChangesDoubleLetter(word, dcs.Ending));
                    continue;
                }

                var rcs = cs as ReplaceEndingCase;

                if (rcs != null)
                {
                    result.AddRange(GenerateChanges(word, rcs.Ending, rcs.ReplacementEnding));
                    continue;
                }

                var rmcs = cs as RemoveEndingCase;

                if (rmcs != null)
                {
                    result.AddRange(GenerateChanges(word, rmcs.Ending));
                    continue;
                }
            }

            return result;
        }

        public string GetRenameTooltip(Word word)
        {
            StringBuilder result = new StringBuilder();

            foreach (ChangeCase cs in _ChangeCases)
            {
                var dcs = cs as DoubleLetterRemoveEndingCase;

                if (dcs != null)
                {
                    MakeDoubleLetterTooltip(result, word, dcs.Ending);
                    continue;
                }

                var rcs = cs as ReplaceEndingCase;

                if (rcs != null)
                {
                    MakeRenameTooltip(result, word, rcs.Ending, rcs.ReplacementEnding);
                    continue;
                }

                var rmcs = cs as RemoveEndingCase;

                if (rmcs != null)
                {
                    MakeRenameTooltip(result, word, rmcs.Ending);
                    continue;
                }
            }

            return result.ToString();
        }

        public bool IsPotentialForRemove(Word word)
        {
            foreach (ChangeCase cs in _ChangeCases)
            {
                var dcs = cs as DoubleLetterRemoveEndingCase;

                if (dcs != null)
                {
                    if (CanBeRemovedDoubleLetter(word, dcs.Ending))
                    {
                        return true;
                    }
                    continue;
                }

                var rcs = cs as ReplaceEndingCase;

                if (rcs != null)
                {
                    if (CanBeRemoved(word, rcs.Ending, rcs.ReplacementEnding))
                    {
                        return true;
                    }
                    continue;
                }

                var rmcs = cs as RemoveEndingCase;

                if (rmcs != null)
                {
                    if (CanBeRemoved(word, rmcs.Ending))
                    {
                        return true;
                    }
                    continue;
                }
            }

            return false;
        }

        private void MakeDoubleLetterTooltip(StringBuilder result, Word word, string ending)
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
                        CheckAndMakeTooltip(newWord, result);
                    }
                }
            }
        }

        private void MakeRenameTooltip(StringBuilder result, Word word, string ending)
        {
            if (word.WordRaw.EndsWith(ending))
            {
                var newWord = word.WordRaw.Remove(word.WordRaw.Length - ending.Length);
                CheckAndMakeTooltip(newWord, result);
            }
        }

        private void MakeRenameTooltip(StringBuilder result, Word word, string ending, string ending2)
        {
            if (word.WordRaw.EndsWith(ending))
            {
                var newWord = word.WordRaw.Remove(word.WordRaw.Length - ending.Length) + ending2;
                CheckAndMakeTooltip(newWord, result);
            }
        }

        private void CheckAndMakeTooltip(String newWord, StringBuilder result)
        {
            Word foundWord = _WordChecker.GetByName(newWord);

            if (foundWord != null)
            {
                AddCorespondingMessage(result, foundWord);
            }
        }

        private static void AddCorespondingMessage(StringBuilder result, Word foundWord)
        {
            if (result.Length > 0)
            {
                result.Append(Environment.NewLine);
            }

            result.AppendFormat("Corresponding word '{0}' already exists ({1})", foundWord.WordRaw, foundWord.Type);
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

        private bool CanBeRemoved(Word word, String ending)
        {
            if (word.WordRaw.EndsWith(ending))
            {
                var newWord = word.WordRaw.Remove(word.WordRaw.Length - ending.Length);

                return _WordChecker.Exists(newWord);
            }

            return false;
        }

        private bool CanBeRemoved(Word word, String ending, String ending2)
        {
            if (word.WordRaw.EndsWith(ending))
            {
                var newWord = word.WordRaw.Remove(word.WordRaw.Length - ending.Length) + ending2;

                return _WordChecker.Exists(newWord);
            }

            return false;
        }

        private bool CanBeRemovedDoubleLetter(Word word, string ending)
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

                        return _WordChecker.Exists(newWord);
                    }
                }
            }

            return false;
        }
    }
}
