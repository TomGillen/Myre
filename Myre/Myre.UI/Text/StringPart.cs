﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Myre.UI.Text
{
    public struct StringPart
    {
        public readonly StringBuilder StringBuilder;
        public readonly string String;
        public readonly int Start;
        public readonly int Length;

        public char this[int i]
        {
            get 
            {
                if (StringBuilder == null)
                    return String[Start + i];
                else
                    return StringBuilder[Start + i];
            }
        }

        public StringPart(string source, int start, int length)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (start < 0 || start > source.Length)
                throw new ArgumentOutOfRangeException("start", "start must be > 0, and <= source.Length");
            if (length < 0 || (start + length) > source.Length)
                throw new ArgumentOutOfRangeException("length", "length must be >= 0, and (start + length) <= source.Length");

            this.StringBuilder = null;
            this.String = source;
            this.Start = start;
            this.Length = length;
        }

        public StringPart(StringBuilder source, int start, int length)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (start < 0 || start >= source.Length)
                throw new ArgumentOutOfRangeException("start", "start must be > 0, and < source.Length");
            if (length < 0 || (start + length) > source.Length)
                throw new ArgumentOutOfRangeException("length", "length must be >= 0, and (start + length) <= source.Length");

            this.StringBuilder = source;
            this.String = null;
            this.Start = start;
            this.Length = length;
        }

        public bool StartsWith(StringPart s)
        {
            if (this.Length < s.Length)
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                if (this[i] != s[i])
                    return false;
            }

            return true;
        }

        public StringPart Substring(int start)
        {
            return Substring(start, Length - start);
        }

        public StringPart Substring(int start, int length)
        {
            if (String != null)
                return new StringPart(String, Start + start, length);
            else
                return new StringPart(StringBuilder, Start + start, length);
        }
        
        public override bool Equals(object obj)
        {
            if (obj is StringPart || obj is string)
                return Equals((StringPart)obj);

            if (obj is StringBuilder)
                return Equals((StringBuilder)obj);

            return base.Equals(obj);
        }

        public bool Equals(StringPart other)
        {
            if (this.Length != other.Length)
                return false;

            if (this.String == other.String && this.StringBuilder == other.StringBuilder)
            {
                return this.Start == other.Start;
            }
            else
            {
                if ((this.String == null && this.StringBuilder == null) ||
                    (other.String == null && other.StringBuilder == null))
                    return false;

                for (int i = 0; i < this.Length; i++)
                {
                    if (other[i] != this[i])
                        return false;
                }

                return true;
            }
        }

        public bool Equals(string other)
        {
            return this.Equals((StringPart)other);
        }

        public bool Equals(StringBuilder other)
        {
            return Equals(other, 0, other.Length);
        }
        
        public bool Equals(StringBuilder other, int start, int length)
        {
            if (other == null || (this.String == null && this.StringBuilder == null))
                return false;

            if (this.Length != length)
                return false;

            for (int i = 0; i < length; i++)
            {
                if (other[i + start] != this.String[i + this.Start])
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            object value = String ?? (object)StringBuilder ?? "";
            return value.GetHashCode() ^ Start.GetHashCode() ^ Length.GetHashCode();
        }

        public override string ToString()
        {
            object value = String ?? (object)StringBuilder;
            return value == null ? null : value.ToString().Substring(Start, Length);
        }

        public static implicit operator StringPart(string text)
        {
            return new StringPart(text, 0, text.Length);
        }
    }

    public static class StringPartExtensions
    {
        public static void AppendPart(this StringBuilder sb, StringPart part)
        {
            if (part.String != null)
                sb.Append(part.String, part.Start, part.Length);
            else if (part.StringBuilder != null)
            {
                sb.EnsureCapacity(sb.Length + part.Length);
                for (int i = 0; i < part.Length; i++)
                    sb.Append(part[i]);
            }
        }
    }
}
