using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Myre.Serialisation
{
    public partial class Dom
    {
        #region nodes
        public partial class Node
        {
            public virtual void Write(Stream stream, int indent)
            {
                string name = GetTypeName(Type);

                Dom.Write(stream, name);
                Dom.Write(stream, " ");
            }

            private static string GetTypeName(Type type)
            {
                string name = type.Name;
                if (type.IsGenericType)
                {
                    var sb = new StringBuilder();
                    sb.Append(name, 0, name.LastIndexOf('`'));

                    sb.Append('<');

                    var args = type.GetGenericArguments();
                    for (int i = 0; i < args.Length; i++)
                    {
                        sb.Append(GetTypeName(args[i]));

                        if (i < args.Length - 1)
                            sb.Append(',');
                    }

                    sb.Append('>');

                    name = sb.ToString();
                }

                return name;
            }
        }

        public partial class ListNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                base.Write(stream, indent);

                Dom.Write(stream, "[");
                Dom.NewLine(stream, indent + 1);

                for (int i = 0; i < Children.Count; i++)
                {
                    var item = Children[i];
                    item.Write(stream, indent + 1);

                    if (i < Children.Count - 1)
                    {
                        Dom.Write(stream, ",");
                        Dom.NewLine(stream, indent + 1);
                    }
                    else
                    {
                        Dom.NewLine(stream, indent);
                    }
                }

                Dom.Write(stream, "]");
            }
        }

        public partial class DictionaryNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                base.Write(stream, indent);

                Dom.Write(stream, "[");
                Dom.NewLine(stream, indent + 1);

                foreach (var item in Children.Select((kvp, i) => new { Key = kvp.Key, Value = kvp.Value, index = i }))
                {                        
                    item.Key.Write(stream, indent + 1);
                    Dom.Write(stream, ": ");

                    if (item.Value == null)
                        Dom.Write(stream, "null");
                    else
                        item.Value.Write(stream, indent + 1);

                    if (item.index < Children.Count - 1)
                    {
                        Dom.Write(stream, ",");
                        Dom.NewLine(stream, indent + 1);
                    }
                    else
                    {
                        Dom.NewLine(stream, indent);
                    }
                }

                Dom.Write(stream, "]");
            }
        }

        public partial class LiteralNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                base.Write(stream, indent);
                Dom.Write(stream, Value);
            }
        }

        public partial class ObjectNode
            : Node
        {
            public override void Write(Stream stream, int indent)
            {
                base.Write(stream, indent);

                Dom.Write(stream, "{");
                Dom.NewLine(stream, indent + 1);

                foreach (var item in Children.Select((kvp, i) => new { Key = kvp.Key, Value = kvp.Value, index = i }))
                {
                    Dom.Write(stream, item.Key);
                    Dom.Write(stream, ": ");

                    if (item.Value == null)
                        Dom.Write(stream, "null");
                    else
                        item.Value.Write(stream, indent + 1);

                    if (item.index < Children.Count - 1)
                    {
                        Dom.Write(stream, ",");
                        Dom.NewLine(stream, indent + 1);
                    }
                    else
                    {
                        Dom.NewLine(stream, indent);
                    }
                }

                Dom.Write(stream, "}");
            }
        }
        #endregion

        private static byte[] buffer = new byte[100];

        public string Save()
        {
            var stream = new MemoryStream();
            Save(stream);
            stream.Flush();
            stream.Position = 0;

            var reader = new StreamReader(stream, Encoding.Unicode);
            return reader.ReadToEnd();
        }

        public void Save(Stream stream)
        {
            Root.Write(stream, 0);
        }

        private static void Write(Stream stream, string value)
        {
            var length = Encoding.Unicode.GetByteCount(value);
            if (buffer.Length < length)
                buffer = new byte[length];

            Encoding.Unicode.GetBytes(value, 0, value.Length, buffer, 0);
            stream.Write(buffer, 0, length);
        }

        private static void NewLine(Stream stream, int indent)
        {
            Dom.Write(stream, "\n");
            for (int i = 0; i < indent; i++)
			    Dom.Write(stream, "  ");
        }
    }
}
