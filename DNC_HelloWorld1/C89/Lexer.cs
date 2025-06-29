using System;
using System.Collections.Generic;
using System.Text;

namespace DNC_HelloWorld1.C89
{
    /// <summary>
    /// C89 の字句解析器
    /// </summary>
    public class Lexer
    {
        private readonly string _source;
        private int _pos;
        private int _line = 1;
        private int _col = 1;

        private static readonly HashSet<string> Keywords = new(
            new[] {
                "auto","break","case","char","const","continue","default","do",
                "double","else","enum","extern","float","for","goto","if",
                "int","long","register","return","short","signed","sizeof",
                "static","struct","switch","typedef","union","unsigned",
                "void","volatile","while"
            });

        public Lexer(string source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public IEnumerable<Token> Tokenize()
        {
            while (true)
            {
                SkipWhiteSpaceAndComments();
                if (IsEnd)
                {
                    yield return new Token(TokenKind.EndOfFile, string.Empty, _line, _col);
                    break;
                }

                char ch = Peek();
                if (IsIdentifierStart(ch))
                {
                    int startCol = _col;
                    string ident = ReadIdentifier();
                    TokenKind kind = Keywords.Contains(ident) ? TokenKind.Keyword : TokenKind.Identifier;
                    yield return new Token(kind, ident, _line, startCol);
                }
                else if (char.IsDigit(ch))
                {
                    int startCol = _col;
                    string number = ReadNumber();
                    yield return new Token(TokenKind.IntegerConstant, number, _line, startCol);
                }
                else if (ch == '"')
                {
                    int startCol = _col;
                    string str = ReadString();
                    yield return new Token(TokenKind.StringLiteral, str, _line, startCol);
                }
                else if (ch == '\'')
                {
                    int startCol = _col;
                    string chr = ReadChar();
                    yield return new Token(TokenKind.CharLiteral, chr, _line, startCol);
                }
                else
                {
                    int startCol = _col;
                    string op = ReadPunctuator();
                    yield return new Token(TokenKind.Punctuator, op, _line, startCol);
                }
            }
        }

        private bool IsEnd => _pos >= _source.Length;

        private char Peek(int offset = 0) => _pos + offset < _source.Length ? _source[_pos + offset] : '\0';

        private char Next()
        {
            char c = _source[_pos++];
            if (c == '\n') { _line++; _col = 1; } else { _col++; }
            return c;
        }

        private void SkipWhiteSpaceAndComments()
        {
            while (!IsEnd)
            {
                char c = Peek();
                if (char.IsWhiteSpace(c)) { Next(); continue; }
                if (c == '/' && Peek(1) == '/')
                {
                    Next(); Next();
                    while (!IsEnd && Peek() != '\n') Next();
                    continue;
                }
                if (c == '/' && Peek(1) == '*')
                {
                    Next(); Next();
                    while (!IsEnd && !(Peek() == '*' && Peek(1) == '/'))
                    {
                        Next();
                    }
                    if (!IsEnd) { Next(); Next(); }
                    continue;
                }
                break;
            }
        }

        private bool IsIdentifierStart(char c) => char.IsLetter(c) || c == '_';

        private bool IsIdentifierPart(char c) => char.IsLetterOrDigit(c) || c == '_';

        private string ReadIdentifier()
        {
            var sb = new StringBuilder();
            while (!IsEnd && IsIdentifierPart(Peek()))
            {
                sb.Append(Next());
            }
            return sb.ToString();
        }

        private string ReadNumber()
        {
            var sb = new StringBuilder();
            if (Peek() == '0' && (Peek(1) == 'x' || Peek(1) == 'X'))
            {
                sb.Append(Next());
                sb.Append(Next());
                while (!IsEnd && IsHexDigit(Peek())) sb.Append(Next());
            }
            else if (Peek() == '0')
            {
                sb.Append(Next());
                while (!IsEnd && IsOctDigit(Peek())) sb.Append(Next());
            }
            else
            {
                while (!IsEnd && char.IsDigit(Peek())) sb.Append(Next());
            }
            return sb.ToString();
        }

        private static bool IsHexDigit(char c) => char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        private static bool IsOctDigit(char c) => c >= '0' && c <= '7';

        private string ReadString()
        {
            var sb = new StringBuilder();
            Next(); // skip opening "
            while (!IsEnd)
            {
                char c = Next();
                if (c == '\\')
                {
                    char n = Next();
                    sb.Append('\\');
                    sb.Append(n);
                }
                else if (c == '"')
                {
                    break;
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string ReadChar()
        {
            var sb = new StringBuilder();
            Next(); // '
            while (!IsEnd)
            {
                char c = Next();
                if (c == '\\')
                {
                    char n = Next();
                    sb.Append('\\');
                    sb.Append(n);
                }
                else if (c == '\'')
                {
                    break;
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private string ReadPunctuator()
        {
            // 最長一致を行うため、複数文字の演算子を優先してチェック
            string[] puncts = {
                "<<=",">>=","++","--","->","&&","||","<=",">=","==","!=","<<",">>",
                "+=","-=","*=","/=","%=","&=","^=","|=","::","..."
            };
            foreach (var p in puncts)
            {
                bool match = true;
                for (int i = 0; i < p.Length; i++)
                {
                    if (Peek(i) != p[i]) { match = false; break; }
                }
                if (match)
                {
                    for (int i = 0; i < p.Length; i++) Next();
                    return p;
                }
            }
            // 単一文字の演算子や区切り記号
            char ch = Next();
            return ch.ToString();
        }
    }
}
