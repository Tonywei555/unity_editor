using System;
using System.Collections.Generic;
using System.Text;

namespace UnityEditorVdf
{
    public sealed class VdfNode
    {
        public VdfNode(string key)
        {
            Key = key;
            Children = new List<VdfNode>();
        }

        public string Key { get; }
        public string Value { get; set; }
        public List<VdfNode> Children { get; }

        public bool HasChildren => Children.Count > 0;
    }

    public static class VdfParser
    {
        public static VdfNode Parse(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var tokens = Tokenize(input);
            var index = 0;
            var root = new VdfNode("root");

            while (index < tokens.Count)
            {
                root.Children.Add(ParseNode(tokens, ref index));
            }

            return root;
        }

        public static string Serialize(VdfNode node)
        {
            var builder = new StringBuilder();
            foreach (var child in node.Children)
            {
                WriteNode(builder, child, 0);
            }

            return builder.ToString();
        }

        private static VdfNode ParseNode(IReadOnlyList<string> tokens, ref int index)
        {
            var key = tokens[index++];
            if (index >= tokens.Count)
            {
                throw new FormatException($"Unexpected end of VDF after key '{key}'.");
            }

            var valueOrBrace = tokens[index++];
            var node = new VdfNode(key);

            if (valueOrBrace == "{")
            {
                while (index < tokens.Count && tokens[index] != "}")
                {
                    node.Children.Add(ParseNode(tokens, ref index));
                }

                if (index >= tokens.Count || tokens[index] != "}")
                {
                    throw new FormatException($"Missing closing brace for key '{key}'.");
                }

                index++;
            }
            else
            {
                node.Value = valueOrBrace;
            }

            return node;
        }

        private static void WriteNode(StringBuilder builder, VdfNode node, int indent)
        {
            var indentString = new string('\t', indent);
            builder.Append(indentString);
            builder.Append('"').Append(Escape(node.Key)).Append('"');

            if (node.HasChildren)
            {
                builder.AppendLine();
                builder.Append(indentString).AppendLine("{");
                foreach (var child in node.Children)
                {
                    WriteNode(builder, child, indent + 1);
                }

                builder.Append(indentString).AppendLine("}");
            }
            else
            {
                builder.Append(' ');
                builder.Append('"').Append(Escape(node.Value ?? string.Empty)).Append('"');
                builder.AppendLine();
            }
        }

        private static List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var index = 0;

            while (index < input.Length)
            {
                var current = input[index];

                if (char.IsWhiteSpace(current))
                {
                    index++;
                    continue;
                }

                if (current == '/' && index + 1 < input.Length && input[index + 1] == '/')
                {
                    index += 2;
                    while (index < input.Length && input[index] != '\n')
                    {
                        index++;
                    }

                    continue;
                }

                if (current == '{' || current == '}')
                {
                    tokens.Add(current.ToString());
                    index++;
                    continue;
                }

                if (current == '"')
                {
                    tokens.Add(ReadQuoted(input, ref index));
                    continue;
                }

                tokens.Add(ReadBare(input, ref index));
            }

            return tokens;
        }

        private static string ReadQuoted(string input, ref int index)
        {
            var builder = new StringBuilder();
            index++;

            while (index < input.Length)
            {
                var current = input[index++];
                if (current == '"')
                {
                    break;
                }

                if (current == '\\' && index < input.Length)
                {
                    var escaped = input[index++];
                    builder.Append(escaped);
                    continue;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

        private static string ReadBare(string input, ref int index)
        {
            var start = index;
            while (index < input.Length)
            {
                var current = input[index];
                if (char.IsWhiteSpace(current) || current == '{' || current == '}')
                {
                    break;
                }

                index++;
            }

            return input.Substring(start, index - start);
        }

        private static string Escape(string value)
        {
            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
