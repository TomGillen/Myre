using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Myre.Entities.Behaviours;
using Ninject;
using Ninject.Parameters;
using System.Xml.Linq;
using Myre;
using Myre.Extensions;
using System.IO;
using ProtoBuf;
using Myre.Entities.Serialisation;

namespace Myre.Entities
{
    /// <summary>
    /// An enum which defines how property values are copied into new property instances.
    /// </summary>
    public enum PropertyCopyBehaviour
    {
        /// <summary>
        /// The value is assigned to the property instance.
        /// </summary>
        None,

        /// <summary>
        /// The value is copied to the property instance via the ICopyable interface.
        /// </summary>
        Copy,

        /// <summary>
        /// A new instance is created of the property type.
        /// </summary>
        New
    }

    /// <summary>
    /// A struct which contains data about an entity property.
    /// </summary>
    [ProtoContract]
    public class PropertyData
    {
        [ProtoMember(1)]
        public PropertyCopyBehaviour CopyBehaviour;

        [ProtoMember(2)]
        public string Name;

        [ProtoMember(3)]
        public byte[] SerialisedValue;

        [ProtoMember(4)]
        public string DataTypeName;

        public object Value;
        public Type DataType;

        public void Rehydrate()
        {
            // restore the data type or type name
            if (DataType == null)
                DataType = Type.GetType(DataTypeName);
            else if (DataTypeName == null)
                DataTypeName = DataType.AssemblyQualifiedName;

#if PROTOBUFFERS
            // restore value or serialised value
            if (Value == null && SerialisedValue != null)
            {
                using (var stream = new MemoryStream(SerialisedValue))
                    Value = ProtobufNonGenericAdaptor.Deserialise(stream, DataType);
            }
            else if (Value != null && SerialisedValue == null)
            {
                using (var stream = new MemoryStream())
                {
                    ProtobufNonGenericAdaptor.Serialise(stream, Value, DataType);
                    SerialisedValue = stream.ToArray();
                }
            }
#endif
        }

        public object CreateValue(IKernel kernel)
        {
            switch (CopyBehaviour)
            {
                case PropertyCopyBehaviour.None:
                    return Value;

                case PropertyCopyBehaviour.Copy:
                    var cloneable = Value as ICopyable;
                    return cloneable.Copy();

                case PropertyCopyBehaviour.New:
                    return kernel.Get(DataType);

                default:
                    return null;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is PropertyData)
                return Equals(obj as PropertyData);
            
            return base.Equals(obj);
        }

        public bool Equals(PropertyData data)
        {
            return this.Name == data.Name
                && this.DataTypeName == data.DataTypeName;
        }
    }

    /// <summary>
    /// A struct which contains data about an entity behaviour.
    /// </summary>
    [ProtoContract]
    public class BehaviourData
    {
        [ProtoMember(1)]
        public string Name;

        [ProtoMember(2)]
        public byte[] SerialisedValue;

        [ProtoMember(3)]
        public string TypeName;

        public Type Type;
        
        internal MemoryStream MemoryStream;

        public void Rehydrate()
        {
            // restore the data type or type name
            if (Type == null)
                Type = Type.GetType(TypeName);
            else if (TypeName == null)
                TypeName = Type.AssemblyQualifiedName;

            // restore memory stream
            if (SerialisedValue != null && MemoryStream == null)
                MemoryStream = new MemoryStream(SerialisedValue);
        }

        public override bool Equals(object obj)
        {
            if (obj is BehaviourData)
                return Equals(obj as BehaviourData);

            return base.Equals(obj);
        }

        public bool Equals(BehaviourData data)
        {
            return this.Name == data.Name
                && this.TypeName == data.TypeName;
        }
    }

    /// <summary>
    /// A class which describes the elements of an entity, and can be used to construct new entity instances.
    /// </summary>
    [ProtoContract]
    public class EntityDescription
    {
        private static Dictionary<Type, Type> propertyTypes = new Dictionary<Type, Type>();
        private static Type genericType = Type.GetType("Myre.Entities.Property`1");

