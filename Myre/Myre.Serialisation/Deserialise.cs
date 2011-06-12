using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Ninject;
using System.Collections;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        public abstract partial class Node
        {
            public abstract object Deserialise(IKernel kernel, Type expectedType);
        }

        public partial class LiteralNode
        {
            private MethodInfo parseMethod;
            private static Dictionary<Type, MethodInfo> parseMethods = new Dictionary<Type, MethodInfo>();

            public override object Deserialise(IKernel kernel, Type expectedType)
            {
                if (Type == typeof(string))
                    return Value;

                FindParseMethod();
                return parseMethod.Invoke(null, new object[] { Value.Unescape() });
            }

            private void FindParseMethod()
            {
                if (parseMethod != null)
                    return;
                
                if (!parseMethods.TryGetValue(Type, out parseMethod))
                {
                    parseMethod = Type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null);
                    parseMethods[Type] = parseMethod;
                }
            }
        }

        public partial class ListNode
        {
            public override object Deserialise(IKernel kernel, Type expectedType)
            {
                var type = Type ?? expectedType;

                if (type.IsArray)
                {
                    var instance = Array.CreateInstance(type.GetElementType(), Children.Count);

                    for (int i = 0; i < Children.Count; i++)
                    {
                        var child = Children[i].GetNode().Deserialise(kernel, type.GetElementType());
                        instance.SetValue(child, i);
                    }

                    return instance;
                }
                else
                {
                    var instance = CreateInstance(type, kernel);

                    IList list = instance as IList;
                    foreach (var item in Children)
                    {
                        var child = DeserialiseNode(item.GetNode(), kernel, typeof(object));
                        list.Add(child);
                    }

                    return list;
                }
            }
        }

        public partial class DictionaryNode
        {
            public override object Deserialise(IKernel kernel, Type expectedType)
            {
                var type = Type ?? expectedType;
                var instance = CreateInstance(type, kernel);

                IDictionary dictionary = instance as IDictionary;
                foreach (var item in Children)
                {
                    var key = DeserialiseNode(item.Key.GetNode(), kernel, typeof(object));
                    var value = DeserialiseNode(item.Value.GetNode(), kernel, typeof(object));
                    dictionary.Add(key, value);
                }

                return dictionary;
            }
        }

        public partial class ObjectNode
        {
            private struct FieldOrProperty
            {
                public FieldInfo Field;
                public PropertyInfo Property;
                public string Name;
                public Type Type;

                public bool IsValid
                {
                    get { return Field != null || Property != null; }
                }

                public void Set(object instance, object value)
                {
                    if (Field != null)
                        Field.SetValue(instance, value);
                    if (Property != null)
                        Property.SetValue(instance, value, null);
                }
            }

            public override object Deserialise(IKernel kernel, Type expectedType)
            {
                var type = Type ?? expectedType;
                var instance = CreateInstance(type, kernel);

                foreach (var item in Children)
                {
                    if (item.Value.GetNode() == null)
                        continue;

                    var field = FindField(type, item.Key);

                    if (!field.IsValid)
                        continue;

                    var value = DeserialiseNode(item.Value.GetNode(), kernel, field.Type);
                    field.Set(instance, value);
                }

                return instance;
            }

            private FieldOrProperty FindField(Type type, string name)
            {
                FieldOrProperty f = new FieldOrProperty();
                f.Name = name;

                var field = type.GetField(name);
                if (field != null)
                {
                    f.Field = field;
                    f.Property = null;
                    f.Type = field.FieldType;
                }
                else
                {
                    var property = type.GetProperty(name);
                    if (property != null)
                    {
                        f.Property = property;
                        f.Field = null;
                        f.Type = property.PropertyType;
                    }
                }

                return f;
            }
        }

        /// <summary>
        /// Deserialises the object described by this DOM instance into the type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of object to instantiate.</typeparam>
        /// <returns>The runtime object described by this DOM.</returns>
        public T Deserialise<T>()
        {
            return (T)DeserialiseNode(Root.GetNode(), Kernel, typeof(T));
        }

        private static object DeserialiseNode(Node node, IKernel kernel, Type expectedType)
        {
            if (typeof(ICustomSerialisable).IsAssignableFrom(node.Type))
            {
                var type = node.Type ?? expectedType;
                var instance = CreateInstance(type, kernel) as ICustomSerialisable;
                instance.Deserialise(node);
                return instance;
            }
            else
                return node.Deserialise(kernel, expectedType);
        }

        private static object CreateInstance(Type type, IKernel kernel)
        {
            if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                return kernel.Get(type);
        }
    }
}
