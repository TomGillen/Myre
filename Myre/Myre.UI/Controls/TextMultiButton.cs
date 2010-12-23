using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myre.UI.Text;
using Myre.UI.Gestures;
using Myre.UI.InputDevices;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A text multibutton.
    /// </summary>
    public class TextMultiButton
        : MultiButton
    {
        private string[] options;
        private int arrowSize;
        private Label leftArrow, rightArrow;
        private Label centreText;

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public StringPart Text
        {
            get { return centreText.Text; }
            set { centreText.Text = value; }
        }

        /// <summary>
        /// Gets or sets the justification.
        /// </summary>
        public override Justification Justification
        {
            get { return centreText.Justification; }
            set { centreText.Justification = value; }
        }

        /// <summary>
        /// Gets the option at the specified index.
        /// </summary>
        /// <value></value>
        public string this[int i]
        {
            get { return options[i]; }
        }

        /// <summary>
        /// Gets or sets the font colour.
        /// </summary>
        public Color Colour { get; set; }

        /// <summary>
        /// Gets or sets the highlight font colour.
        /// </summary>
        public Color Highlight { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextMultiButton"/> class.
        /// </summary>
        /// <param name="parent">This controls parent control.</param>
        /// <param name="font">The font.</param>
        /// <param name="options">The options.</param>
        /// <param name="focusScope">The focus scope.</param>
        public TextMultiButton(Control parent, SpriteFont font, string[] options)
            : base(parent)
        {
            if (options == null)
                throw new ArgumentNullException("options");
            if (options.Length == 0)
                throw new ArgumentException("There must be at least one option.", "options");

            this.Colour = Color.White;
            this.Highlight = Color.CornflowerBlue;
            this.options = options;
            this.OptionsCount = options.Length;

            this.leftArrow = new Label(this, font);
            this.leftArrow.Text = "<";
            this.leftArrow.SetPoint(Points.TopLeft, 0, 0);
            this.rightArrow = new Label(this, font);
            this.rightArrow.Text = ">";
            this.rightArrow.SetPoint(Points.TopRight, 0, 0);

            this.centreText = new Label(this, font);
            this.centreText.Justification = Justification.Centre;
            this.centreText.SetPoint(Points.TopLeft, leftArrow.Area.Width, 0);
            this.centreText.SetPoint(Points.TopRight, -rightArrow.Area.Width, 0);
            this.centreText.Text = options[0];

            ControlEventHandler recalcSize = delegate(Control c)
            {
                Vector2 maxSize = Vector2.Zero;
                for (int i = 0; i < options.Length; i++)
                {
                    var size = font.MeasureString(options[i]);
                    maxSize.X = Math.Max(maxSize.X, size.X);
                    maxSize.Y = Math.Max(maxSize.Y, size.Y);
                }
                arrowSize = (int)font.MeasureString("<").X;
                maxSize.X += arrowSize * 2;
                SetSize((int)maxSize.X, (int)maxSize.Y);
                leftArrow.SetSize(arrowSize, font.LineSpacing);
                rightArrow.SetSize(arrowSize, font.LineSpacing);
            };

            ControlEventHandler highlight = delegate(Control c)
            {
                (c as Label).Colour = (c.IsFocused || c.IsWarm) ? Highlight : Colour;
            };

            leftArrow.WarmChanged += highlight;
            rightArrow.WarmChanged += highlight;
            recalcSize(this);

            SelectionChanged += delegate(Control c)
            {
                centreText.Text = this[SelectedOption];
            };

            BindGestures();
        }

        /// <summary>
        /// Binds the left and right buttons to next and previous.
        /// </summary>
        private void BindGestures()
        {
            leftArrow.Gestures.Bind(PreviousOption,
                new MouseReleased(MouseButtons.Left));

            rightArrow.Gestures.Bind(NextOption,
                new MouseReleased(MouseButtons.Left));
        }

        private void NextOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            SelectedOption++;
        }

        private void PreviousOption(IGesture gesture, GameTime time, IInputDevice device)
        {
            SelectedOption--;
        }

        /// <summary>
        /// Draws the control and its' children.
        /// </summary>
        /// <param name="batch">An spritebactch already started for alpha blending with deferred sort mode.</param>
        public override void Draw(SpriteBatch batch)
        {
            centreText.Colour = IsFocused ? Highlight : Colour;
        }
    }
}
