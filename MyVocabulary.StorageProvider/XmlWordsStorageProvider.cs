﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MyVocabulary.StorageProvider.Enums;
using Shared.Extensions;
using Shared.Helpers;

namespace MyVocabulary.StorageProvider
{
    public class XmlWordsStorageProvider : IWordsStorageProvider
    {
        #region Fields

        private readonly IList<Word> _AllWords;
        private string _Filename;

        #endregion

        #region Ctors
        
        public XmlWordsStorageProvider()
        {
            _AllWords = new List<Word>();

            _AllWords.AddRange(new List<Word>() 
            { 
                new Word("hello", WordType.Known), 
                new Word("world", WordType.Known) ,
                new Word("just", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("cat", WordType.Known) ,
                new Word("dog", WordType.Known) 
            });
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
            _AllWords.AddRange(doc.DocumentElement.SelectNodes("Words/Item").OfType<XmlNode>().Select(p => LoadFromXml(p)));
        }
        
        #endregion

        #region Private
        
        private Word LoadFromXml(XmlNode node)
        {
            return new Word(node.GetAttribute("word"), (WordType)Convert.ToInt32(node.GetAttribute("type")));
        }
        
        #endregion
        
        #endregion

        #region IWordsStorageProvider

        public IEnumerable<Word> Get()
        {
            return _AllWords;
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
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<Word> words)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Save Items
        /// </summary>
        public void Save()
        {
            Checker.NotNullOrEmpty(_Filename, "File name");

            var doc = new XmlDocument();

            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\" ?><MyVocabulary version=\"1.0\"></MyVocabulary>");
            var nodeWords = doc.DocumentElement.AddNode("Words");

            _AllWords.OrderBy(p => p.Type).CallOnEach(p => 
                {
                    nodeWords.AddNode("Item").AddAttribute("word", p.WordRaw).AddAttribute("type", ((int)p.Type).ToString());
                });
        }
        
        #endregion
    }
}
