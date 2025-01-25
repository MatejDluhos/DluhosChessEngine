using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Data
{
    public class MoveTree
    {
        public int Evaluation { get; set; }
        public List<List<string>> Paths { get; set; }

        public MoveTree(int evaluation, List<List<string>> paths)
        {
            Evaluation = evaluation;
            Paths = paths;
        }

        public bool IsEmpty()
        {
            return Paths.Count == 0;
        }

        public void PrintInfoString()
        {
            if (Paths.Count == 0)
            {
                Console.WriteLine("info string NO POSSIBLE MATE DETECTED IN THIS POSITION.");
                return;
            }
            foreach (List<string> path in Paths)
            {
                string temp = "info string ";
                foreach (string move in path)
                {
                    string translatedMove = Move.TranslateEngineToGUI(move);
                    if (move == path[4])
                    {
                        temp += translatedMove;
                    }
                    else
                    {
                        temp += translatedMove + " | ";
                    }
                }
                Console.WriteLine(temp);
            }
        }

        public override string ToString()
        {
            string result = "";
            result += "POSSIBLE MOVE PATHS:\n\n";
            result += "MOVE1 | MOVE2 | MOVE3 | MOVE4 | MOVE5\n";
            result += "-------------------------------------\n";
            foreach (List<string> path in Paths)
            {
                foreach (string move in path)
                {
                    if (move == path[4])
                    {
                        result += move;
                    }
                    else
                    {
                        result += move + " | ";
                    }
                }
                result += "\n";
            }
            return result;
        }
    }
}
