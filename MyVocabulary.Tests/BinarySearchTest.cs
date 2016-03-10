using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shared.Extensions;
using System.Collections.Generic;

namespace MyVocabulary.Tests
{
    [TestClass]
    public class BinarySearchTest
    {
        [TestMethod]
        public void BinarySearchIndex1()
        {
            var list = new List<int>() { 12, 15, 16, 18 };

            int index = list.BinarySearchIndex(p => intPredicate(p, 16));

            Assert.AreEqual(2, index);
        }

        [TestMethod]
        public void BinarySearchIndex2()
        {
            var list = new List<int>() { 12, 15, 16, 18, 20 };

            int index = list.BinarySearchIndex(p => intPredicate(p, 18));

            Assert.AreEqual(3, index);
        }

        [TestMethod]
        public void BinarySearchIndex3()
        {
            var list = new List<int>() { 12, 20 };

            int index = list.BinarySearchIndex(p => intPredicate(p, 12));

            Assert.AreEqual(0, index);
        }

        [TestMethod]
        public void BinarySearchIndex4()
        {
            var list = new List<int>() { 12, 20 };

            int index = list.BinarySearchIndex(p => intPredicate(p, 20));

            Assert.AreEqual(1, index);
        }

        [TestMethod]
        public void BinarySearchIndex5()
        {
            var list = new List<int>() { 12, 20, 99 };

            int index = list.BinarySearchIndex(p => intPredicate(p, 99));

            Assert.AreEqual(2, index);
        }

        [TestMethod]
        public void BinarySearchIndexEmpty()
        {
            var list = new List<int>();

            int index = list.BinarySearchIndex(p => intPredicate(p, 16));

            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void BinarySearchIndexAll()
        {
            var list = new List<int>() { 12, 15, 16, 18, 21, 45 };
            int counter = 0;

            foreach (int val in list)
            {
                int index = list.BinarySearchIndex(p => intPredicate(p, val));

                Assert.AreEqual(counter, index);

                counter++;
            }
        }

        private static int intPredicate(int val1, int val2)
        {
            if (val1 > val2)
            {
                return 1;
            }

            return val2 > val1 ? -1 : 0;
        }
    }
}
