using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Myre.UI.Text;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A control which draws glyphed text.
    /// </summary>
    /// <remarks>
    /// Changing the text, font or width will cause many string allocations.
    /// The label automatically sets it's size to that of its text when the text, justification or font changes.
    /// </remarks>
    public class Label
        : Control
    {
        StringPart text;
        SpriteFont font;
        Vector2 scale;

        /// <summary>
        /// Gets the size of the text as printed by this label.
        /// </summary>
        public Vector2 TextSize { get; private set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public StringPart Text
        {
            get { return text; }
            set
            {
                if (!text.Equals(value))
                {
                    text = value;
                    UpdateSize();
                }
            }
        }

        /// <summary>
        /// Gets or sets the justification.
        /// </summary>
        /// <value>The justification.</value>
        public Justification Justification
        {
            get;
            set;
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
                UpdateSize();
            }
        }

        /// <summary>
        /// Gets or sets the default colour.
        /// </summary>
        public Color Colour
        {
            get;
            set;
        }

        public Vector2 Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    UpdateSize();
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Label"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="font">The font.</param>
        /// <param name="focusScope">The focus scope.</param>
        public Label(Control parent, SpriteFont font)
            : base(parent)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            this.font = font;
            Text = "";
            Colour = Color.White;
            Scale = Vector2.One;

            AreaChanged += c => UpdateSize();
        }

        private void UpdateSize()
        {
            TextSize = Font.MeasureParsedString(Text, Scale, IsWidthFixed ? Area.Width : 0);
            SetSize((int)TextSize.X, (int)TextSize.Y);
        }

        /// <summary>
        /// Draws the control.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            batch.DrawParsedString(Font, Text, new Vector2(Area.X, Area.Y), Colour, 0, Vector2.Zero, Scale, Area.Width, Justification);
        }
    }
}
