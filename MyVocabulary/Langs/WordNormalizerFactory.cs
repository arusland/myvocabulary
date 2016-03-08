using MyVocabulary.Controls;
using MyVocabulary.Langs.English;
using MyVocabulary.StorageProvider;
using MyVocabulary.StorageProvider.Enums;
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
            var changes = CreateNormalizer(Language.English).GetChanges(word).ToList();

            if (!changes.Any())
            {
                changes = CreateNormalizer(Language.Russian).GetChanges(word).ToList();
            }

            return changes;
        }
    }
}
