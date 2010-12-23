using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;

namespace Myre.UI.Text
{
    public class ContentParser<T>
    {
        private ContentManager content;
        private Dictionary<StringPart, T> items;

        public Dictionary<StringPart, T> Items
        {
            get { return items; }
        }

        public ContentParser(Game game, string contentDirectory)
            : this(new ContentManager(game.Services, contentDirectory))
        {
        }

        public ContentParser(ContentManager content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            this.content = content;
            this.items = new Dictionary<StringPart, T>();
        }

        public bool TryParse(StringPart name, out T item)
        {
            if (items.TryGetValue(name, out item))
                return true;

            if (TryLoad(name, out item))
                return true;

            return false;
        }

        private bool TryLoad(StringPart name, out T item)
        {
            try
            {
                item = content.Load<T>(name.ToString());
                items.Add(name, item);
                return true;
            }
            catch
            {
                item = default(T);
                return false;
            }
        }
    }
}
