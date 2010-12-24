using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Ninject;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Myre.Entities;

namespace Myre.Entities.Serialisation
{
    /// <summary>
    /// A class which provides serialisation and deserialisation between EntityDescription objects and xml.
    /// </summary>
    public class EntityXmlParser
    {
        private IKernel kernel;
        //private XmlSerializerNamespaces emptyNamespaces;

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityXmlParser"/> class.
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        public EntityXmlParser(IKernel kernel)
        {
            this.kernel = kernel;

            //emptyNamespaces = new XmlSerializerNamespaces();
            //emptyNamespaces.Add("", "");
        }

        /// <summary>
        /// Serialises the specified entity description.
        /// </summary>
        /// <param name="writer">The xml writer to which the xml is written.</param>
        /// <param name="description">The description to serialise.</param>
        public void Serialise(XmlWriter writer, EntityDescription description)
        {
            writer.WriteStartElement("Entity");

            WriteProperties(writer, description);
            WriteBehaviours(writer, description);

            writer.WriteEndElement();
        }

        private void WriteProperties(XmlWriter writer, EntityDescription description)
        {
            if (description.Properties.Count == 0)
                return;

            writer.WriteStartElement("Properties");

            foreach (var item in description.Properties)
            {
                writer.WriteStartElement("Property");
                writer.WriteAttributeString("name", item.Name);
                writer.WriteAttributeString("type", item.DataType.AssemblyQualifiedName);
                writer.WriteAttributeString("copyBehaviour", item.CopyBehaviour.ToString());

                //if (item.Serialise)
                //{
                    if (item.Value != null)
                    {
                        //XmlSerializer serialiser = new XmlSerializer(item.DataType);
                        //serialiser.Serialize(writer, item.Data);//, emptyNamespaces);
                        throw new NotImplementedException();
                    }
                //}
                else
                    writer.WriteAttributeString("serialiseValue", "false");

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void WriteBehaviours(XmlWriter writer, EntityDescription description)
        {
            if (description.Behaviours.Count == 0)
                return;

            writer.WriteStartElement("Behaviours");

            foreach (var item in description.Behaviours)
            {
                writer.WriteStartElement("Behaviour");

                writer.WriteAttributeString("type", item.Type.AssemblyQualifiedName);

                if (item.Name != null)
                    writer.WriteAttributeString("name", item.Name);

                //if (item.Settings != null)
                //    item.Settings.WriteTo(writer);

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        /// <summary>
        /// Deserialises an entity description.
        /// </summary>
        /// <param name="reader">The xml reader containing the serialised entity description.</param>
        /// <returns>The deserialised entity description/</returns>
        public EntityDescription Deserialise(XmlReader reader)
        {
            EntityDescription entity = kernel.Get<EntityDescription>();

            reader.ReadStartElement("Entity");

            if (reader.IsStartElement("Properties"))
                ParseProperties(reader, entity);

            if (reader.IsStartElement("Behaviours"))
                ParseBehaviours(reader, entity);

            reader.ReadEndElement();

            return entity;
        }

        private void ParseProperties(XmlReader reader, EntityDescription entity)
        {
            reader.ReadStartElement("Properties");

            while (reader.IsStartElement("Property"))
            {
                var property = ParsePropertyAttributes(reader);
                var dataDefinition = reader.ReadInnerXml().Trim();
                if (!string.IsNullOrEmpty(dataDefinition))
                    property.Value = ParsePropertyValue(dataDefinition, property.DataType);

                entity.AddProperty(property);
            }

            reader.ReadEndElement();
        }

        private PropertyData ParsePropertyAttributes(XmlReader reader)
        {
            PropertyData data = new PropertyData();

            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                switch (reader.Name)
                {
                    case "name":
                        data.Name = reader.Value;
                        break;
                    case "type":
                        data.DataType = Type.GetType(reader.Value);
                        break;
                    case "copyBehaviour":
                        data.CopyBehaviour = (PropertyCopyBehaviour)Enum.Parse(typeof(PropertyCopyBehaviour), reader.Value, true);
                        break;
                    //case "serialiseValue":
                    //    data.Serialise = bool.Parse(reader.Value);
                    //    break;
                    default:
                        break;
                }
            }

            reader.MoveToElement();
            return data;
        }

        private object ParsePropertyValue(string xml, Type type)
        {
            using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(xml)))
            {
                //XmlSerializer serialiser = new XmlSerializer(type);
                //return serialiser.Deserialize(stream);
                throw new NotImplementedException();
            }
        }

        private void ParseBehaviours(XmlReader reader, EntityDescription entity)
        {
            reader.ReadStartElement("Behaviours");

            while (reader.IsStartElement("Behaviour"))
            {
                var behaviour = ParseBehaviour(reader);
                entity.AddBehaviour(behaviour);
            }

            reader.ReadEndElement();
        }

        private BehaviourData ParseBehaviour(XmlReader reader)
        {
            BehaviourData data = new BehaviourData();

            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                switch (reader.Name)
                {
                    case "name":
                        data.Name = reader.Value;
                        break;
                    case "type":
                        data.Type = Type.GetType(reader.Value);
                        break;
                    default:
                        break;
                }
            }

            reader.MoveToElement();
            var settings = reader.ReadInnerXml().Trim();
            //if (!string.IsNullOrEmpty(settings))
            //    data.Settings = XElement.Parse(settings);

            return data;
        }
    }
}
