using MyVocabulary.StorageProvider.Enums;
using MyVocabulary.StorageProvider.Helpers;
using Shared.Extensions;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace MyVocabulary.StorageProvider
{
    public class XmlWordsStorageProvider : IWordsStorageProvider, IComparer<Word>
    {
        private const String ATTR_LANG = "lang";

        #region Fields

        private readonly List<Word> _AllWords;
        private readonly List<WordLabel> _AllLabels;
        private readonly Version _CurrentVersion;
        private string _Filename;

        #endregion

        #region Ctors

        public XmlWordsStorageProvider()
        {
            _AllWords = new List<Word>();
            _AllLabels = new List<WordLabel>();
            _CurrentVersion = new Version(1, 1);
        }

        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Loads items from xml-file.
        /// </summary>
        /// <param name="filename">Name of file.</param>
        public void Load(string filename)
        {
            Checker.NotNullOrEmpty(filename, "filename");

            var doc = new XmlDocument();

            _Filename = filename;

            doc.Load(filename);

            _AllWords.Clear();

            var version = new Version(doc.DocumentElement.GetAttribute("version"));

            if (version > _CurrentVersion)
            {
                throw new InvalidOperationException(string.Format("New file version - {0}. You should update the program.", version.ToString()));
            }

            if (version == new Version(1, 0))
            {
                LoadV10(doc);
            }
            else if (version == _CurrentVersion)
            {
                LoadV11(doc);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Invalid file version - {0}.", version.ToString()));
            }
        }        

        public void SetPath(string filename)
        {
            Checker.NotNullOrEmpty(filename, "filename");

            _Filename = filename;
            IsModified = true;
        }

        #endregion

        #region Private

        private void LoadV10(XmlDocument doc)
        {
            _AllWords.AddRange(doc.DocumentElement.SelectNodes("Words/Item").OfType<XmlNode>().Select(p => LoadFromXmlV10(p)));
            IsModified = false;
        }

        private void LoadV11(XmlDocument doc)
        {
            _AllLabels.AddRange(doc.DocumentElement.SelectNodes("Labels/Item").OfType<XmlNode>().Select(p => LoadLabel(p)));
            _AllWords.AddRange(doc.DocumentElement.SelectNodes("Words/Item").OfType<XmlNode>().Select(p => LoadFromXmlV11(p)));
            Lang = doc.DocumentElement.HasAttribute(ATTR_LANG) ?
                (Language)Enum.Parse(typeof(Language), doc.DocumentElement.Attributes[ATTR_LANG].Value) : Language.English;
            IsModified = false;
        }

        private void SortWords()
        {
            _AllWords.Sort((IComparer<Word>)this);
        }

        private Word LoadFromXmlV10(XmlNode node)
        {
            return new Word(node.GetAttribute("word").ToLower(), (WordType)Convert.ToInt32(node.GetAttribute("type")), new List<WordLabel>());
        }

        private Word LoadFromXmlV11(XmlNode node)
        {
            var labels = LoadWordLabels(node.GetAttributeSafe("labels"));
            return new Word(node.GetAttribute("word").ToLower(), (WordType)Convert.ToInt32(node.GetAttribute("type")), labels);
        }

        private IList<WordLabel> LoadWordLabels(string attr)
        {
            var splitted = attr.Split(',').Where(p => p.IsNotEmpty()).Select(p => Convert.ToInt32(p)).ToList();

            return _AllLabels.Where(p => splitted.Contains(p.Id)).ToList();
        }

        private string LabelsToAttribute(IList<WordLabel> labels)
        {
            var result = new StringBuilder();

            foreach (var label in labels.Where(p => _AllLabels.Any(l => l.Id == p.Id)))
            {
                if (result.Length > 0)
                    result.AppendFormat(",{0}", label.Id.ToString());
                else
                    result.Append(label.Id.ToString());
            }

            return result.ToString();
        }

        private WordLabel LoadLabel(XmlNode node)
        {
            return new WordLabel(Convert.ToInt32(node.GetAttribute("id")), node.GetAttribute("name"));
        }

        #endregion

        #endregion

        #region IWordsStorageProvider

        public bool IsModified
        {
            get;
            private set;
        }

        public Language Lang
        {
            get;
            set;
        }

        public IEnumerable<Word> Get()
        {
            return _AllWords.OrderBy(p => p.WordRaw);
        }

        public IEnumerable<WordLabel> GetLabels()
        {
            return _AllLabels.OrderBy(p => p.Label);
        }

        public IEnumerable<Word> Get(WordType type)
        {
            return Get().Where(p => p.Type == type);
        }

        public IEnumerable<Word> Find(string text)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<Word> words)
        {
            var editedWords = new List<Word>();

            foreach (var word in words)
            {
                var existingWord = _AllWords.FirstOrDefault(p => p.WordRaw == word.WordRaw);

                if (existingWord.IsNotNull())
                {
                    editedWords.Add(new Word(word.WordRaw, word.Type, LabelHelper.JoinLabels(existingWord.Labels, word.Labels, _AllLabels)));
                }
                else
                {
                    editedWords.Add(new Word(word.WordRaw, word.Type, LabelHelper.JoinLabels(new WordLabel[0], word.Labels, _AllLabels)));
                }
            }

            Delete(words);

            _AllWords.AddRange(editedWords);
            SortWords();
            IsModified = true;
        }        

        public void Delete(IEnumerable<Word> words)
        {
            foreach (var word in words)
            {
                int index = _AllWords.FindIndex(p => p.WordRaw == word.WordRaw);

                if (index >= 0)
                {
                    _AllWords.RemoveAt(index);
                }
            }

            SortWords();
            IsModified = true;
        }

        /// <summary>
        /// Save Items
        /// </summary>
        public void Save()
        {
            Checker.NotNullOrEmpty(_Filename, "File name");

            var doc = new XmlDocument();

            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><MyVocabulary version=\"1.1\"></MyVocabulary>");
            doc.DocumentElement.SetAttribute(ATTR_LANG, Lang.ToString());
            var nodeLabels = doc.DocumentElement.AddNode("Labels");
            var nodeWords = doc.DocumentElement.AddNode("Words");

            _AllLabels.OrderBy(p => p.Label).CallOnEach(p =>
                {
                    nodeLabels.AddNode("Item").AddAttribute("name", p.Label).AddAttribute("id", p.Id.ToString());
                });

            _AllWords.OrderBy(p => p.WordRaw).CallOnEach(p =>
                {
                    var node = nodeWords.AddNode("Item");
                    node.AddAttribute("word", p.WordRaw).AddAttribute("type", ((int)p.Type).ToString());

                    var labels = LabelsToAttribute(p.Labels);

                    if (labels.IsNotEmpty())
                    {
                        node.AddAttribute("labels", labels);
                    }
                });

            doc.Save(_Filename);

            IsModified = false;
        }

        public bool Rename(string oldWord, string newWord)
        {
            if (_AllWords.FindIndex(p => p.WordRaw == newWord) >= 0)
            {
                return false;
            }

            int indexOld = _AllWords.FindIndex(p => p.WordRaw == oldWord);

            if (indexOld < 0)
            {
                return false;
            }

            _AllWords[indexOld] = new Word(newWord, _AllWords[indexOld].Type, _AllWords[indexOld].Labels);

            SortWords();
            IsModified = true;

            return true;
        }

        

        public WordLabel UpdateLabel(WordLabel label)
        {
            WordLabel result = null;

            if (label.IsNew)
            {
                var found = _AllLabels.FirstOrDefault(p => p.EqualsName(label));

                if (found.IsNotNull())
                {
                    return null;
                }

                result = new WordLabel(NextLabelId(), label.Label);
                _AllLabels.Add(result);
            }
            else
            {
                var found = _AllLabels.FirstOrDefault(p => label.Id == p.Id);

                if (found.IsNull())
                {
                    return null;
                }

                if (_AllLabels.Any(p => p.EqualsName(label)))
                {
                    return null;
                }

                found.SetLabel(label.Label);

                result = found;
            }

            IsModified = true;

            return result;
        }

        private int NextLabelId()
        {
            if (_AllLabels.Any())
            {
                return _AllLabels.Max(p => p.Id) + 1;
            }

            return 1;
        }

        public void DeleteLabels(IEnumerable<WordLabel> labels)
        {
            var toRemove = labels.Where(p => !p.IsNew).ToList();

            foreach (var label in toRemove)
            {
                _AllLabels.Remove(label);
            }

            IsModified = true;
        }

        public void SetLabel(IEnumerable<Word> words, WordLabel label)
        {
            var editedWords = new List<Word>();

            foreach (var word in _AllWords.Where(p => words.Any(g => g.WordRaw == p.WordRaw)))
            {
                editedWords.Add(new Word(word.WordRaw, word.Type, LabelHelper.JoinLabels(word.Labels, new List<WordLabel> { label }, _AllLabels)));
            }

            Delete(words);

            _AllWords.AddRange(editedWords);

            IsModified = true;
        }

        public bool Exists(string name)
        {
            int index =_AllWords.BinarySearchIndex(p => p.WordRaw.CompareTo(name));

            return index >= 0;
        }        

        public Word GetByName(string name)
        {
            Word word = _AllWords.BinarySearch(p => p.WordRaw.CompareTo(name));

            return word;
        }

        #endregion

        #region IComparer<Word>

        int IComparer<Word>.Compare(Word x, Word y)
        {
            return x.WordRaw.CompareTo(y.WordRaw);
        }

        #endregion
    }
}
