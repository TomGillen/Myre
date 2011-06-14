using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Myre.Serialisation.Tests
{
    /// <summary>
    /// Summary description for LoadTests
    /// </summary>
    [TestClass]
    public class LoadTest
    {
        public class Generic<T, F>
        {
            public class CompositeType<T>
            {
                public string Foo;
            }
        }

        class TwoStrings
        {
            public string A;
            public string B;
        }

        public LoadTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void LoadLiteral()
        {
            string encodedLiteral = "String \"Hello\"";
            var dom = Dom.Load(encodedLiteral);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.LiteralNode));
            Assert.AreEqual("Hello", (dom.Root.GetNode() as Dom.LiteralNode).Value);
            Assert.AreEqual(typeof(string), (dom.Root.GetNode() as Dom.LiteralNode).Type);
        }

        [TestMethod]
        public void LoadList()
        {
            string input = "List<Int32> [\n  Int32 \"1\",\n  Int32 \"2\"\n]";
            Dom dom = Dom.Load(input);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.ListNode));
            Assert.AreEqual(typeof(List<int>), dom.Root.GetNode().Type);
            Assert.IsNotNull((dom.Root.GetNode() as Dom.ListNode).Children);
            Assert.AreEqual(2, (dom.Root.GetNode() as Dom.ListNode).Children.Count);

            var list = (dom.Root.GetNode() as Dom.ListNode).Children;
            for (int i = 0; i < list.Count; i++)
            {
                Assert.IsInstanceOfType(list[i].GetNode(), typeof(Dom.LiteralNode));
                Assert.AreEqual(typeof(int), list[i].GetNode().Type);
                Assert.AreEqual((i + 1).ToString(), (list[i].GetNode() as Dom.LiteralNode).Value);
            }
        }

        [TestMethod]
        public void LoadDictionary()
        {
            string input = "Dictionary<Int32,String> [\n  Int32 \"1\": String \"one\"\n]";
            Dom dom = Dom.Load(input);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.DictionaryNode));
            Assert.AreEqual(typeof(Dictionary<int, string>), dom.Root.GetNode().Type);
            Assert.IsNotNull((dom.Root.GetNode() as Dom.DictionaryNode).Children);
            Assert.AreEqual(1, (dom.Root.GetNode() as Dom.DictionaryNode).Children.Count);

            var dictionary = (dom.Root.GetNode() as Dom.DictionaryNode).Children;
            var element = dictionary.First();

            Assert.IsInstanceOfType(element.Key.GetNode(), typeof(Dom.LiteralNode));
            Assert.AreEqual(typeof(int), element.Key.GetNode().Type);
            Assert.AreEqual(1.ToString(), (element.Key.GetNode() as Dom.LiteralNode).Value);

            Assert.IsInstanceOfType(element.Value.GetNode(), typeof(Dom.LiteralNode));
            Assert.AreEqual(typeof(string), element.Value.GetNode().Type);
            Assert.AreEqual("one", (element.Value.GetNode() as Dom.LiteralNode).Value);
        }

        [TestMethod]
        public void LoadObject()
        {
            Debug.WriteLine(typeof(Generic<int, string>.CompositeType<float>).FullName);

            string input = "LoadTest+Generic<Int32, String>+CompositeType<Single> { Foo: String \"Hello\" }";
            Dom dom = Dom.Load(input);

            Assert.IsInstanceOfType(dom.Root.GetNode(), typeof(Dom.ObjectNode));
            Assert.AreEqual(typeof(Generic<int, string>.CompositeType<float>), dom.Root.GetNode().Type);
            Assert.AreEqual(1, (dom.Root.GetNode() as Dom.ObjectNode).Children.Count);

            var element = (dom.Root.GetNode() as Dom.ObjectNode).Children["Foo"];
            Assert.IsInstanceOfType(element.GetNode(), typeof(Dom.LiteralNode));
            Assert.AreEqual(typeof(string), element.GetNode().Type);
            Assert.AreEqual("Hello", (element.GetNode() as Dom.LiteralNode).Value);
        }

        [TestMethod]
        public void LoadSharedResources()
        {
            string input = "R[\n  0: String \"Foo\"\n]\nLoadTest+TwoStrings {\n  A: #0,\n  B: #0\n}";
            Dom dom = Dom.Load(input);

            TwoStrings output = dom.Deserialise<TwoStrings>();
            Assert.IsNotNull(output);
            Assert.AreEqual("Foo", output.A);
            Assert.AreEqual("Foo", output.B);
            Assert.ReferenceEquals(output.A, output.B);
        }
    }
}
