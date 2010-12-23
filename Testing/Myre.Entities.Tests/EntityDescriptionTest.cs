using Myre.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ninject;
using Myre.Entities.Behaviours;
using System.IO;
using MyreAssert = Myre.Assert;
using UAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ProtoBuf;

namespace Myre.Entities.Tests
{
    
    
    /// <summary>
    ///This is a test class for EntityDescriptionTest and is intended
    ///to contain all EntityDescriptionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EntityDescriptionTest
    {
        [ProtoContract]
        class BasicBehaviour
            : Behaviour
        {
            [ProtoMember(1)]
            public string Foo = "hello world";
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
        /// Tests that entity descriptions can correctly serialise a behaviour into a BehaviourData struct,
        /// and then use that struct to instantiate a new clone of that behaviour.
        /// </summary>
        [TestMethod]
        [DeploymentItem("Myre.Entities.dll")]
        public void BehaviourProtobufSerialisation()
        {
            var kernel = new StandardKernel();
            var description = new EntityDescription_Accessor(kernel);

            var behaviour = new BasicBehaviour();
            var buffer = description.SerialiseBehaviour(behaviour);

            var behaviourData = new BehaviourData()
            {
                SerialisedValue = buffer,
                Type = typeof(BasicBehaviour)
            };

            var deserialised = description.CreateBehaviourInstance(kernel, behaviourData) as BasicBehaviour;
            UAssert.IsNotNull(deserialised);
            UAssert.AreEqual(deserialised.Foo, behaviour.Foo);
        }
    }
}
