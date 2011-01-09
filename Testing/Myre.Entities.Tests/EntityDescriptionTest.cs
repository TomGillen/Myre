using Myre.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Ninject;
using Myre.Entities.Behaviours;
using System.IO;
using MyreAssert = Myre.Assert;
using UAssert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ProtoBuf;
using Microsoft.Xna.Framework;

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
            [ProtoMember(2)]
            public string Foo = "hello world";

            [ProtoMember(3)]
            public Vector2 Vector = new Vector2(5, 10);
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
        public void BehaviourProtobufferSerialisation()
        {
            var kernel = new StandardKernel();
            var description = new EntityDescription_Accessor(kernel);

            var behaviour = new BasicBehaviour();
            behaviour.Foo = "foo";
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

        /// <summary>
        /// Tests that an entity description can be serialised and deserialised to/from
        /// the protobuffer protocol via protobuf-net.
        /// </summary>
        [TestMethod]
        [DeploymentItem("Myre.Entities.dll")]        
        public void EntityDescriptionProtobufferSerialisation()
        {
            var kernel = new StandardKernel();
            
            var original = kernel.Get<EntityDescription>();
            original.AddProperty<int>("bar", 5);
            original.AddBehaviour<BasicBehaviour>("behaviour1");

            var originalEntity = original.Create();

            byte[] buffer;
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize<EntityDescription>(stream, original);
                buffer = stream.ToArray();
            }

            var deserialised = kernel.Get<EntityDescription>();
            using (var stream = new MemoryStream(buffer))
                Serializer.Merge<EntityDescription>(stream, deserialised);

            var deserialisedEntity = deserialised.Create();

            UAssert.AreEqual(originalEntity.GetProperty<int>("bar").Value, deserialisedEntity.GetProperty<int>("bar").Value);
            UAssert.AreEqual(originalEntity.GetBehaviour<BasicBehaviour>("behaviour1").Foo, deserialisedEntity.GetBehaviour<BasicBehaviour>("behaviour1").Foo);
        }
    }
}
