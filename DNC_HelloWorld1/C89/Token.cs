using System;

namespace DNC_HelloWorld1.C89
{
    /// <summary>
    /// C89 のトークン種別
    /// </summary>
    public enum TokenKind
    {
        EndOfFile,
        Identifier,
        Keyword,
        IntegerConstant,
        StringLiteral,
        CharLiteral,
        Punctuator
    }

    /// <summary>
    /// 字句解析で得られるトークン
    /// </summary>
    public class Token
    {
        public TokenKind Kind { get; }
        public string Text { get; }
        public int Line { get; }
        public int Column { get; }

        public Token(TokenKind kind, string text, int line, int column)
        {
            Kind = kind;
            Text = text;
            Line = line;
            Column = column;
        }

        public override string ToString() => $"{Kind}: {Text} (L{Line},C{Column})";
    }
}
