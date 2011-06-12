using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Myre.Serialisation.Tests
{
    [TestClass]
    public class SaveTest
    {
        struct NonParsable
        {
            public int A;
            public float B;
            public string C { get; set; }
            public string D { get; private set; }
        }

        class CompositeType
        {
            public string Foo;
            public NonParsable Bar;
        }

        class TwoStrings
        {
            public string A;
            public string B;
        }

        [TestMethod]
        public void SaveLiteral()
        {
            Dom dom = Dom.Serialise("hello");
            string output = dom.Save();

            Assert.AreEqual("String \"hello\"", output);
        }

        [TestMethod]
        public void SaveObject()
        {
            NonParsable foo = new NonParsable() { A = 5, B = 5.0f, C = "hello \"world\"!" };
            Dom dom = Dom.Serialise(foo);
            
            string output = dom.Save();
            string expected = "SaveTest+NonParsable {\n  A: Int32 \"5\",\n  B: Single \"5\",\n  C: String \"hello \\\"world\\\"!\",\n  D: null\n}";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void SaveList()
        {
            List<int> list = new List<int>() { 1, 2, 3, 4, 5 };
            Dom dom = Dom.Serialise(list);

            string output = dom.Save();
            string expected = "List<Int32> [\n  Int32 \"1\",\n  Int32 \"2\",\n  Int32 \"3\",\n  Int32 \"4\",\n  Int32 \"5\"\n]";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void SaveDictionary()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>() { { 1, "one" }, { 2, "two" }, { 3, "three" } };
            Dom dom = Dom.Serialise(dictionary);

            string output = dom.Save();
            string expected = "Dictionary<Int32,String> [\n  Int32 \"1\": String \"one\",\n  Int32 \"2\": String \"two\",\n  Int32 \"3\": String \"three\"\n]";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void SaveCompositeType()
        {
            CompositeType value = new CompositeType();
            value.Foo = "foobar";
            value.Bar.A = 5;
            value.Bar.B = 10;
            value.Bar.C = "hello \"world\"!";

            Dom dom = Dom.Serialise(value);

            string output = dom.Save();
            string expected = "SaveTest+CompositeType {\n  Foo: String \"foobar\",\n  Bar: SaveTest+NonParsable {\n    A: Int32 \"5\",\n    B: Single \"10\",\n    C: String \"hello \\\"world\\\"!\",\n    D: null\n  }\n}";
            
            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void SaveCompositeTypeDictionary()
        {
            CompositeType value = new CompositeType();
            value.Foo = "foobar";
            value.Bar.A = 5;
            value.Bar.B = 10;
            value.Bar.C = "hello \"world\"!";

            var dictionary = new Dictionary<CompositeType, CompositeType>() { { value, value } };
            Dom dom = Dom.Serialise(dictionary);

            string output = dom.Save();
            string expected = "Dictionary<SaveTest+CompositeType,SaveTest+CompositeType> [\n  SaveTest+CompositeType {\n    Foo: String \"foobar\",\n    Bar: SaveTest+NonParsable {\n      A: Int32 \"5\",\n      B: Single \"10\",\n      C: String \"hello \\\"world\\\"!\",\n      D: null\n    }\n  }: SaveTest+CompositeType {\n    Foo: String \"foobar\",\n    Bar: SaveTest+NonParsable {\n      A: Int32 \"5\",\n      B: Single \"10\",\n      C: String \"hello \\\"world\\\"!\",\n      D: null\n    }\n  }\n]";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void SaveArray()
        {
            int[] value = new int[] { 1, 2, 3, 4 };

            Dom dom = Dom.Serialise(value);

            string output = dom.Save();
            string expected = "Int32[] [\n  Int32 \"1\",\n  Int32 \"2\",\n  Int32 \"3\",\n  Int32 \"4\"\n]";

            Assert.AreEqual(expected, output);
        }

        [TestMethod]
        public void SaveSharedResources()
        {
            string foo = "Foo";
            TwoStrings value = new TwoStrings() { A = foo, B = foo };

            Dom dom = Dom.Serialise(value);

            string output = dom.Save();
            string expected = "R[\n  0: String \"Foo\"\n]\nSaveTest+TwoStrings {\n  A: #0,\n  B: #0\n}";

            Assert.AreEqual(expected, output);
        }
    }
}
