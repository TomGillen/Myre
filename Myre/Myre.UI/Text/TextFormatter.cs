// TODO: TextFormatter code is horrible. Tidy up sometime.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Myre.Extensions;

namespace Myre.UI.Text
{
    public static class TextFormatter
    {
        struct FontChange
        {
            public int Index;
            public SpriteFont Font;
        }

        enum TagType
        {
            None,
            FontStart,
            FontEnd,
            ColourStart,
            ColourEnd,
            NewLine
        }

        private static ContentParser<SpriteFont> fonts;
        private static Stack<SpriteFont> fontHistory;
        private static Stack<Color> colourHistory;
        private static StringBuilder buffer;
        private static StringBuilder input;
        private static List<FontChange> fontChanges;
        private static List<float> lineWidths;

        private static SpriteFont defaultFont;
        private static Color defaultColour;

        static TextFormatter()// TextFormatter(ContentManager fontContent)
        {
            //fonts = new ContentParser<SpriteFont>(fontContent);
            fontHistory = new Stack<SpriteFont>();
            colourHistory = new Stack<Color>();
            buffer = new StringBuilder();
            input = new StringBuilder();
            fontChanges = new List<FontChange>();
            lineWidths = new List<float>();
        }

        public static void SetFontSource(ContentManager fontContent)
        {
            fonts = new ContentParser<SpriteFont>(fontContent);
        }

        public static Vector2 MeasureParsedString(this SpriteFont font, StringPart text, Vector2 scale, int wrapWidth)
        {
            // bit of a hack..
            return DrawParsedString(null, font, text, Vector2.Zero, Color.White, 0, Vector2.Zero, Vector2.One, wrapWidth, Justification.Left, false);
        }

        public static Vector2 DrawParsedString(this SpriteBatch spriteBatch, SpriteFont font, StringPart text, Vector2 position, Color colour)
        {
            return DrawParsedString(spriteBatch, font, text, position, colour, 0, Vector2.Zero, Vector2.One, 0, Justification.Left);
        }

        public static Vector2 DrawParsedString(this SpriteBatch spriteBatch, SpriteFont font, StringPart text, Vector2 position, Color colour, float rotation, Vector2 origin, Vector2 scale, int wrapWidth, Justification justification)
        {
            if (wrapWidth <= 0)
                throw new ArgumentOutOfRangeException("wrapWidth", "wrapWidth must be greater than 0.");

            return DrawParsedString(spriteBatch, font, text, position, colour, rotation, origin, scale, wrapWidth, justification, true);
        }

