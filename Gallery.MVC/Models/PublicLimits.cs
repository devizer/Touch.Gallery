using System;

namespace Gallery.MVC.Models
{
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
}