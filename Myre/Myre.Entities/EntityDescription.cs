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
    public struct PropertyData
    {
        public PropertyCopyBehaviour CopyBehaviour;
        public Type DataType;
        public string Name;
        public object Data;

        public object CreateValue(IKernel kernel)
        {
            switch (CopyBehaviour)
            {
                case PropertyCopyBehaviour.None:
                    return Data;

                case PropertyCopyBehaviour.Copy:
                    var cloneable = Data as ICopyable;
                    return cloneable.Copy();

                case PropertyCopyBehaviour.New:
                    return kernel.Get(DataType);

                default:
                    return null;
            }
        }
    }

    /// <summary>
    /// A struct which contains data about an entity behaviour.
    /// </summary>
    public struct BehaviourData
    {
        public Type Type;
        public string Name;
        public XElement Settings;
    }

    /// <summary>
    /// A class which describes the elements of an entity, and can be used to construct new entity instances.
    /// </summary>
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
        public List<BehaviourData> Behaviours { get { return behaviours; } }
        
        /// <summary>
        /// Gets a list of properties in this instance.
        /// </summary>
        /// <value>The properties.</value>
        public List<PropertyData> Properties { get { return properties; } }

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
        public void AddFrom(Entity entity)
        {
            foreach (var item in entity.Behaviours)
            {
                behaviours.Add(new BehaviourData()
                {
                    Name = item.Name,
                    Type = item.GetType(),
                    Settings = item.Settings
                });
            }

            foreach (var item in entity.Properties)
            {
                properties.Add(new PropertyData()
                {
                    Name = item.Name,
                    DataType = item.Type,
                    Data = item.Value,
                    CopyBehaviour = item.CopyBehaviour
                });
            }

            IncrementVersion();
        }

        /// <summary>
        /// Adds the behaviour.
        /// </summary>
        /// <param name="behaviour">The behaviour.</param>
        public void AddBehaviour(BehaviourData behaviour)
        {
            Assert.ArgumentNotNull("behaviour.Type", behaviour.Type);

            behaviours.Add(behaviour);
            IncrementVersion();
        }

        /// <summary>
        /// Adds the behaviour.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="settings">The settings.</param>
        public void AddBehaviour(Type type, string name = null, XElement settings = null)
        {
            AddBehaviour(new BehaviourData() { Type = type, Name = name, Settings = settings });
        }

        /// <summary>
        /// Adds the behaviour.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="settings">The settings.</param>
        public void AddBehaviour<T>(string name = null, XElement settings = null)
            where T : Behaviour
        {
            AddBehaviour(typeof(T), name, settings);
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
        /// Adds the property.
        /// </summary>
        /// <param name="property">The property.</param>
        public void AddProperty(PropertyData property)
        {
            Assert.ArgumentNotNull("property.Name", property.Name);
            Assert.ArgumentNotNull("property.DataType", property.DataType);

            properties.Add(property);
            IncrementVersion();
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="copyBehaviour">The copy behaviour.</param>
        public void AddProperty(Type dataType, string name, object initialValue = null, PropertyCopyBehaviour copyBehaviour = PropertyCopyBehaviour.None)
        {
            AddProperty(new PropertyData()
            {
                Name = name,
                DataType = dataType,
                Data = initialValue,
                CopyBehaviour = copyBehaviour
            });
        }

        /// <summary>
        /// Adds the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <param name="initialValue">The initial value.</param>
        /// <param name="copyBehaviour">The copy behaviour.</param>
        public void AddProperty<T>(string name, T initialValue = default(T), PropertyCopyBehaviour copyBehaviour = PropertyCopyBehaviour.None)
        {
            AddProperty(typeof(T), name, initialValue, copyBehaviour);
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
            if (pool.Count > 0)
                return InitialisePooledEntity();

            return new Entity(CreateProperties(), CreateBehaviours(), new EntityVersion(this, Version));
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
            var name = new ConstructorArgument("name", behaviour.Name);
            var settings = new ConstructorArgument("settings", behaviour.Settings);
            var entity = new Parameter("entity", this, true);
            var instance = kernel.Get(behaviour.Type, name, settings, entity) as Behaviour;
            instance.Settings = behaviour.Settings;

            return instance;
        }
    }
}
