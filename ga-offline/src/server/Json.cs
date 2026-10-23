using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace GAOffline
{
    // Responses are built as Dictionary<string, object> / List<object> trees and written here.
    // The client's JSONObject parser deletes every backslash before parsing, so escapes are never emitted:
    // quotes become apostrophes and control characters become spaces.
    public static class Json
    {
        public static string Write(object v)
        {
            var sb = new StringBuilder();
            Emit(sb, v);
            return sb.ToString();
        }

        static void Emit(StringBuilder sb, object v)
        {
            if (v == null) { sb.Append("null"); return; }
            if (v is string) { Str(sb, (string)v); return; }
            if (v is bool) { sb.Append((bool)v ? "true" : "false"); return; }
            if (v is int || v is long) { sb.Append(Convert.ToInt64(v).ToString(CultureInfo.InvariantCulture)); return; }
            if (v is float || v is double)
            {
                double d = Convert.ToDouble(v);
                sb.Append(d == Math.Floor(d) && Math.Abs(d) < 1e15 ? ((long)d).ToString(CultureInfo.InvariantCulture) : d.ToString("R", CultureInfo.InvariantCulture));
                return;
            }
            var dict = v as IDictionary;
            if (dict != null)
            {
                sb.Append('{');
                bool first = true;
                foreach (DictionaryEntry e in dict)
                {
                    if (!first) sb.Append(',');
                    first = false;
                    Str(sb, e.Key.ToString());
                    sb.Append(':');
                    Emit(sb, e.Value);
                }
                sb.Append('}');
                return;
            }
            var list = v as IList;
            if (list != null)
            {
                sb.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    Emit(sb, list[i]);
                }
                sb.Append(']');
                return;
            }
            Str(sb, v.ToString());
        }

        static void Str(StringBuilder sb, string s)
        {
            sb.Append('"');
            foreach (char c in s)
            {
                if (c == '"') sb.Append('\'');
                else if (c == '\\') sb.Append('/');
                else if (c < ' ') sb.Append(' ');
                else sb.Append(c);
            }
            sb.Append('"');
        }

        // Minimal JSON reader for the embedded data and the save file (not the client's JSONObject, which strips
        // backslashes and is slow on large input). Objects -> Dictionary<string, object>, arrays -> List<object>,
        // numbers -> double.
        public static object Parse(string text)
        {
            int i = 0;
            return Value(text, ref i);
        }

        static void Ws(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        static object Value(string s, ref int i)
        {
            Ws(s, ref i);
            if (i >= s.Length) return null;
            char c = s[i];
            if (c == '{')
            {
                var d = new Dictionary<string, object>();
                i++;
                Ws(s, ref i);
                if (s[i] == '}') { i++; return d; }
                while (true)
                {
                    Ws(s, ref i);
                    string k = StrLit(s, ref i);
                    Ws(s, ref i);
                    i++; // ':'
                    d[k] = Value(s, ref i);
                    Ws(s, ref i);
                    if (s[i++] == '}') return d;
                }
            }
            if (c == '[')
            {
                var l = new List<object>();
                i++;
                Ws(s, ref i);
                if (s[i] == ']') { i++; return l; }
                while (true)
                {
                    l.Add(Value(s, ref i));
                    Ws(s, ref i);
                    if (s[i++] == ']') return l;
                }
            }
            if (c == '"') return StrLit(s, ref i);
            if (s.Length - i >= 4 && string.CompareOrdinal(s, i, "true", 0, 4) == 0) { i += 4; return true; }
            if (s.Length - i >= 5 && string.CompareOrdinal(s, i, "false", 0, 5) == 0) { i += 5; return false; }
            if (s.Length - i >= 4 && string.CompareOrdinal(s, i, "null", 0, 4) == 0) { i += 4; return null; }
            int st = i;
            while (i < s.Length && "+-0123456789.eE".IndexOf(s[i]) >= 0) i++;
            return double.Parse(s.Substring(st, i - st), CultureInfo.InvariantCulture);
        }

        static string StrLit(string s, ref int i)
        {
            var sb = new StringBuilder();
            i++; // opening quote
            while (s[i] != '"')
            {
                char c = s[i++];
                if (c != '\\') { sb.Append(c); continue; }
                char e = s[i++];
                switch (e)
                {
                    case 'n': sb.Append('\n'); break;
                    case 't': sb.Append('\t'); break;
                    case 'r': sb.Append('\r'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'u': sb.Append((char)Convert.ToInt32(s.Substring(i, 4), 16)); i += 4; break;
                    default: sb.Append(e); break;
                }
            }
            i++;
            return sb.ToString();
        }

        // typed reads from the native tree
        public static Dictionary<string, object> O(object o, string k)
        {
            var d = o as Dictionary<string, object>;
            object v;
            if (d != null && d.TryGetValue(k, out v) && v is Dictionary<string, object>) return (Dictionary<string, object>)v;
            return null;
        }

        public static List<object> L(object o, string k)
        {
            var d = o as Dictionary<string, object>;
            object v;
            if (d != null && d.TryGetValue(k, out v) && v is List<object>) return (List<object>)v;
            return new List<object>();
        }

        public static long N(object o, string k, long def = 0)
        {
            var d = o as Dictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(k, out v) || v == null) return def;
            if (v is double) return (long)(double)v;
            if (v is int || v is long) return Convert.ToInt64(v);
            if (v is bool) return (bool)v ? 1 : 0;
            long r;
            return long.TryParse(v.ToString(), out r) ? r : def;
        }

        public static int I(object o, string k, int def = 0)
        {
            return (int)N(o, k, def);
        }

        public static string S(object o, string k, string def = "")
        {
            var d = o as Dictionary<string, object>;
            object v;
            if (d == null || !d.TryGetValue(k, out v) || v == null) return def;
            if (v is double) return ((long)(double)v).ToString(CultureInfo.InvariantCulture);
            return v.ToString();
        }
    }
}
