using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;

namespace Thesis.UCI
{
    internal class UCIProtocol
    {
        private static Thread searchThread;
        private static GameStateStack stack;
        private static MoveTree? tree;
        private static bool searchPerformed;
        private static string bestMove;

        public static void UCILoop()
        {
            bestMove = "";
            searchPerformed = false;
            stack = new GameStateStack();
            string engineName = "Dluhos Thesis Engine";
            string author = "Matej Dluhos";
            string input;
            while (true)
            {
                input = Console.ReadLine().Trim();
                if (input == null || input.StartsWith('\n'))
                {
                    continue;
                }
                if (input == "uci")
                {
                    Console.WriteLine("id name " + engineName);
                    Console.WriteLine("id author " + author);
                    Console.WriteLine("uciok");
                }
                else if (input == "isready")
                {
                    Console.WriteLine("readyok");
                }
                else if (input.StartsWith("position"))
                {
                    GameState state = new GameState();
                    stack.Push(state);
                    ParsePosition(stack, input);
                }
                else if (input.StartsWith("go"))
                {
                    Console.WriteLine($"bestmove {bestMove}");
                }
                else if (input == "stop")
                {
                    if (searchThread != null && searchThread.IsAlive)
                    {
                        searchThread.Join();
                    }
                }
                else if (input == "quit")
                {
                    return;
                }
                else if (input == "ucinewgame")
                {
                    stack.Clear();
                    tree = null;
                    bestMove = "";
                    searchPerformed = false;
                }
            }
        }

        //note: moves sent from the GUI are in GUI notation
        public static void ParsePosition(GameStateStack stack, string input)
        {
            GameState mainState = stack.Peek();
            string[] parts = input.Split(' ');
            string fen = string.Join(" ", parts.Skip(2).ToArray());
            if (parts[1] == "startpos")
            {
                mainState.SetStartPos();
            }
            else
            {
                mainState.LoadFEN(fen);
            }
            if (!searchPerformed)
            {
                searchThread = new Thread(new ThreadStart(Search));
                searchThread.Start();
                searchThread.Join();
                searchPerformed = true;
            }
            int basis = parts[1] == "startpos" ? 2 : 8;
            if (parts.Length > basis && parts[basis] == "moves")
            {
                //extract the move sequence to check against the MoveTree and get matching paths
                List<string> moveSequence = new List<string>(parts.Skip(basis + 1));
                if (moveSequence.Count > 0)
                {
                    List<List<string>> matchingPaths = FindMatchingPaths(moveSequence);
                    if (matchingPaths.Count > 0)
                    {
                        bestMove = Move.TranslateEngineToGUI(matchingPaths[0][moveSequence.Count]);
                        moveSequence.Add(bestMove);
                        matchingPaths = FindMatchingPaths(moveSequence);
                        MoveTree prunedTree = new MoveTree(0, matchingPaths);
                        prunedTree.PrintInfoString();
                    }
                    else
                    {
                        bestMove = "NONE";
                        tree.PrintInfoString();
                        if (tree.Paths.Count != 0)
                        {
                            Console.WriteLine("info string INCORRECT MOVE, RESTART THE POSITION.");
                        }
                    }
                }

            }
        }

        static void Search()
        {
            tree = Move.FindMate(stack, 5, 0, 1, true);
            searchPerformed = true;
            if (tree.Paths.Count != 0)
            {
                Evaluation.Evaluation.Evaluate(stack.Peek(), tree.Paths);
            }
        }

        private static List<List<string>> FindMatchingPaths(List<string> moveSequence)
        {
            List<List<string>> matchingPaths = new List<List<string>>();
            if (tree == null)
            {
                return matchingPaths;
            }
            foreach (List<string> path in tree.Paths)
            {
                bool matches = true;
                for (int i = 0; i < moveSequence.Count; i++)
                {
                    if (!moveSequence[i].Equals(Move.TranslateEngineToGUI(path[i])))
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches)
                {
                    matchingPaths.Add(path);
                }
            }
            //returns all matching paths with moves in ENGINE NOTATION
            return matchingPaths;
        }
    }
}
