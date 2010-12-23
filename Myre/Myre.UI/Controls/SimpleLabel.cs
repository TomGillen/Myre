using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A simple label control for drawing text. Does not support glyphs or text wrapping.
    /// Use this for labels which change often.
    /// </summary>
    public class SimpleLabel
        : Control
    {
        private string text;
        private SpriteFont font;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        public string Text 
        {
            get { return text; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                if (!text.Equals(value))
                {
                    text = value;
                    SetSize((Int2D)font.MeasureString(text));
                }
            }
        }

        /// <summary>
        /// Gets or sets the font.
        /// </summary>
        public SpriteFont Font
        {
            get { return font; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value");
                font = value;
                SetSize(new Int2D(Area.Width, Font.LineSpacing));
            }
        }

        /// <summary>
        /// Gets or sets the colour.
        /// </summary>
        public Color Colour { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleLabel"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="font">The font.</param>
        public SimpleLabel(Control parent, SpriteFont font)
            : base(parent)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            this.text = "";
            this.Font = font;
            this.SetSize((Int2D)font.MeasureString(text));

            Action<Frame> recalculateSize = delegate(Frame c)
            {
                SetSize(new Int2D(Area.Width, Font.LineSpacing));
            };

            AreaChanged += recalculateSize;
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            batch.DrawString(Font, Text, new Vector2(Area.X, Area.Y), Colour);
        }
    }
}
