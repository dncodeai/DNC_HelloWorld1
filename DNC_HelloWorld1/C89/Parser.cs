using System;
using System.Collections.Generic;
using System.Linq;

namespace DNC_HelloWorld1.C89
{
    /// <summary>
    /// C89 構文解析器
    /// </summary>
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _index;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens.ToList();
        }

        private Token Peek(int offset = 0) => _index + offset < _tokens.Count ? _tokens[_index + offset] : _tokens[^1];
        private Token Next() => _tokens[_index++];
        private bool Match(string text)
        {
            if (Peek().Text == text)
            {
                _index++;
                return true;
            }
            return false;
        }

        public TranslationUnit Parse()
        {
            var tu = new TranslationUnit();
            while (Peek().Kind != TokenKind.EndOfFile)
            {
                tu.Declarations.Add(ParseExternalDeclaration());
            }
            return tu;
        }

        private ExternalDeclaration ParseExternalDeclaration()
        {
            // ここでは単純化のため、関数定義のみを扱う
            string type = ParseTypeName();
            string name = Expect(TokenKind.Identifier).Text;
            Expect("(");
            var parameters = new List<ParameterDeclaration>();
            if (!Match(")"))
            {
                do
                {
                    string ptype = ParseTypeName();
                    string pname = Expect(TokenKind.Identifier).Text;
                    parameters.Add(new ParameterDeclaration { Type = ptype, Name = pname });
                } while (Match(","));
                Expect(")");
            }
            var body = ParseCompoundStatement();
            return new FunctionDefinition
            {
                ReturnType = type,
                Name = name,
                Parameters = parameters,
                Body = body
            };
        }

        private string ParseTypeName()
        {
            var tk = Peek();
            if (tk.Kind == TokenKind.Keyword)
            {
                _index++;
                return tk.Text;
            }
            throw new InvalidOperationException($"型名を期待しました: {tk}");
        }

        private CompoundStatement ParseCompoundStatement()
        {
            Expect("{");
            var stmts = new List<Statement>();
            while (!Match("}"))
            {
                stmts.Add(ParseStatement());
            }
            return new CompoundStatement { Statements = stmts };
        }

        private Statement ParseStatement()
        {
            var tk = Peek();
            if (tk.Text == "return")
            {
                Next();
                Expression? expr = null;
                if (!Match(";"))
                {
                    expr = ParseExpression();
                    Expect(";");
                }
                return new ReturnStatement { Expr = expr };
            }
            if (tk.Text == "{")
            {
                return ParseCompoundStatement();
            }
            if (tk.Text == "if")
            {
                Next();
                Expect("(");
                var cond = ParseExpression();
                Expect(")");
                var then = ParseStatement();
                Statement? els = null;
                if (Match("else"))
                {
                    els = ParseStatement();
                }
                return new IfStatement { Condition = cond, Then = then, Else = els };
            }
            if (tk.Text == "while")
            {
                Next();
                Expect("(");
                var cond = ParseExpression();
                Expect(")");
                var body = ParseStatement();
                return new WhileStatement { Condition = cond, Body = body };
            }
            if (tk.Text == "for")
            {
                Next();
                Expect("(");
                Statement? init = null;
                if (!Match(";"))
                {
                    init = ParseExpressionStatement();
                }
                Expression? cond = null;
                if (!Match(";"))
                {
                    cond = ParseExpression();
                    Expect(";");
                }
                Expression? post = null;
                if (!Match(")"))
                {
                    post = ParseExpression();
                    Expect(")");
                }
                var body = ParseStatement();
                return new ForStatement { Init = init, Condition = cond, Post = post, Body = body };
            }
            return ParseExpressionStatement();
        }

        private ExpressionStatement ParseExpressionStatement()
        {
            if (Match(";")) return new ExpressionStatement();
            var expr = ParseExpression();
            Expect(";");
            return new ExpressionStatement { Expr = expr };
        }

        private Expression ParseExpression()
        {
            return ParseAssignment();
        }

        private Expression ParseAssignment()
        {
            var left = ParseEquality();
            if (Match("="))
            {
                var right = ParseAssignment();
                return new BinaryExpression { Left = left, Operator = "=", Right = right };
            }
            return left;
        }

        private Expression ParseEquality()
        {
            var expr = ParseRelational();
            while (true)
            {
                if (Match("=="))
                {
                    expr = new BinaryExpression { Left = expr, Operator = "==", Right = ParseRelational() };
                }
                else if (Match("!="))
                {
                    expr = new BinaryExpression { Left = expr, Operator = "!=", Right = ParseRelational() };
                }
                else break;
            }
            return expr;
        }

        private Expression ParseRelational()
        {
            var expr = ParseAdditive();
            while (true)
            {
                if (Match("<")) expr = new BinaryExpression { Left = expr, Operator = "<", Right = ParseAdditive() };
                else if (Match("<=")) expr = new BinaryExpression { Left = expr, Operator = "<=", Right = ParseAdditive() };
                else if (Match(">")) expr = new BinaryExpression { Left = expr, Operator = ">", Right = ParseAdditive() };
                else if (Match(">=")) expr = new BinaryExpression { Left = expr, Operator = ">=", Right = ParseAdditive() };
                else break;
            }
            return expr;
        }

        private Expression ParseAdditive()
        {
            var expr = ParseTerm();
            while (true)
            {
                if (Match("+")) expr = new BinaryExpression { Left = expr, Operator = "+", Right = ParseTerm() };
                else if (Match("-")) expr = new BinaryExpression { Left = expr, Operator = "-", Right = ParseTerm() };
                else break;
            }
            return expr;
        }

        private Expression ParseTerm()
        {
            var expr = ParseFactor();
            while (true)
            {
                if (Match("*")) expr = new BinaryExpression { Left = expr, Operator = "*", Right = ParseFactor() };
                else if (Match("/")) expr = new BinaryExpression { Left = expr, Operator = "/", Right = ParseFactor() };
                else break;
            }
            return expr;
        }

        private Expression ParseFactor()
        {
            var tk = Peek();
            if (Match("("))
            {
                var expr = ParseExpression();
                Expect(")");
                return expr;
            }
            if (tk.Kind == TokenKind.IntegerConstant)
            {
                Next();
                return new LiteralExpression { Value = tk.Text };
            }
            if (tk.Kind == TokenKind.Identifier)
            {
                Next();
                return new IdentifierExpression { Name = tk.Text };
            }
            throw new InvalidOperationException($"式が解析できません: {tk}");
        }

        private Token Expect(TokenKind kind)
        {
            var tk = Peek();
            if (tk.Kind != kind) throw new InvalidOperationException($"{kind} が必要ですが {tk.Kind} でした");
            _index++;
            return tk;
        }

        private Token Expect(string text)
        {
            var tk = Peek();
            if (tk.Text != text) throw new InvalidOperationException($"'{text}' が必要です (現在: {tk.Text})");
            _index++;
            return tk;
        }
    }
}
