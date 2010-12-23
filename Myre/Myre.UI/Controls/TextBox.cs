using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Globalization;
using Myre.UI.Text;
using Myre.Extensions;
using Microsoft.Xna.Framework.GamerServices;
using Myre.UI.InputDevices;
using Myre.UI.Gestures;

namespace Myre.UI.Controls
{
    /// <summary>
    /// A text box control.
    /// </summary>
    public class TextBox
        : Control
    {
        private string textString;
        private StringBuilder text;
        private bool typing;
        private string title;
        private string description;

        private StringBuilder drawBuffer;
        private SpriteFont font;
        private Color colour;
        private int drawStartIndex;
        private int drawEndIndex;

        private Pulser blink;
        private Pulser keyRepeat;
        private int selectionStartIndex;
        private int selectionEndIndex;
        private int selectionStartDrawPosition;
        private int selectionEndDrawPosition;
        private StringBuilder measurementBuffer;

        private bool dirty;
        private Texture2D blank;

#if WINDOWS
        private KeyboardState currentState;
        private KeyboardState previousState;
#endif

        public string Text
        {
            get { return textString; }
            set
            {
                if (value == null)
                    throw new ArgumentException("value");

                text.Clear();
                text.Append(value);
                textString = value;
                selectionStartIndex = value.Length;
                selectionEndIndex = value.Length;
                Dirty();
            }
        }

        public bool TextFitsInSpace
        {
            get;
            private set;
        }

        public List<char> IgnoredCharacters
        {
            get;
            set;
        }

        public TextBox(Control parent, Game game, SpriteFont font, string title, string description)
            : base(parent)
        {
            this.textString = string.Empty;
            this.text = new StringBuilder();
            this.textString = "";
            this.typing = false;
            this.title = title;
            this.description = description;

            this.drawBuffer = new StringBuilder();
            this.font = font;
            this.colour = Color.White;
            this.drawStartIndex = 0;
            this.drawEndIndex = 0;

            this.blink = new Pulser(PulserType.SquareWave, TimeSpan.FromSeconds(0.5));
            this.keyRepeat = new Pulser(PulserType.Simple, TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.5));
            this.selectionStartIndex = 0;
            this.selectionEndIndex = 0;
            this.measurementBuffer = new StringBuilder();

            this.blank = new Texture2D(game.GraphicsDevice, 1, 1);
            this.blank.SetData(new Color[] { Color.White });

            this.IgnoredCharacters = new List<char>();

            base.SetSize(100, font.LineSpacing);

            Gestures.Bind((g, t, i) =>
                {
                    i.Owner.Focus(this);
                    BeginTyping(i.Owner.ID < 4 ? (PlayerIndex)i.Owner.ID : PlayerIndex.One);
                },
                new ButtonPressed(Buttons.A),
                new MousePressed(MouseButtons.Left));

            Gestures.Bind((g, t, i) => 
                {
                    var keyboard = i as KeyboardDevice;
                    foreach (var character in keyboard.Characters)
                        Write(character.ToString());
                },
                new CharactersEntered());

            Gestures.Bind((g, t, i) => Copy(),
                new KeyCombinationPressed(Keys.C, Keys.LeftControl),
                new KeyCombinationPressed(Keys.C, Keys.RightControl));

            Gestures.Bind((g, t, i) => Cut(),
                new KeyCombinationPressed(Keys.X, Keys.LeftControl),
                new KeyCombinationPressed(Keys.X, Keys.RightControl));

            Gestures.Bind((g, t, i) => Paste(),
                new KeyCombinationPressed(Keys.V, Keys.LeftControl),
                new KeyCombinationPressed(Keys.V, Keys.RightControl));

            FocusedChanged += control => { if (!IsFocused) typing = false; };


        }

        public void BeginTyping(PlayerIndex player)
        {
            typing = true;
#if !WINDOWS
            Guide.BeginShowKeyboardInput(player, title, description, text.ToString(), GuideCallback, null);
#endif
        }

#if !WINDOWS
        private void GuideCallback(IAsyncResult result)
        {
            string input = Guide.EndShowKeyboardInput(result);
            Text = input;

            selectionStartIndex = selectionEndIndex = 0;
            typing = false;
        }
#endif

        public override void Update(GameTime gameTime)
        {
            if (typing)
            {
                blink.Update();
                keyRepeat.Update();

                ReadNavigationKeys();
            }

            if (dirty)
            {
                UpdatePositions();
                textString = text.ToString();
            }
        }

        private void Copy()
        {
            Clipboard.Text = Text.Substring(Math.Min(selectionStartIndex, selectionEndIndex), Math.Abs(selectionEndIndex - selectionStartIndex));
        }

        private void Paste()
        {
            var clipboard = Clipboard.Text;
            if (!string.IsNullOrEmpty(clipboard))
                Write(clipboard);
        }

        private void Cut()
        {
            Copy();
            Write("");
        }

