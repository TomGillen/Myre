using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using ProtoBuf;
using Myre.Extensions;

namespace Myre.Entities.Serialisation
{
    interface ISerialisationAdaptor
    {
        void Serialise(Stream stream, object instance);
        object Deserialise(Stream stream);
        object Merge(Stream stream, object instance);
    }

    class SerialisationAdaptor<T>
        : ISerialisationAdaptor
    {
        public void Serialise(Stream stream, object instance)
        {
            Serializer.Serialize<T>(stream, (T)instance);
        }

        public object Deserialise(Stream stream)
        {
            return Serializer.Deserialize<T>(stream);
        }

        public object Merge(Stream stream, object instance)
        {
            return Serializer.Merge<T>(stream, (T)instance);
        }
    }

    static class ProtobufNonGenericAdaptor
    {
        private static Dictionary<Type, ISerialisationAdaptor> adaptors = new Dictionary<Type, ISerialisationAdaptor>();
        private static Type genericType = Type.GetType("Myre.Entities.Serialisation.SerialisationAdaptor`1");

        public static void Serialise(Stream stream, object instance, Type type)
        {
            var adaptor = GetAdaptor(type);
            adaptor.Serialise(stream, instance);
        }

        public static object Deserialise(Stream stream, Type type)
        {
            var adaptor = GetAdaptor(type);
            return adaptor.Deserialise(stream);
        }

        public static object Merge(Stream stream, object instance, Type type)
        {
            var adaptor = GetAdaptor(type);
            return adaptor.Merge(stream, instance);
        }

        private static ISerialisationAdaptor GetAdaptor(Type type)
        {
            ISerialisationAdaptor adaptor;
            if (!adaptors.TryGetValue(type, out adaptor))
            {
                var adaptorType = genericType.MakeGenericType(type);
                adaptor = adaptorType.CreateInstance() as ISerialisationAdaptor;
                adaptors[type] = adaptor;
            }

            return adaptor;
        }
    }
}
