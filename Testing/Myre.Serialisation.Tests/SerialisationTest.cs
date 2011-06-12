using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;

namespace Myre.Serialisation.Tests
{
    [TestClass]
    public class SerialisationTest
    {
        class NonParsable
        {
            public int A;
            public float B;
            public string C { get; set; }
            public string D { get; private set; }
        }

        [TestMethod]
        public void WriteParsableNumberObject()
        {
            Dom dom = Dom.Serialise(10);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.LiteralNode));

            Dom.LiteralNode node = dom.Root.GetNode() as Dom.LiteralNode;
            Assert.AreEqual(typeof(int), node.Type);
            Assert.AreEqual("10", node.Value);
        }

        [TestMethod]
        public void WriteParsableCustomObject()
        {
            var time = new DateTime(2010, 10, 4);
            Dom dom = Dom.Serialise(time);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.LiteralNode));

            Dom.LiteralNode node = dom.Root.GetNode() as Dom.LiteralNode;
            Assert.AreEqual(typeof(DateTime), node.Type);
            Assert.AreEqual(string.Format("{0}", time), node.Value);
        }

        [TestMethod]
        public void WriteNonParsableObject()
        {
            NonParsable foo = new NonParsable() { A = 5, B = 5.0f, C = "hello world" };
            Dom dom = Dom.Serialise(foo);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.ObjectNode));

            Dom.ObjectNode node = dom.Root.GetNode() as Dom.ObjectNode;
            
            Assert.AreEqual(typeof(NonParsable), node.Type);

            Assert.AreEqual(typeof(int), node.Children["A"].GetNode().Type);
            Assert.AreEqual("5", (node.Children["A"].GetNode() as Dom.LiteralNode).Value);

            Assert.AreEqual(typeof(float), node.Children["B"].GetNode().Type);
            Assert.AreEqual("5", (node.Children["B"].GetNode() as Dom.LiteralNode).Value);

            Assert.AreEqual(typeof(string), node.Children["C"].GetNode().Type);
            Assert.AreEqual("hello world", (node.Children["C"].GetNode() as Dom.LiteralNode).Value);
        }

        [TestMethod]
        public void WriteList()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4, 5 };
            Dom dom = Dom.Serialise(list);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.ListNode));

            Dom.ListNode node = dom.Root.GetNode() as Dom.ListNode;

            Assert.AreEqual(typeof(List<int>), node.Type);

            Assert.AreEqual(list.Count, node.Children.Count);
            for (int i = 0; i < list.Count; i++)
            {
                Assert.AreEqual(typeof(int), node.Children[i].GetNode().Type);
                Assert.AreEqual(string.Format("{0}", list[i]), (node.Children[i].GetNode() as Dom.LiteralNode).Value);
            }
        }

        [TestMethod]
        public void WriteArray()
        {
            int[] array = new int[] { 1, 2, 3, 4, 5 };
            Dom dom = Dom.Serialise(array);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.ListNode));

            Dom.ListNode node = dom.Root.GetNode() as Dom.ListNode;

            Assert.AreEqual(typeof(int[]), node.Type);

            Assert.AreEqual(array.Length, node.Children.Count);
            for (int i = 0; i < array.Length; i++)
            {
                Assert.AreEqual(typeof(int), node.Children[i].GetNode().Type);
                Assert.AreEqual(string.Format("{0}", array[i]), (node.Children[i].GetNode() as Dom.LiteralNode).Value);
            }
        }

        [TestMethod]
        public void WriteDictionary()
        {
            Dictionary<string, int> dictionary = new Dictionary<string, int>() { { "foo", 1 }, { "bar", 2 } };
            Dom dom = Dom.Serialise(dictionary);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.DictionaryNode));

            Dom.DictionaryNode node = dom.Root.GetNode() as Dom.DictionaryNode;

            Assert.AreEqual(typeof(Dictionary<string, int>), node.Type);

            Assert.AreEqual(dictionary.Count, node.Children.Count);
            foreach (var key in dictionary.Keys)
            {
                var matches = node.Children.Where(kvp =>
                    (kvp.Key.GetNode() as Dom.LiteralNode).Value == string.Format("{0}", key)
                 && (kvp.Value.GetNode() as Dom.LiteralNode).Value == string.Format("{0}", dictionary[key]));

                Assert.AreEqual(1, matches.Count());
            }
        }
    }
}
