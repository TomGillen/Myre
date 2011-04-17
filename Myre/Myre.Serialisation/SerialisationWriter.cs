using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using Myre.Serialisation;

namespace Myre.Serialisation
{
    public class SerialisationWriter
    {
        private static readonly HashSet<Type> numbers = new HashSet<Type>()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };

        private Stream stream;
        private Dictionary<Type, bool> parsable;
        private Dictionary<Type, Action<SerialisationWriter, object>> customSerialisers;

        public SerialisationWriter(Stream stream, Dictionary<Type, Action<SerialisationWriter, object>> customSerialisers)
        {
            this.stream = stream;
            this.parsable = new Dictionary<Type, bool>();
            this.customSerialisers = customSerialisers;
        }

        private void Write(object value)
        {
            var encoded = Encoding.ASCII.GetBytes(value.ToString());
            stream.Write(encoded, 0, encoded.Length);
        }

        public void WriteLiteral(object value)
        {
            var type = value.GetType();

            if (IsNumber(type))
                Write(value);
            else
                Write("\"" + value.ToString().Escape() + "\"");
        }

        public void WriteKeyValue(string key, object value, Type fieldType)
        {
            Write(key);
            Write(": ");
            WriteObject(value, fieldType);
            Write(",");
        }

        public void StartCompositeObject()
        {
            Write("{");
        }

        public void EndCompositeObject()
        {
            Write("}");
        }
        
        public void WriteObject(object value, Type expectedType)
        {
            var type = value.GetType();
            if (type != expectedType)
            {
                Write(type.Name);
                Write(" ");
            }

            Action<SerialisationWriter, object> serialiser;
            if (customSerialisers.TryGetValue(type, out serialiser))
                serialiser(this, value);

            //else if (value is ICustomSerialisable)
            //    (value as ICustomSerialisable).Serialise(this);

            else if (IsParsable(type))
                WriteLiteral(value);

            else
            {
                StartCompositeObject();

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.CanRead && property.CanWrite)
                        WriteKeyValue(property.Name, property.GetValue(value, null), property.PropertyType);
                }

                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    WriteKeyValue(field.Name, field.GetValue(value), field.FieldType);

                EndCompositeObject();
            }
        }

        private bool IsParsable(Type type)
        {
            bool isParsable;
            if (!parsable.TryGetValue(type, out isParsable))
            {
                var parseMethod = type.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string) }, null);
                isParsable = parseMethod != null && type.IsAssignableFrom(parseMethod.ReturnType);

                parsable[type] = isParsable;
            }

            return isParsable;
        }

        private bool IsNumber(Type type)
        {
            return numbers.Contains(type);
        }
    }
}