        private IKernel kernel;
        private List<BehaviourData> behaviours;
        private List<PropertyData> properties;

        private Queue<Entity> pool;
        internal uint Version;

        /// <summary>
        /// Gets a list of behaviours in this instance.
        /// </summary>
        /// <value>The behaviours.</value>
        [ProtoMember(1)]
        public List<BehaviourData> Behaviours 
        { 
            get { return behaviours; }
            set
            {
                Assert.ArgumentNotNull("value", value);
                IncrementVersion();
                behaviours = value;
            }
        }
        
        /// <summary>
        /// Gets a list of properties in this instance.
        /// </summary>
        /// <value>The properties.</value>
        [ProtoMember(2)]
        public List<PropertyData> Properties 
        { 
            get { return properties; }
            set
            {
                Assert.ArgumentNotNull("value", value);
                IncrementVersion();
                properties = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDescription"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public EntityDescription(IKernel kernel)
        {
            this.kernel = kernel;
            this.behaviours = new List<BehaviourData>();
            this.properties = new List<PropertyData>();
            this.pool = new Queue<Entity>();
        }

        /// <summary>
        /// Resets this instance, clearing all property and behaviour deta.
        /// </summary>
        public void Reset()
        {
            behaviours.Clear();
            properties.Clear();
            IncrementVersion();
        }

        /// <summary>
        /// Adds all the properties and behaviours from the specified entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void AddFrom(Entity entity, bool serialisePropertyValues = true, bool serialiseBehaviourValues = true)
        {
            foreach (var item in entity.Behaviours)
            {
                AddBehaviour(new BehaviourData()
                {
                    Name = item.Name,
                    Type = item.GetType(),
                    SerialisedValue = serialiseBehaviourValues ? SerialiseBehaviour(item) : null
                });
            }

            foreach (var item in entity.Properties)
            {
                AddProperty(new PropertyData()
                {
                    Name = item.Name,
                    DataType = item.Type,
                    Value = serialisePropertyValues ? item.Value : null,
                    CopyBehaviour = item.CopyBehaviour
                });
            }
        }

        /// <summary>
        /// Adds all the properties and behaviours from the specified entity description.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public void AddFrom(EntityDescription description)
        {
            foreach (var item in description.Behaviours)
                AddBehaviour(item);

            foreach (var item in description.Properties)
                AddProperty(item);
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour(BehaviourData behaviour)
        {
            Assert.ArgumentNotNull("behaviour.Type", behaviour.Type);

            behaviour.Rehydrate();

            if (behaviours.Contains(behaviour))
                return false;

            behaviours.Add(behaviour);
            IncrementVersion();

            return true;
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="settings">The settings.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour(Type type, string name = null)
        {
            return AddBehaviour(new BehaviourData() { Type = type, Name = name });
        }

        /// <summary>
        /// Adds the behaviour, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="settings">The settings.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddBehaviour<T>(string name = null)
            where T : Behaviour
        {
            return AddBehaviour(typeof(T), name);
        }

        /// <summary>
        /// Removes the behaviour.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveBehaviour(Type type, string name = null)
        {
            for (int i = 0; i < behaviours.Count; i++)
            {
                var item = behaviours[i];
                if (item.Type == type && (name == null || item.Name == name))
                {
                    behaviours.RemoveAt(i);
                    IncrementVersion();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes the behaviour.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveBehaviour<T>(string name = null)
            where T : Behaviour
        {
            return RemoveBehaviour(typeof(T), name);
        }

        /// <summary>
        /// Adds the property, provided that such a property does not already exist.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty(PropertyData property)
        {
            Assert.ArgumentNotNull("property.Name", property.Name);
            Assert.ArgumentNotNull("property.DataType", property.DataType);

            property.Rehydrate();

            if (properties.Contains(property))
                return false;

            properties.Add(property);
            IncrementVersion();

            return true;
        }

        /// <summary>
        /// Adds the property, provided that such a behaviour does not already exist.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="copyBehaviour">The copy behaviour.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty(Type dataType, string name, object initialValue = null, PropertyCopyBehaviour copyBehaviour = PropertyCopyBehaviour.None)
        {
            var data = new PropertyData()
            {
                Name = name,
                DataType = dataType,
                Value = initialValue,
                CopyBehaviour = copyBehaviour,
            };

            return AddProperty(data);
        }

        /// <summary>
        /// Adds the property, provided that such a behaviour does not already exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="copyBehaviour">The copy behaviour.</param>
        /// <returns><c>true</c> if the behaviour was added; else <c>false</c>.</returns>
        public bool AddProperty<T>(string name, T initialValue = default(T), PropertyCopyBehaviour copyBehaviour = PropertyCopyBehaviour.None)
        {
            return AddProperty(typeof(T), name, initialValue, copyBehaviour);
        }

        /// <summary>
        /// Removes the property.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public bool RemoveProperty(string name)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i].Name == name)
                {
                    properties.RemoveAt(i);
                    IncrementVersion();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Creates a new entity with the properties and behaviours described by this instance.
        /// </summary>
        /// <returns></returns>
        public Entity Create()
        {
            Entity e;

            if (pool.Count > 0)
                e = InitialisePooledEntity();
            else
                e = new Entity(CreateProperties(), CreateBehaviours(), new EntityVersion(this, Version));

            e.CreateProperties();

            return e;
        }

        /// <summary>
        /// Makes the specified entity available for re-use.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns><c>true</c> if the entity was recycled; else <c>false</c>.</returns>
        public bool Recycle(Entity entity)
        {
            if (entity.Version.Creator != this || entity.Version.Version != Version)
                return false;

            pool.Enqueue(entity);
            return true;
        }

        private Entity InitialisePooledEntity()
        {
            var entity = pool.Dequeue();

            foreach (var item in entity.Properties)
            {
                for (int i = 0; i < properties.Count; i++)
                {
                    if (properties[i].Name == item.Name)
                    {
                        var data = properties[i].CreateValue(kernel);
                        item.Value = data;
                        break;
                    }
                }                
            }

            return entity;
        }

        private void IncrementVersion()
        {
            unchecked
            {
                Version++;
            }

            pool.Clear();
        }

        private IEnumerable<IProperty> CreateProperties()
        {
            foreach (var item in properties)
                yield return CreatePropertyInstance(kernel, item);
        }

        private IEnumerable<Behaviour> CreateBehaviours()
        {
            foreach (var item in behaviours)
                yield return CreateBehaviourInstance(kernel, item);
        }

        private IProperty CreatePropertyInstance(IKernel kernel, PropertyData property)
        {
            property.Rehydrate();

            Type type;
            if (!propertyTypes.TryGetValue(property.DataType, out type))
            {
                type = genericType.MakeGenericType(property.DataType);
                propertyTypes.Add(property.DataType, type);
            }

            var data = property.CreateValue(kernel);
            return type.CreateInstance(
                new Type[] { typeof(string), property.DataType, typeof(PropertyCopyBehaviour) },
                new object[] { property.Name, data, property.CopyBehaviour })
               as IProperty;
        }

        private Behaviour CreateBehaviourInstance(IKernel kernel, BehaviourData behaviour)
        {
            behaviour.Rehydrate();

            var name = new ConstructorArgument("name", behaviour.Name);
            var instance = kernel.Get(behaviour.Type, name) as Behaviour;
            instance.Name = behaviour.Name;

            if (behaviour.SerialisedValue != null)
            {
                behaviour.MemoryStream.Position = 0;
                ProtobufNonGenericAdaptor.Merge(behaviour.MemoryStream, instance, behaviour.Type);
            }

            return instance;
        }

        private byte[] SerialiseBehaviour(Behaviour behaviour)
        {
#if PROTOBUFFERS
            using (var stream = new MemoryStream())
            {
                ProtobufNonGenericAdaptor.Serialise(stream, behaviour, behaviour.GetType());
                return stream.ToArray();
            }
#else
            return null;
#endif
        }
    }
}
