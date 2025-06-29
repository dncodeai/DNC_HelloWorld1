using System;
using System.Collections.Generic;
using DNC_HelloWorld1.C89;

namespace DNC_HelloWorld1
{
    /// <summary>
    /// C89 の字句解析と構文解析を行うインタプリタ基盤。
    /// まだ実行部分は実装されていない。
    /// </summary>
    public static class CInterpreter
    {
        /// <summary>
        /// 与えられた C89 ソースコードを解析して抽象構文木を生成する。
        /// 現段階では実行は行わず、main 関数が存在することのみ確認する。
        /// </summary>
        /// <param name="cProgramBody">C プログラム</param>
        /// <returns>main 関数の戻り値 (現在は常に 0)</returns>
        public static int RunC(string cProgramBody)
        {
            if (cProgramBody is null) throw new ArgumentNullException(nameof(cProgramBody));

            var lexer = new Lexer(cProgramBody);
            var tokens = lexer.Tokenize();
            var parser = new Parser(tokens);
            TranslationUnit tu = parser.Parse();

            // main 関数が存在するかチェック
            bool hasMain = false;
            foreach (var ext in tu.Declarations)
            {
                if (ext is FunctionDefinition fd && fd.Name == "main")
                {
                    hasMain = true;
                    break;
                }
            }
            if (!hasMain)
                throw new InvalidOperationException("main 関数が見つかりません");

            // 実行機能は今後実装予定
            return 0;
        }
    }
}