        private static Vector2 DrawParsedString(this SpriteBatch spriteBatch, SpriteFont font, StringPart text, Vector2 position, Color colour, float rotation, Vector2 origin, Vector2 scale, int wrapWidth, Justification justification, bool drawingEnabled)
        {
            input.Clear();
            input.AppendPart(text);

            defaultFont = font;
            defaultColour = colour;

            lineWidths.Clear();
            if (wrapWidth > 0)
                Wrap(input, scale, wrapWidth);

            input.Replace("\n", "[\n]");

            colourHistory.Clear();
            fontHistory.Clear();

            var currentPositionOffset = Vector2.Zero;
            var lineSpacing = CurrentFont().LineSpacing;
            var width = 0f;

            int lineIndex = 0;
            Vector2 justificationOffset = new Vector2(Justify(lineWidths.Count > 0 ? lineWidths[0] : 0, wrapWidth, justification), 0);

            int i = 0;
            int tagStart = 0;
            int tagEnd = 0;
            Vector2 size = Vector2.Zero;
            while (i < input.Length
                && (tagStart = IndexOf(input, "[", i)) != -1
                && (tagEnd = IndexOf(input, "]", tagStart)) != -1)
            {
                if (tagStart > i)
                {
                    SetBuffer(new StringPart(input, i, tagStart - i));

                    if (drawingEnabled)
                        DrawBuffer(spriteBatch, ref position, rotation, ref origin, ref scale, currentPositionOffset + justificationOffset);

                    size = CurrentFont().MeasureString(buffer);
                    currentPositionOffset.X += size.X;
                    lineSpacing = Math.Max(lineSpacing, (int)size.Y);
                    width = Math.Max(width, currentPositionOffset.X);
                }

                i = tagStart;

                if (ParseTag(new StringPart(input, tagStart + 1, tagEnd - tagStart - 1), ref currentPositionOffset, ref lineSpacing, ref lineIndex) != TagType.None)
                {
                    i = tagEnd;

                    if (lineWidths.Count > lineIndex)
                        justificationOffset.X = Justify(lineWidths[lineIndex], wrapWidth, justification);
                }
                else
                {
                    SetBuffer(new StringPart(input, i, 1));

                    if (drawingEnabled)
                        DrawBuffer(spriteBatch, ref position, rotation, ref origin, ref scale, currentPositionOffset + justificationOffset);

                    size = CurrentFont().MeasureString(buffer);
                    currentPositionOffset.X += size.X;
                    lineSpacing = Math.Max(lineSpacing, (int)size.Y);
                    width = Math.Max(width, currentPositionOffset.X);
                }

                i++;
            }

            if (i != input.Length)
            {                
                SetBuffer(new StringPart(input, i, input.Length - i));
                
                if (drawingEnabled)
                    DrawBuffer(spriteBatch, ref position, rotation, ref origin, ref scale, currentPositionOffset + justificationOffset);

                size = CurrentFont().MeasureString(buffer);
                currentPositionOffset.X += size.X;
                lineSpacing = Math.Max(lineSpacing, (int)size.Y);
                width = Math.Max(width, currentPositionOffset.X);
            }

            return new Vector2(width, currentPositionOffset.Y + lineSpacing) * scale;
        }

        private static void SetBuffer(StringPart text)
        {
            buffer.Clear();
            buffer.AppendPart(text);
        }

        private static void DrawBuffer(SpriteBatch spriteBatch, ref Vector2 position, float rotation, ref Vector2 origin, ref Vector2 scale, Vector2 currentPositionOffset)
        {
            spriteBatch.DrawString(
                CurrentFont(),
                buffer,
                position,
                CurrentColour(),
                rotation,
                origin - currentPositionOffset,
                scale,
                SpriteEffects.None,
                0);
        }

        private static float Justify(float lineWidth, int wrapWidth, Justification justification)
        {
            switch (justification)
            {
                case Justification.Centre:
                    return (wrapWidth - lineWidth) / 2f;
                case Justification.Right:
                    return (wrapWidth - lineWidth);
                default:
                    return 0;
            }
        }

        private static int IndexOf(StringBuilder text, string value, int index)
        {
            int c = 0;
            for (int i = index; i < text.Length; i++)
            {
                if (text[i] == value[c])
                    c++;
                else
                    c = 0;

                if (c == value.Length)
                    return i;
            }

            return -1;
        }

        private static void Wrap(StringBuilder input, Vector2 scale, float allowedWidth)
        {
            RecordFontChanges(input);

            float lineWidth = 0;
            float wordWidth = 0;
            int fontIndex = 0;
            int wordStart = 0;
            float spaceSize = 0;
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '[')
                {
                    var closeBracket = IndexOf(input, "]", i);
                    if (closeBracket != -1)
                    {
                        Vector2 currentPositionOffset = Vector2.Zero;
                        int a = 0, b = 0;
                        if (ParseTag(new StringPart(input, i + 1, closeBracket - i - 1), ref currentPositionOffset, ref a, ref b) != TagType.None)
                        {
                            i = closeBracket;
                            continue;
                        }
                    }
                }

                if (input[i] == '\n')
                {
                    lineWidths.Add((lineWidth + wordWidth) / scale.X);

                    lineWidth = 0;
                    wordWidth = 0;
                    wordStart = i + 1;
                    continue;
                }

                SpriteFont font = defaultFont;
                while (fontIndex < fontChanges.Count && fontChanges[fontIndex].Index <= i)
                {
                    font = fontChanges[fontIndex].Font;
                    fontIndex++;
                }
                
