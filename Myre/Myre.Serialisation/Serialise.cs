using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using Myre.Serialisation;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        struct FieldsAndProperties
        {
            public PropertyInfo[] Properties;
            public FieldInfo[] Fields;
        }

        private static Dictionary<Type, bool> parsable = new Dictionary<Type, bool>();
        private static Dictionary<Type, FieldsAndProperties> properties = new Dictionary<Type, FieldsAndProperties>();

        public static Dom Serialise<T>(T value)
        {
            var root = SerialiseObject(value);
            return new Dom(root);
        }

        private static Node SerialiseObject(object value)
        {
            if (value == null)
                return null;

            if (value is ICustomSerialisable)
                return (value as ICustomSerialisable).Serialise();

            var type = value.GetType();

            if (IsParsable(type))
                return CreateLiteral(value);

            if (value is IList)
                return CreateList(value as IList, type);

            if (value is IDictionary)
                return CreateDictionary(value as IDictionary, type);

            return CreateObject(value, type);
        }

        private static Node CreateLiteral(object value)
        {
            return new LiteralNode()
            {
                Type = value.GetType(),
                Value = string.Format("\"{0}\"", value.ToString().Escape())
            };
        }

        private static Node CreateList(IList list, Type type)
        {
            var genericParameters = (from i in type.GetInterfaces()
                                     where i.FullName.StartsWith(typeof(IList<>).FullName)
                                     select i.GetGenericArguments().First()).ToArray();

            var elementType = genericParameters.Length == 1 ? genericParameters[0] : typeof(object);
            
            var node = new ListNode();
            node.Type = type;
            node.ElementType = elementType;
            node.Children = new List<Node>();

            foreach (var item in list)
            {
                var serialisedItem = SerialiseObject(item);
                node.Children.Add(serialisedItem);
            }

            return node;
        }

        private static Node CreateDictionary(IDictionary dictionary, Type type)
        {
            var genericParameters = (from i in type.GetInterfaces()
                                     where i.FullName.StartsWith(typeof(IDictionary<,>).FullName)
                                     let args = i.GetGenericArguments()
                                     select new { Key = args[0], Value = args[1] }).ToArray();

            var types = genericParameters.Length == 1 ? genericParameters[0] : new { Key = typeof(object), Value = typeof(object) };

            var node = new DictionaryNode();
            node.Type = type;
            node.KeyType = types.Key;
            node.ValueType = types.Value;
            node.Children = new Dictionary<Node, Node>();

            foreach (DictionaryEntry item in dictionary)
            {
                var serialisedKey = SerialiseObject(item.Key);
                var serialisedValue = SerialiseObject(item.Value);
                node.Children.Add(serialisedKey, serialisedValue);
            }

            return node;
        }

        private static Node CreateObject(object value, Type type)
        {
            var fap = GetFieldsAndProperties(type);
            var node = new ObjectNode();
            node.Type = type;
            node.Children = new Dictionary<string,Node>();

            foreach (var field in fap.Fields)
            {
                var fieldValue = field.GetValue(value);
                var serialisedValue = SerialiseObject(fieldValue);
                node.Children.Add(field.Name, serialisedValue);
            }

            foreach (var property in fap.Properties)
            {
                var fieldValue = property.GetValue(value, null);
                var serialisedValue = SerialiseObject(fieldValue);
                node.Children.Add(property.Name, serialisedValue);
            }

            return node;
        }

        private static bool IsParsable(Type type)
        {
            if (type == typeof(string))
                return true;

            bool isParsable;
            if (!parsable.TryGetValue(type, out isParsable))
            {
                var parseMethod = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                isParsable = parseMethod != null && type.IsAssignableFrom(parseMethod.ReturnType);

                parsable[type] = isParsable;
            }

            return isParsable;
        }

        private static FieldsAndProperties GetFieldsAndProperties(Type type)
        {
            FieldsAndProperties fap;
            if (properties.TryGetValue(type, out fap))
                return fap;

            fap.Fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            fap.Properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite).ToArray();

            properties[type] = fap;
            return fap;
        }
    }
}
