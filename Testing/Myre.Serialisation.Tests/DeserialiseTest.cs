using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Myre.Serialisation.Tests
{
    [TestClass]
    public class DeserialiseTest
    {
        public struct NonParsable
        {
            public int A;
            public float B;
            public string C { get; set; }
            public string D { get; private set; }
        }

        [TestMethod]
        public void DeserialiseLiteral()
        {
            var input = "String \"Hello\"";
            var dom = Dom.Load(input);
            
            var expected = "Hello";
            var result = dom.Deserialise<string>();

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void DeserialiseList()
        {
            var input = "List<Int32> [\n  Int32 \"1\",\n  Int32 \"2\"\n]";
            var dom = Dom.Load(input);

            var result = dom.Deserialise<List<int>>();

            Assert.AreEqual(result[0], 1);
            Assert.AreEqual(result[1], 2);
        }

        [TestMethod]
        public void DeserialiseArray()
        {
            var input = "Int32[] [\n  Int32 \"1\",\n  Int32 \"2\"\n]";
            var dom = Dom.Load(input);

            var result = dom.Deserialise<int[]>();

            Assert.AreEqual(result[0], 1);
            Assert.AreEqual(result[1], 2);
        }

        [TestMethod]
        public void DeserialiseDictionary()
        {
            string input = "Dictionary<Int32,String> [\n  Int32 \"1\": String \"one\"\n]";
            Dom dom = Dom.Load(input);

            var result = dom.Deserialise<Dictionary<int, string>>();

            Assert.AreEqual(result[1], "one");
        }

        [TestMethod]
        public void DeserialiseObject()
        {
            var expected = new NonParsable() { A = 5, B = 5.0f, C = "hello world" };
            
            Dom dom = Dom.Serialise(expected);
            var actual = dom.Deserialise<NonParsable>();

            Assert.AreEqual(expected, actual);
        }
    }
}
