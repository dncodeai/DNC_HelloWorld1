using System;

namespace DNC_HelloWorld1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            // 追加でスタートメッセージを表示
            Console.WriteLine("Start Game");

            // 勝敗と学習用データ
            int[] userMoves = new int[4];                   // 1:グー 2:チョキ 3:パー の出現回数
            int[,] transition = new int[4, 4];              // 直前の手 -> 次の手 の遷移回数
            int lastUser = 0;                               // 前回ユーザーが出した手
            int win = 0, lose = 0, draw = 0;                // 勝敗の記録
            Random rand = new Random();

            while (true)
            {
                Console.WriteLine("手を入力してください (1: グー, 2: チョキ, 3: パー, 0: 習性を見る)");
                string? input = Console.ReadLine();

                if (input == "0")
                {
                    ShowStatus(userMoves, transition, lastUser);
                    continue;
                }

                if (!int.TryParse(input, out int user) || user < 1 || user > 3)
                {
                    Console.WriteLine("無効な入力です");
                    continue;
                }

                // 学習データの更新
                userMoves[user]++;
                if (lastUser != 0)
                {
                    transition[lastUser, user]++;
                }

                // ユーザーの次の手を予想して、それに勝つ手を出す
                int predicted = PredictUserMove(userMoves, transition, lastUser, rand);
                int machine = GetWinningMove(predicted);

                Console.WriteLine($"機械の手: {ToHandString(machine)}");

                // 勝敗判定
                int result = (3 + machine - user) % 3;
                if (result == 0)
                {
                    draw++;
                    Console.WriteLine("結果: あいこ");
                }
                else if (result == 1)
                {
                    lose++;
                    Console.WriteLine("結果: あなたの勝ち");
                }
                else
                {
                    win++;
                    Console.WriteLine("結果: 機械の勝ち");
                }

                lastUser = user;

                int total = win + lose + draw;
                Console.WriteLine($"現在の成績 - 勝ち: {win}, 負け: {lose}, あいこ: {draw}, 機械勝率: {(double)win / total * 100:0.00}%");
            }
        }

        // ユーザーの次の手を予想する
        private static int PredictUserMove(int[] userMoves, int[,] transition, int lastUser, Random rand)
        {
            if (lastUser != 0)
            {
                int max = 0;
                int best = 0;
                bool tie = false;
                for (int i = 1; i <= 3; i++)
                {
                    int c = transition[lastUser, i];
                    if (c > max)
                    {
                        max = c;
                        best = i;
                        tie = false;
                    }
                    else if (c == max)
                    {
                        tie = true;
                    }
                }
                if (max > 0 && !tie)
                {
                    return best;
                }
            }

            int overallMax = 0;
            int overallBest = 0;
            bool overallTie = false;
            for (int i = 1; i <= 3; i++)
            {
                int c = userMoves[i];
                if (c > overallMax)
                {
                    overallMax = c;
                    overallBest = i;
                    overallTie = false;
                }
                else if (c == overallMax)
                {
                    overallTie = true;
                }
            }
            if (overallMax > 0 && !overallTie)
            {
                return overallBest;
            }

            // 読みきれない場合はランダム
            return rand.Next(1, 4);
        }

        // ユーザーの手に勝つ手を返す
        private static int GetWinningMove(int userMove)
        {
            return userMove % 3 + 1; // 1->2, 2->3, 3->1
        }

        // 手番号を日本語表記に変換
        private static string ToHandString(int move)
        {
            return move switch
            {
                1 => "グー",
                2 => "チョキ",
                3 => "パー",
                _ => "?"
            };
        }

        // 現在学習しているユーザーの傾向を表示
        private static void ShowStatus(int[] userMoves, int[,] transition, int lastUser)
        {
            Console.WriteLine("ユーザーの手の傾向:");
            Console.WriteLine($"グー: {userMoves[1]}回, チョキ: {userMoves[2]}回, パー: {userMoves[3]}回");
            if (lastUser != 0)
            {
                int next = PredictUserMove(userMoves, transition, lastUser, new Random());
                Console.WriteLine($"直前の手が{ToHandString(lastUser)}のとき、次に出しそうな手: {ToHandString(next)}");
            }
            else
            {
                int next = PredictUserMove(userMoves, transition, lastUser, new Random());
                Console.WriteLine($"次に出しそうな手の予想: {ToHandString(next)}");
            }
        }
    }
}

