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
    /// Specifies a growth direction
    /// </summary>
    public enum GrowthDirection
    {
        /// <summary>
        /// Up.
        /// </summary>
        Up,
        /// <summary>
        /// Down.
        /// </summary>
        Down
    }

    /// <summary>
    /// A text log control.
    /// </summary>
    public class TextLog
        : Control
    {
        List<StringPart> text;
        SpriteFont font;
        bool moveNextDrawToNewLine;
        int historyCapacity;
        int startIndex;

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
                if (font != value)
                {
                    font = value;
                    //Recalculate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the font colour.
        /// </summary>
        public Color Colour
        {
            get;
            set;
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
        /// Gets or sets the scale.
        /// </summary>
        /// <value>The scale.</value>
        public Vector2 Scale
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the direction text will grow in.
        /// </summary>
        /// <value>The direction.</value>
        public GrowthDirection Direction
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextLog"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="font">The font to use to draw the text contained by this log.</param>
        public TextLog(Control parent, SpriteFont font, int historyCapacity)
            : base(parent)
        {
            this.historyCapacity = historyCapacity;
            text = new List<StringPart>();
            Font = font;
            Colour = Color.White;
            Scale = Vector2.One;
            Direction = GrowthDirection.Up;

            //Action<Frame> recalc = delegate(Frame c)
            //{
            //    Recalculate();
            //};

            //AreaChanged += recalc;
        }

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="line">The line.</param>
        public void WriteLine(StringPart line)
        {
            //var t = text[text.Count - 1];
            //text.RemoveAt(text.Count - 1);
            //t.Batch.Clear();
            //t.Batch.Write(line);
            //t.Height = t.Batch.CalculateArea(Int2D.Zero, Justification, Area.Width).Height;
            //text.Insert(0, t);
            Write(line.ToString());
            moveNextDrawToNewLine = true;
        }

        /// <summary>
        /// Appends text onto the last line written.
        /// </summary>
        /// <param name="text">The text to append.</param>
        public void Write(StringPart line)
        {
            if (moveNextDrawToNewLine || text.Count == 0)
            {
                text.Add(line);
                moveNextDrawToNewLine = false;
            }
            else
            {
                var current = text[text.Count - 1];
                text[text.Count - 1] = current.ToString() + line.ToString();
            }

            //if (line[line.Length - 1] == '\n')
            //    moveNextDrawToNewLine = true;

            text.RemoveRange(0, Math.Max(0, text.Count - historyCapacity));

            if (startIndex == text.Count - 1)
                ScrollToNewest();
        }

        /// <summary>
        /// Clears all text from this log
        /// </summary>
        public void Clear()
        {
            //for (int i = 0; i < text.Count; i++)
            //{
            //    text[i].Batch.Clear();
            //}
            text.Clear();
        }

        /// <summary>
        /// Draws the control and its' children.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            var heightOffset = 0f;

            for (int i = startIndex - 1; i >= 0; i--)
            {
                var line = text[i];
                var size = font.MeasureParsedString(line, Scale, Area.Width);

                var position = new Vector2(Area.X, Area.Y + (Direction == GrowthDirection.Down ? heightOffset : Area.Height - heightOffset - size.Y));
                batch.DrawParsedString(Font, line, position, Colour, 0, Vector2.Zero, Scale, Area.Width, Justification);

                heightOffset += size.Y;
                if (heightOffset > Area.Height)
                    break;
            }

            base.Draw(batch);
        }

        /// <summary>
        /// Scrolls towards older messages.
        /// </summary>
        public void ScrollBackward()
        {
            startIndex = Math.Max(0, startIndex - 1);
        }

        /// <summary>
        /// Scrolls towards newer messages.
        /// </summary>
        public void ScrollForward()
        {
            startIndex = Math.Min(text.Count, startIndex + 1);
        }

        /// <summary>
        /// Scroll to the most recent message.
        /// </summary>
        public void ScrollToNewest()
        {
            startIndex = text.Count;
        }
    }
}
