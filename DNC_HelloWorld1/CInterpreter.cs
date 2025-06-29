using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DNC_HelloWorld1
{
    /// <summary>
    /// 非安全コードを用いずに、ごく簡易的な C89 風プログラムを解釈し実行するクラス。
    /// 本実装は多数の機能を省いており、変数宣言、代入、加減算、printf、return のみを
    /// サポートする簡易的なものです。完全な C89 互換のインタプリタではありません。
    /// </summary>
    public static class CInterpreter
    {
        class Context
        {
            public Dictionary<string, int> Variables { get; } = new();
        }

        /// <summary>
        /// 与えられた C 風ソースコードから main 関数を抽出して解釈実行する。
        /// サポートする構文はごく一部のみであり、複雑なプログラムには対応していない。
        /// </summary>
        /// <param name="cProgramBody">C 言語のソースコード (プリプロセッサ命令は不可)</param>
        /// <returns>main 関数の戻り値</returns>
        public static int RunC(string cProgramBody)
        {
            if (cProgramBody is null) throw new ArgumentNullException(nameof(cProgramBody));

            var mainMatch = Regex.Match(cProgramBody, @"int\s+main\s*\(\s*\)\s*\{(.*)\}", RegexOptions.Singleline);
            if (!mainMatch.Success)
            {
                throw new InvalidOperationException("main 関数が見つかりません");
            }

            var body = mainMatch.Groups[1].Value;
            var ctx = new Context();
            var statements = body.Split(';');
            int returnValue = 0;

            foreach (var raw in statements)
            {
                var line = raw.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                if (line.StartsWith("int "))
                {
                    var decl = line.Substring(4);
                    var parts = decl.Split('=');
                    var varName = parts[0].Trim();
                    int val = 0;
                    if (parts.Length > 1)
                    {
                        val = EvaluateExpression(parts[1], ctx);
                    }
                    ctx.Variables[varName] = val;
                }
                else if (line.StartsWith("return "))
                {
                    var expr = line.Substring(7);
                    returnValue = EvaluateExpression(expr, ctx);
                    break;
                }
                else if (line.StartsWith("printf"))
                {
                    var match = Regex.Match(line, @"printf\s*\((.*)\)");
                    if (match.Success)
                    {
                        var args = match.Groups[1].Value.Trim();
                        if (args.StartsWith("\"") && args.EndsWith("\""))
                        {
                            var content = args.Substring(1, args.Length - 2)
                                .Replace("\\n", "\n")
                                .Replace("\\t", "\t");
                            Console.Write(content);
                        }
                    }
                }
                else
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        var varName = parts[0].Trim();
                        var expr = parts[1];
                        ctx.Variables[varName] = EvaluateExpression(expr, ctx);
                    }
                }
            }

            return returnValue;
        }

        static int EvaluateExpression(string expr, Context ctx)
        {
            expr = expr.Trim();
            var tokens = new List<string>();
            var ops = new List<char>();
            int pos = 0;
            while (pos < expr.Length)
            {
                if (expr[pos] == '+' || expr[pos] == '-')
                {
                    ops.Add(expr[pos]);
                    pos++;
                }
                else if (char.IsWhiteSpace(expr[pos]))
                {
                    pos++;
                }
                else
                {
                    int start = pos;
                    while (pos < expr.Length && !"+-".Contains(expr[pos]) && !char.IsWhiteSpace(expr[pos])) pos++;
                    tokens.Add(expr.Substring(start, pos - start));
                }
            }

            if (tokens.Count == 0) return 0;
            int value = GetTokenValue(tokens[0], ctx);
            for (int i = 0; i < ops.Count; i++)
            {
                int val = GetTokenValue(tokens[i + 1], ctx);
                if (ops[i] == '+') value += val; else value -= val;
            }
            return value;
        }

        static int GetTokenValue(string token, Context ctx)
        {
            if (int.TryParse(token, out int num)) return num;
            if (ctx.Variables.TryGetValue(token, out int val)) return val;
            return 0;
        }
    }
}
