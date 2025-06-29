using System.Collections.Generic;

namespace DNC_HelloWorld1.C89
{
    // 抽象構文木の基本ノード
    public abstract class AstNode { }

    public class TranslationUnit : AstNode
    {
        public List<ExternalDeclaration> Declarations { get; } = new();
    }

    public abstract class ExternalDeclaration : AstNode { }

    public class FunctionDefinition : ExternalDeclaration
    {
        public string ReturnType { get; set; } = "int";
        public string Name { get; set; } = string.Empty;
        public List<ParameterDeclaration> Parameters { get; set; } = new();
        public CompoundStatement Body { get; set; } = new();
    }

    public class ParameterDeclaration : AstNode
    {
        public string Type { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class Declaration : ExternalDeclaration
    {
        public string Type { get; set; } = string.Empty;
        public List<string> Names { get; } = new();
    }

    public abstract class Statement : AstNode { }

    public class CompoundStatement : Statement
    {
        public List<Statement> Statements { get; set; } = new();
    }

    public class ExpressionStatement : Statement
    {
        public Expression? Expr { get; set; }
    }

    public class ReturnStatement : Statement
    {
        public Expression? Expr { get; set; }
    }

    public class IfStatement : Statement
    {
        public Expression Condition { get; set; } = null!;
        public Statement Then { get; set; } = null!;
        public Statement? Else { get; set; }
    }

    public class WhileStatement : Statement
    {
        public Expression Condition { get; set; } = null!;
        public Statement Body { get; set; } = null!;
    }

    public class ForStatement : Statement
    {
        public Statement? Init { get; set; }
        public Expression? Condition { get; set; }
        public Expression? Post { get; set; }
        public Statement Body { get; set; } = null!;
    }

    public abstract class Expression : AstNode { }

    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; } = null!;
        public string Operator { get; set; } = string.Empty;
        public Expression Right { get; set; } = null!;
    }

    public class UnaryExpression : Expression
    {
        public string Operator { get; set; } = string.Empty;
        public Expression Operand { get; set; } = null!;
    }

    public class LiteralExpression : Expression
    {
        public string Value { get; set; } = string.Empty;
    }

    public class IdentifierExpression : Expression
    {
        public string Name { get; set; } = string.Empty;
    }
}
