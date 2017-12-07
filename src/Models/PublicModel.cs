using System;
using System.Collections.Generic;

namespace Gallery.MVC.Models
{
    public class PublicModel
    {
        public LimitKind Kind;
        public int LimitValue;
        public List<PublicTopic> Topics = new List<PublicTopic>();
    }

    public class PublicTopic
    {
        public string Title;
        public List<PublicBlob> Blobs = new List<PublicBlob>();
    }

    public class PublicLimits
    {
        public LimitKind Kind;
        public int LimitValue;

        public PublicLimits()
        {
        }

        public PublicLimits(LimitKind kind, int limitValue)
        {
            Kind = kind;
            LimitValue = limitValue;
        }

        public string Serialize()
        {
            return Kind + "=" + LimitValue;
        }

        public static PublicLimits Parse(string raw)
        {
            Action throwArg = () =>
            {
                throw new ArgumentException($"String {raw} is an invalid Limits value");
            };

            if (raw == null) throw new ArgumentNullException("raw");
            string[]  parts = raw.Split('=');
            if (parts.Length != 2) throwArg();
            LimitKind kind;
            if (!Enum.TryParse<LimitKind>(parts[0], out kind)) throwArg();
            int limit;
            if (!int.TryParse(parts[1], out limit)) throwArg();
            return new PublicLimits(kind, limit);
        }

        public override string ToString()
        {
            return $"{Kind}={LimitValue}";
        }

        protected bool Equals(PublicLimits other)
        {
            return Kind == other.Kind && LimitValue == other.LimitValue;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PublicLimits) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Kind * 397) ^ LimitValue;
            }
        }
    }

    public enum LimitKind
    {
        Width = 1,
        Height = 2,
    }

    public class PublicBlob
    {
        public string Id;
        public int Width;
        public int Height;
        public int File;
        public int Position;
        public int Length;
        public string IdContent;

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Width)}: {Width}, {nameof(Height)}: {Height}, {nameof(File)}: {File}, {nameof(Position)}: {Position}, {nameof(Length)}: {Length}, {nameof(IdContent)}: {IdContent}";
        }
    }
}
