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
        public class Node
        {
            public Type Type;
        }

        public class ListNode
            : Node
        {
            public Type ElementType;
            public List<Node> Children;
        }

        public class DictionaryNode
            : Node
        {
            public Type KeyType;
            public Type ValueType;
            public Dictionary<Node, Node> Children;
        }

        public class LiteralNode
            : Node
        {
            public string Value;
        }

        public class ObjectNode
            : Node
        {
            public Dictionary<string, Node> Children;
        }
        #endregion

        public Node Root { get; private set; }

        public Dom(Node root)
        {
            this.Root = root;
        }
    }
}
