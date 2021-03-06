using System;
using System.Text;
using NHaml.Core.Ast;
using NHaml.Core.IO;

namespace NHaml.Core.Parser
{
    public class TextParser
    {
        private readonly CharacterReader _reader;
        private StringBuilder _buffer;
        private bool? _escape;

        public TextParser(CharacterReader reader, bool? escape)
        {
            if(reader == null)
                throw new ArgumentNullException("reader");

            _reader = reader;
            _escape = escape;
        }

        public TextNode Parse()
        {
            var node = new TextNode();
            _buffer = new StringBuilder();

            while(_reader.Read())
                switch(_reader.CurrentChar)
                {
                    case '\\': // is possible escaping
                    {
                        if(_reader.NextChar == '#' || _reader.NextChar == '\\')
                            _reader.Skip("\\"); // escaped

                        goto default;
                    }
                    case '#':
                    {
                        if(_reader.NextChar == '{')
                        {
                            ParseInterpolation(node);

                            continue;
                        }

                        goto default;
                    }
                    default:
                    {
                        _buffer.Append(_reader.CurrentChar);
                        break;
                    }
                }

            if(_buffer.Length > 0)
                node.Chunks.Add(new TextChunk(_buffer.ToString()));

            return node.Chunks.Count == 0 ? null : node;
        }

        private void ParseInterpolation(TextNode node)
        {
            _reader.Skip("#");

            if(_buffer.Length > 0)
                node.Chunks.Add(new TextChunk(ReturnAndClearBuffer()));

            while(_reader.Read() && _reader.CurrentChar != '}')
            {
                if(_reader.CurrentChar == '\\')
                    if(_reader.NextChar == '}' || _reader.NextChar == '\\')
                        _reader.Skip("\\"); // escaping

                _buffer.Append(_reader.CurrentChar);
            }

            if(_buffer.Length > 0)
                node.Chunks.Add(new CodeChunk(ReturnAndClearBuffer(),_escape));
        }

        private string ReturnAndClearBuffer()
        {
            var buffer = _buffer.ToString();

            _buffer = new StringBuilder();

            return buffer;
        }
    }
}