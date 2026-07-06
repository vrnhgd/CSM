using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace CSM.Util
{
    /// <summary>
    ///     A small, dependency-free JSON parser. Exists because, in this Unity/Mono
    ///     environment, UnityEngine.JsonUtility fails to deserialize arrays of custom
    ///     types (class or struct) and Newtonsoft.Json's default contract resolver
    ///     depends on System.Runtime.Serialization, which isn't part of the game's
    ///     bundled Mono runtime. Parses into plain object graphs: Dictionary&lt;string, object&gt;
    ///     for objects, List&lt;object&gt; for arrays, string, double, bool, or null for scalars.
    /// </summary>
    public static class MiniJson
    {
        public static object Parse(string json)
        {
            int index = 0;
            object result = ParseValue(json, ref index);
            return result;
        }

        private static object ParseValue(string json, ref int index)
        {
            SkipWhitespace(json, ref index);

            char c = json[index];
            switch (c)
            {
                case '{':
                    return ParseObject(json, ref index);
                case '[':
                    return ParseArray(json, ref index);
                case '"':
                    return ParseString(json, ref index);
                case 't':
                case 'f':
                    return ParseBool(json, ref index);
                case 'n':
                    index += 4; // "null"
                    return null;
                default:
                    return ParseNumber(json, ref index);
            }
        }

        private static Dictionary<string, object> ParseObject(string json, ref int index)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();

            index++; // consume '{'
            SkipWhitespace(json, ref index);

            if (json[index] == '}')
            {
                index++;
                return result;
            }

            while (true)
            {
                SkipWhitespace(json, ref index);
                string key = ParseString(json, ref index);

                SkipWhitespace(json, ref index);
                index++; // consume ':'

                object value = ParseValue(json, ref index);
                result[key] = value;

                SkipWhitespace(json, ref index);
                if (json[index] == ',')
                {
                    index++;
                    continue;
                }

                index++; // consume '}'
                break;
            }

            return result;
        }

        private static List<object> ParseArray(string json, ref int index)
        {
            List<object> result = new List<object>();

            index++; // consume '['
            SkipWhitespace(json, ref index);

            if (json[index] == ']')
            {
                index++;
                return result;
            }

            while (true)
            {
                object value = ParseValue(json, ref index);
                result.Add(value);

                SkipWhitespace(json, ref index);
                if (json[index] == ',')
                {
                    index++;
                    continue;
                }

                index++; // consume ']'
                break;
            }

            return result;
        }

        private static string ParseString(string json, ref int index)
        {
            StringBuilder sb = new StringBuilder();

            index++; // consume opening '"'
            while (json[index] != '"')
            {
                char c = json[index];
                if (c == '\\')
                {
                    index++;
                    char escaped = json[index];
                    switch (escaped)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            string hex = json.Substring(index + 1, 4);
                            sb.Append((char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture));
                            index += 4;
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }

                index++;
            }

            index++; // consume closing '"'
            return sb.ToString();
        }

        private static bool ParseBool(string json, ref int index)
        {
            if (json[index] == 't')
            {
                index += 4; // "true"
                return true;
            }

            index += 5; // "false"
            return false;
        }

        private static double ParseNumber(string json, ref int index)
        {
            int start = index;
            while (index < json.Length && (char.IsDigit(json[index]) || json[index] == '-' || json[index] == '+' ||
                                            json[index] == '.' || json[index] == 'e' || json[index] == 'E'))
            {
                index++;
            }

            return double.Parse(json.Substring(start, index - start), CultureInfo.InvariantCulture);
        }

        private static void SkipWhitespace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }
    }
}
