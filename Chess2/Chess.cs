using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess2
{
    static class Chess
    {
        public enum Figure : byte
        {
            P = 10,
            p = 11,
            K = 100,
            k = 101,
            Q = 90,
            q = 91,
            N = 30,
            n = 31,
            B = 40,
            b = 41,
            R = 50,
            r = 51,
            Null = 0
        }

            public static int DEPTH = 25;
            public static char[,] board = new char[8,8];
            public static bool Turn;//true - White, false - Black
            public static int cellSize { get; set; }
        
        //  rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1 //
        public static string FEN { get; set; } = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

        public static void ParseFEN()
        {
            board = new char[8, 8];
            var fenSplit = FEN.Split(' ');
            var pieces = fenSplit[0].Split('/');
            int boardline=0;
            foreach (var line in pieces)
            {
                int index=0;
                for (int i = 0; i < line.Length; i++)
                {
                    if (char.IsDigit(line[i]))
                    {
                        index += line[i]-49;
                    }
                    else
                    {
                        board[boardline,i+index] = line[i];
                    }
                }

                boardline++;
            }

            Turn = fenSplit[1] == "w";

        }
        public static string ChessAppDir { get; set; }
        public static string EngineName { get; set; }
        public static string ChessGUIName { get; set; }
        public static Process EngineProcess { get; set; }

        public static void Send(string cmd)
        {
            EngineProcess.StandardInput.WriteLine(cmd);
        }
    }
}