                if (input[i] == ' ')
                {
                    wordStart = i + 1;
                    lineWidth += wordWidth;
                    spaceSize = font.MeasureString(" ").X * scale.X;
                    lineWidth += spaceSize;
                    wordWidth = 0;
                    continue;
                }

                buffer.Clear();
                buffer.AppendPart(new StringPart(input, i, 1));
                var size = font.MeasureString(buffer) * scale;
                wordWidth += size.X;

                if (wordWidth > allowedWidth)
                {
                    lineWidths.Add((wordWidth - size.X) / scale.X);

                    input.Insert(i, "\n");
                    //i++;

                    lineWidth = 0;
                    wordWidth = 0;
                }
                else if (lineWidth + wordWidth > allowedWidth)
                {
                    if (lineWidth > 0)
                        lineWidth -= spaceSize;
                    lineWidths.Add(lineWidth / scale.X);

                    input.Insert(wordStart, "\n");
                    i++;

                    lineWidth = 0;
                }
            }

            lineWidths.Add(lineWidth + wordWidth);
        }

        private static void RecordFontChanges(StringBuilder input)
        {
            fontChanges.Clear();

            int tagStart = 0;
            int tagEnd = 0;
            while ((tagStart = IndexOf(input, "[", tagEnd)) != -1
                && (tagEnd = IndexOf(input, "]", tagStart)) != -1)
            {
                var tag = new StringPart(input, tagStart + 1, tagEnd - tagStart - 1);

                var fontChanged = false;
                if (tag.StartsWith("f:") && fonts != null)
                {
                    var fontName = tag.Substring(2);
                    SpriteFont font;
                    if (fonts.TryParse(fontName, out font))
                    {
                        PushFont(font);
                        fontChanged = true;
                    }
                }
                else if (tag.Equals("/f"))
                {
                    PopFont();
                    fontChanged = true;
                }

                if (fontChanged)
                    fontChanges.Add(new FontChange() { Font = CurrentFont(), Index = tagStart });
            }
        }

        private static TagType ParseTag(StringPart text, ref Vector2 position, ref int lineSpacing, ref int lineIndex)
        {
            var type = TagType.None;

            if (text.Equals("\n"))
            {
                NewLine(ref position, ref lineSpacing, ref lineIndex);
                type = TagType.NewLine;
            }
            else if (text.StartsWith("f:") && fonts != null)
            {
                var fontName = text.Substring(2);
                SpriteFont font;
                if (fonts.TryParse(fontName, out font))
                {
                    PushFont(font);
                    type = TagType.FontStart;
                }                    
            }
            else if (text.Equals("/f"))
            {
                PopFont();
                type = TagType.FontEnd;
            }
            else if (text.StartsWith("c:"))
            {
                var colourName = text.Substring(2);
                Color colour;
                if (ColourParser.TryParse(colourName, out colour))
                {
                    PushColour(colour);
                    type = TagType.ColourStart;
                }
            }
            else if (text.Equals("/c"))
            {
                PopColour();
                type = TagType.ColourEnd;
            }

            return type;
        }

        private static SpriteFont CurrentFont()
        {
            if (fontHistory.Count > 0)
                return fontHistory.Peek();

            return defaultFont;
        }

        private static void PushFont(SpriteFont font)
        {
            fontHistory.Push(font);
        }

        private static void PopFont()
        {
            if (fontHistory.Count > 0)
                fontHistory.Pop();
        }

        private static Color CurrentColour()
        {
            if (colourHistory.Count > 0)
            {
                var colour = colourHistory.Peek().ToVector4();
                colour *= defaultColour.ToVector4();

                return new Color(colour);
            }

            return defaultColour;
        }

        private static void PushColour(Color colour)
        {
            colourHistory.Push(colour);
        }

        private static void PopColour()
        {
            if (colourHistory.Count > 0)
                colourHistory.Pop();
        }

        private static void NewLine(ref Vector2 position, ref int lineSpacing, ref int lineIndex)
        {
            position.X = 0;
            position.Y += lineSpacing;
            lineSpacing = CurrentFont().LineSpacing;

            lineIndex++;
        }
    }
}
