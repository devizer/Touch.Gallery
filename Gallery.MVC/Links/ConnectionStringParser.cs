using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace WaitFor.Common
{
    // Valid Status=200,403,100-499; Uri=http://mywebapi:80/get-status; Method=POST; *Accept=application/json, text/javascript; Payload={'verbosity':'normal'}"
    public class ConnectionStringParser
    {
        [NotNull]
        public readonly string ConnectionString;

        [NotNull, ItemNotNull]
        public IEnumerable<Pair> Pairs => _LazyPairs.Value;

        private readonly Lazy<List<Pair>> _LazyPairs;

        public class Pair
        {

            [NotNull] public string Key { get; set; }
            [NotNull] public string Value { get; set; }

            public bool HasKey { get; set; }

            public bool IsValueTrue
            {
                get
                {
                    const StringComparison Ignore = StringComparison.InvariantCultureIgnoreCase;

                    return Value != null &&
                           ("True".Equals(Value, Ignore)
                            || "On".Equals(Value, Ignore)
                            || "Yes".Equals(Value, Ignore));
                }

            }

            public override string ToString()
            {
                return $"{nameof(HasKey)}: {HasKey}, {nameof(Key)}: {Key}, {nameof(Value)}: {Value}";
            }
        }

        private ConnectionStringParser()
        {
            _LazyPairs = new Lazy<List<Pair>>(() => Parse_Impl());
        }

        public ConnectionStringParser(string connectionString) : this()
        {
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }


        List<Pair> Parse_Impl()
        {
            List<Pair> ret = new List<Pair>();
            var vars = this.
                ConnectionString.Split(';')
                .Select(x => x.Trim())
                .Where(x => x.Length > 0);

            foreach (var v in vars)
            {
                int p = v.IndexOf('=');
                if (p < 0)
                {
                    ret.Add(new Pair { Key = v, Value = v, HasKey = false });
                }
                else
                {
                    string key = p > 0 ? v.Substring(0, p).Trim() : "";
                    string value = p<v.Length-1 ? v.Substring(p + 1).Trim() : "";
                    ret.Add(new Pair
                    {
                        Key = key,
                        Value = value,
                        HasKey = !string.IsNullOrEmpty(key)
                    });
                }
            }

            return ret;
        }

    }

    public static class ConnectionStringBuilderExtensions
    {
        const StringComparison Ignore = StringComparison.InvariantCultureIgnoreCase;

        public static bool GetFirstBool(this IEnumerable<ConnectionStringParser.Pair> pairs, string parameterName, bool defVal = false)
        {
            return pairs.FirstOrDefault(x => parameterName.Equals(x.Key, Ignore))
                       ?.IsValueTrue ?? defVal;
        }

        public static int GetFirstInt(this IEnumerable<ConnectionStringParser.Pair> pairs, string parameterName, int defVal, int min = 0, int max = Int32.MaxValue)
        {
            var raw = pairs.FirstOrDefault(x => parameterName.Equals(x.Key, Ignore))?.Value;
            int ret;
            if (raw == null || !int.TryParse(raw, out ret))
                ret = defVal;

            return Math.Max(Math.Min(ret, max), min);
        }

        public static string GetFirstString(this IEnumerable<ConnectionStringParser.Pair> pairs, string parameterName)
        {
            return pairs.FirstOrDefault(x => parameterName.Equals(x.Key, Ignore))?.Value;
        }

        public static string GetFirstWithoutKey(this IEnumerable<ConnectionStringParser.Pair> pairs)
        {
            return pairs.FirstOrDefault(x => !x.HasKey && !string.IsNullOrWhiteSpace(x.Value))?.Value;
        }
    }

}
