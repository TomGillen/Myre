using Myre.Serialisation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using Microsoft.Xna.Framework;
using System.Text;
using System.Collections.Generic;

namespace Myre.Serialisation.Tests
{
    
    
    /// <summary>
    ///This is a test class for SerialisationWriterTest and is intended
    ///to contain all SerialisationWriterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SerialisationWriterTest
    {
        class NonParsable
        {
            public int A;
            public string B;
            public string C { get; set; }
            public string D { get; private set; }
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
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for WriteObject
        ///</summary>
        [TestMethod()]
        public void WriteParsableObject()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>());
                writer.WriteObject(10, typeof(int));

                var s = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("\"10\"", s);
            }

            using (var stream = new MemoryStream())
            {
                var time = new DateTime(2010, 10, 4);
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>());
                writer.WriteObject(time, typeof(DateTime));

                var s = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual(time.ToString(), s);
            }
        }

        [TestMethod]
        public void WriteCustomSerialiserObject()
        {
            using (var stream = new MemoryStream())
            {
                Action<SerialisationWriter, object> stringFormatter = (w, s) => w.WriteLiteral(s);
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>() { { typeof(string), stringFormatter } });

                writer.WriteObject("foo", typeof(string));

                var result = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("\"foo\"", result);
            }
        }

        [TestMethod]
        public void WriteParsableObjectUnexpectedType()
        {
            using (var stream = new MemoryStream())
            {
                var time = new DateTime(2010, 10, 4);
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>());
                writer.WriteObject(time, typeof(object));

                var s = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("DateTime \"" + time.ToString() + "\"", s);
            }
        }

        /// <summary>
        ///A test for WriteLiteral
        ///</summary>
        [TestMethod()]
        public void WriteLiteral()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>());
                writer.WriteLiteral("foo");

                var s = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("\"foo\"", s);
            }
        }

        /// <summary>
        ///A test for WriteKeyValue
        ///</summary>
        [TestMethod()]
        public void WriteParsableKeyValue()
        {
            using (var stream = new MemoryStream())
            {
                var time = new DateTime(2010, 10, 4);
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>());
                writer.WriteKeyValue("Time", time, typeof(object));

                var s = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("Time: DateTime \"" + time.ToString() + "\",", s);
            }
        }

        /// <summary>
        ///A test for StartCompositeObject
        ///</summary>
        [TestMethod()]
        public void StartCompositeObject()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>());
                writer.StartCompositeObject();

                var s = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("{", s);
            }
        }

        /// <summary>
        ///A test for IsParsable
        ///</summary>
        [TestMethod()]
        [DeploymentItem("Myre.Serialisation.dll")]
        public void IsParsable()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>());
                Assert.IsTrue(writer.IsParsable(typeof(int)));
                Assert.IsTrue(writer.IsParsable(typeof(float)));
                
                Assert.IsFalse(writer.IsParsable(typeof(Vector2)));                
                Assert.IsFalse(writer.IsParsable(typeof(NonParsable)));
            }
        }

        /// <summary>
        ///A test for EndCompositeObject
        ///</summary>
        [TestMethod()]
        public void EndCompositeObject()
        {
            using (var stream = new MemoryStream())
            {
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type,Action<SerialisationWriter,object>>());
                writer.EndCompositeObject();

                var s = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("}", s);
            }
        }

        [TestMethod]
        public void WriteCustomCompositeObjectWithParsableFields()
        {
            using (var stream = new MemoryStream())
            {
                Action<SerialisationWriter, object> stringFormatter = (w, s) => w.WriteLiteral(s);
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type,Action<SerialisationWriter,object>>() { {typeof(string), stringFormatter} });

                writer.StartCompositeObject();
                writer.WriteKeyValue("A", 5, typeof(int));
                writer.WriteKeyValue("B", "hello", typeof(string));
                writer.WriteKeyValue("C", "hello", null);
                writer.EndCompositeObject();

                var result = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("{A: 5,B: \"hello\",C: String \"hello\",}", result);
            }
        }

        [TestMethod]
        public void WriteNonParsableObject()
        {
            using (var stream = new MemoryStream())
            {
                Action<SerialisationWriter, object> stringFormatter = (w, s) => w.WriteLiteral(s);
                var writer = new SerialisationWriter_Accessor(stream, new Dictionary<Type, Action<SerialisationWriter, object>>() { { typeof(string), stringFormatter } });

                var foo = new NonParsable() { A = 5, B = "hello", C = "hello" };
                writer.WriteObject(foo, typeof(NonParsable));

                var result = Encoding.ASCII.GetString(stream.ToArray());
                Assert.AreEqual("{C: \"hello\",A: 5,B: \"hello\",}", result);
            }
        }
    }
}
