using MyVocabulary.Controls;
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

        public string GetRenameTooltip(Word word)
        {
            StringBuilder result = new StringBuilder();

            if (word.WordRaw.EndsWith("d"))
            {
                MakeRenameTooltip(result, word, "d");
                MakeRenameTooltip(result, word, "ed");
                MakeDoubleLetterTooltip(result, word, "ed");
                MakeRenameTooltip(result, word, "ied", "y");
            }
            else if (word.WordRaw.EndsWith("s"))
            {
                MakeRenameTooltip(result, word, "s");
                MakeRenameTooltip(result, word, "es");
                MakeRenameTooltip(result, word, "ies", "y");
                MakeRenameTooltip(result, word, "ness");
                MakeRenameTooltip(result, word, "less");
            }
            else if (word.WordRaw.EndsWith("ly"))
            {
                MakeRenameTooltip(result, word, "ly");
                MakeRenameTooltip(result, word, "ly", "e");
            }
            else if (word.WordRaw.EndsWith("ing"))
            {
                MakeRenameTooltip(result, word, "ing");
                MakeRenameTooltip(result, word, "ing", "e");
                MakeDoubleLetterTooltip(result, word, "ing");
            }
            else if (word.WordRaw.EndsWith("er"))
            {
                MakeRenameTooltip(result, word, "er");
                MakeRenameTooltip(result, word, "r");
            }
            else if (word.WordRaw.EndsWith("st"))
            {
                MakeRenameTooltip(result, word, "st");
                MakeRenameTooltip(result, word, "est");
            }
            else if (word.WordRaw.EndsWith("ion"))
            {
                MakeRenameTooltip(result, word, "ion");
                MakeRenameTooltip(result, word, "ion", "e");
            }

            return result.ToString();
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

        public bool IsPotentialForRemove(Word word)
        {
            bool canBeRemoved = false;

            if (word.WordRaw.EndsWith("d"))
            {
                canBeRemoved = CanBeRemoved(word, "d") ||
                    CanBeRemoved(word, "ed") ||
                    CanBeRemovedDoubleLetter(word, "ed") ||
                    CanBeRemoved(word, "ied", "y");
            }
            else if (word.WordRaw.EndsWith("s"))
            {
                canBeRemoved = CanBeRemoved(word, "s") ||
                    CanBeRemoved(word, "es") ||
                    CanBeRemoved(word, "ies", "y") ||
                    CanBeRemoved(word, "ness") ||
                    CanBeRemoved(word, "less");
            }
            else if (word.WordRaw.EndsWith("ly"))
            {
                canBeRemoved = CanBeRemoved(word, "ly") ||
                    CanBeRemoved(word, "ly", "e");
            }
            else if (word.WordRaw.EndsWith("ing"))
            {
                canBeRemoved = CanBeRemoved(word, "ing") ||
                    CanBeRemoved(word, "ing", "e") ||
                    CanBeRemovedDoubleLetter(word, "ing");
            }
            else if (word.WordRaw.EndsWith("er"))
            {
                canBeRemoved = CanBeRemoved(word, "er") ||
                    CanBeRemoved(word, "r");
            }
            else if (word.WordRaw.EndsWith("st"))
            {
                canBeRemoved = CanBeRemoved(word, "st") ||
                    CanBeRemoved(word, "est");
            }
            else if (word.WordRaw.EndsWith("ion"))
            {
                canBeRemoved = CanBeRemoved(word, "ion") ||
                    CanBeRemoved(word, "ion", "e");
            }

            return canBeRemoved;
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