        private void ReadNavigationKeys()
        {
#if WINDOWS
            previousState = currentState;
            currentState = Keyboard.GetState();

            bool shift = currentState.IsKeyDown(Keys.LeftShift) || currentState.IsKeyDown(Keys.RightShift);

            if (NewOrRepeat(Keys.Left))
                Left(shift);
            if (NewOrRepeat(Keys.Right))
                Right(shift);
            if (NewOrRepeat(Keys.Home))
                Home(shift);
            if (NewOrRepeat(Keys.End))
                End(shift);
            if (NewOrRepeat(Keys.Delete))
                Delete();
            if (NewOrRepeat(Keys.Back))
                Backspace();
#endif
        }

#if WINDOWS
        private bool NewOrRepeat(Keys key)
        {
            if (!currentState.IsKeyDown(key))
                return false;

            if (!previousState.IsKeyDown(key))
            {
                keyRepeat.Restart(false, true);
                return true;
            }

            return keyRepeat.IsSignalled;
        }
#endif

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsVisible)
                return;

            spriteBatch.DrawString(font, drawBuffer, new Vector2(Area.X, Area.Y), colour);

            if (typing)
            {
                spriteBatch.Draw(
                    blank,
                    new Rectangle(
                        Area.X + Math.Min(selectionStartDrawPosition, selectionEndDrawPosition), Area.Y,
                        Math.Abs(selectionEndDrawPosition - selectionStartDrawPosition), font.LineSpacing),
                    new Color(0.5f, 0.5f, 0.5f, 0.5f));

                if (blink.IsSignalled)
                {
                    spriteBatch.Draw(
                        blank,
                        new Rectangle(Area.X + selectionEndDrawPosition, Area.Y, 1, font.LineSpacing),
                        Color.White);
                }
            }
        }

        private void UpdatePositions()
        {
            drawStartIndex = Math.Min(drawStartIndex, selectionEndIndex);

            bool success = true;
            do
            {
                drawBuffer.Clear();
                drawEndIndex = drawStartIndex;

                bool fits;
                while (true)
                {
                    fits = font.MeasureString(drawBuffer).X <= Area.Width;
                    if (!fits)
                    {
                        drawBuffer.Remove(drawBuffer.Length - 1, 1);
                        drawEndIndex--;
                        break;
                    }
                    else
                    {
                        if (drawEndIndex >= text.Length)
                            break;

                        drawBuffer.Append(text[drawEndIndex]);
                        drawEndIndex++;
                    }
                }

                success = drawEndIndex >= selectionEndIndex;

                if (!success)
                    drawStartIndex++;

            } while (!success);

            TextFitsInSpace = font.MeasureString(text).X <= Area.Width;

            selectionStartDrawPosition = (int)MathHelper.Clamp(MeasureString(text, drawStartIndex, selectionStartIndex - drawStartIndex).X, 0, Area.Width - 1);
            selectionEndDrawPosition = (int)MathHelper.Clamp(MeasureString(text, drawStartIndex, selectionEndIndex - drawStartIndex).X, 0, Area.Width - 1);

            dirty = false;
        }

        private Vector2 MeasureString(StringBuilder sb, int startIndex, int length)
        {
            measurementBuffer.Clear();
            measurementBuffer.Append(sb, startIndex, length);
            return font.MeasureString(measurementBuffer);
        }

        private void Write(string characters)
        {
            var selectStart = Math.Min(selectionStartIndex, selectionEndIndex);
            var selectEnd = Math.Max(selectionStartIndex, selectionEndIndex);

            text.Remove(selectStart, selectEnd - selectStart);

            int charactersAdded = 0;
            for (int i = characters.Length - 1; i >= 0; i--)
            {
                var c = characters[i];
                if ((font.DefaultCharacter != null || font.Characters.Contains(c))
                    && !IgnoredCharacters.Contains(c))
                {
                    text.Insert(selectStart, c.ToString());
                    charactersAdded++;
                }
            }

            selectionEndIndex = selectStart + charactersAdded;
            selectionStartIndex = selectionEndIndex;

            Dirty();
        }

        private void Dirty()
        {
            blink.Restart(true, true);
            dirty = true;
        }

        private void Delete()
        {
            if (selectionEndIndex != selectionStartIndex)
                Write(string.Empty);
            else
            {
                if (selectionEndIndex < text.Length)
                {
                    text.Remove(selectionEndIndex, 1);
                    Dirty();
                }
            }
        }

        private void Backspace()
        {
            if (selectionEndIndex != selectionStartIndex)
                Write(string.Empty);
            else
            {
                Left(false);
                Delete();
            }
        }

        private void Left(bool shift)
        {
            selectionEndIndex--;
            CompleteMove(shift);
        }

        private void Right(bool shift)
        {
            selectionEndIndex++;
            CompleteMove(shift);
        }

        private void Home(bool shift)
        {
            selectionEndIndex = 0;
            CompleteMove(shift);
        }

        private void End(bool shift)
        {
            selectionEndIndex = text.Length;
            CompleteMove(shift);
        }

        private void CompleteMove(bool shift)
        {
            selectionEndIndex = (int)MathHelper.Clamp(selectionEndIndex, 0, text.Length);
            if (!shift)
                selectionStartIndex = selectionEndIndex;
            Dirty();
        }
    }
}
