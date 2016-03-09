using MyVocabulary.Controls;
using MyVocabulary.Langs.English;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyVocabulary.Langs
{
    internal class WordNormalizerFactory : IWordNormalizer
    {
        private readonly IWordChecker _WordChecker;
        private readonly List<IWordNormalizer> _Normalizers = new List<IWordNormalizer>();

        public WordNormalizerFactory(IWordChecker wordChecker)
        {
            Checker.NotNull(wordChecker, "wordChecker");

            _WordChecker = wordChecker;
        }

        public IWordNormalizer CreateNormalizer(Language lang)
        {
            lock (_Normalizers)
            {
                var normalizer = _Normalizers.FirstOrDefault(p => p.Lang == lang);

                if (normalizer != null)
                {
                    return normalizer;
                }

                if (lang != Language.Automatic && lang != Language.None)
                {
                    switch (lang)
                    {
                        case Language.English:
                            normalizer = new EnglishWordNormalizer(_WordChecker);
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported language: " + lang);
                    }

                    _Normalizers.Add(normalizer);

                    return normalizer;
                }

                return this;
            }
        }


        Language IWordNormalizer.Lang
        {
            get { return Language.Automatic; }
        }

        IEnumerable<WordChange> IWordNormalizer.GetChanges(Word word)
        {
            foreach (IWordNormalizer normalizer in GetNormalizers())
            {
                var changes = normalizer.GetChanges(word).ToList();

                if (changes.Any())
                {
                    return changes;
                }
            }

            return new List<WordChange>();
        }


        string IWordNormalizer.GetRenameTooltip(Word word)
        {
            foreach (IWordNormalizer normalizer in GetNormalizers())
            {
                String tooltip = normalizer.GetRenameTooltip(word);

                if (tooltip.IsNotNullOrEmpty())
                {
                    return tooltip;
                }
            }

            return String.Empty;
        }

        private IEnumerable<IWordNormalizer> GetNormalizers()
        {
            yield return CreateNormalizer(Language.English);
            yield return CreateNormalizer(Language.Russian);
        }
    }
}
