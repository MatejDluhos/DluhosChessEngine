using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;
using Thesis.Pieces;

namespace Thesis.Evaluation
{
    internal class EvalPath
    {
        public List<string> Path { get; set; }
        //States[0] is the starting state, States[1] is the state after move 1, etc. ([5] is mate state)
        public List<GameState> States { get; set; }
        public List<EvalPosition> EvalPositions { get; set; }
        public double WLM { get; set; } = 0;

        public EvalPath()
        {
            Path = new List<string>();
            States = new List<GameState>();
            EvalPositions = new List<EvalPosition>();
        }

        public EvalPath(GameState startingState, List<string> path)
        {
            States = new List<GameState> { startingState };
            Path = path;
            EvalPositions = new List<EvalPosition>();
            CreateStatesFromPath();
            int stateCount = 0;
            foreach (string move in Path)
            {
                EvalPosition evalPosition = new EvalPosition(States[stateCount], States[stateCount + 1], move, stateCount + 1);
                EvalPositions.Add(evalPosition);
                stateCount++;
            }

        }

        public List<EvalPosition> Evaluate()
        {
            EvaluateSacrificeMaterial();
            foreach (EvalPosition evalPosition in EvalPositions)
            {
                evalPosition.Evaluate();
            }
            return EvalPositions;
        }

        public double Value()
        {
            double value = 0;
            foreach (EvalPosition evalPosition in EvalPositions)
            {
                value += evalPosition.Value();
            }
            return value;
        }

        private void CreateStatesFromPath()
        {
            GameStateStack stack = new GameStateStack();
            stack.Push(States.Last());
            for (int i = 0; i < 5; i++)
            {
                Move.MakeMove(Path[i], stack);
                States.Add(stack.Peek());
            }
        }


        public string ToTable()
        {
            string resString = "";
            string path;
            if (Path.Count > 0)
            {
                path = Move.TranslateEngineToGUI(Path[0]);
                for (int i = 1; i < Path.Count; i++)
                {
                    path += $" -> {Move.TranslateEngineToGUI(Path[i])}";
                }
            }
            else
            {
                path = "average of all possible move paths";
            }
            List<string> themes = new List<string>() { "SacrificeMaterial\t", "Fork\t\t\t", "Pin\t\t\t", "Skewer\t\t\t", "XRay\t\t\t",
                "DiscoveredAttack\t", "SmotheredMate\t\t", "CrossCheck\t\t", "Promotion\t\t", "Switchback\t\t" };
            resString += $"Path: {path}\n\n";
            resString += "Themes:\t\t\t| POS 1 | POS 2 | POS 3 | POS 4 | POS 5 | TOTAL |\n";
            resString += "-------------------------------------------------------------------------\n";
            foreach (string theme in themes)
            {
                double sum = 0;
                resString += $"{theme}|";
                for (int i = 0; i < 5; i++)
                {
                    resString += $" {EvalPositions[i].GetThemeValue(theme):F2}\t|";
                    sum += EvalPositions[i].GetThemeValue(theme);
                }
                resString += $" {sum:F2}\t|\n";
            }
            resString += $"\nWin With Less Material Bonus: {WLM:F0}\n\n";
            resString += $"TOTAL AESTHETICS SCORE: {(WLM + Value()):F2}\n";
            if (Path.Count > 0)
            {
                foreach (EvalPosition evalPosition in EvalPositions)
                {
                    resString += evalPosition.ToStringDoubleBoard();
                }
                resString += "--------------------------------------------------------------------------\n\n";

            }
            return resString;
        }

        public void EvaluateSwitchback()
        {
            List<EvalPosition> switchback = FindSwitchback(Path);
            if (switchback.Count > 0)
            {
                switchback[0].Switchback = switchback[0].AnalyzeSwitchback();
                switchback[1].Switchback = switchback[1].AnalyzeSwitchback();
            }
        }

        private List<EvalPosition> FindSwitchback(List<string> path)
        {
            string from, to;
            EvalPosition fromPos, toPos;
            for (int j = 0; j < 3; j += 2)
            {
                from = Move.TranslateEngineToGUI(path[j]).Substring(0, 2);
                to = Move.TranslateEngineToGUI(path[j]).Substring(2, 2);
                fromPos = EvalPositions[j];
                for (int i = 2; i < 5; i += 2)
                {
                    if (i == 2 && j == 0)
                    {
                        continue;
                    }
                    toPos = EvalPositions[i];
                    if (Move.TranslateEngineToGUI(path[i]).Substring(0, 2) == to && Move.TranslateEngineToGUI(path[i]).Substring(2, 2) == from)
                    {
                        return new List<EvalPosition> { fromPos, toPos };
                    }
                }
            }
            from = Move.TranslateEngineToGUI(path[1]).Substring(0, 2);
            to = Move.TranslateEngineToGUI(path[1]).Substring(2, 2);
            fromPos = EvalPositions[1];
            toPos = EvalPositions[3];
            if (Move.TranslateEngineToGUI(path[3]).Substring(0, 2) == to && Move.TranslateEngineToGUI(path[3]).Substring(2, 2) == from)
            {
                return new List<EvalPosition> { fromPos, toPos };
            }
            return new List<EvalPosition>();
        }

        public void EvaluateSacrificeMaterial()
        {
            for (int i = 1; i <= 3; i += 2)
            {
                if (Move.IsCapture(Path[i]))
                {
                    string translatedMove = Move.TranslateEngineToGUI(Path[i]);
                    Piece sacrifice = States[i].Board.GetTile(translatedMove[2], int.Parse(translatedMove.Substring(3, 1))).PieceOnTile;
                    double sacrificeValue = Evaluation.GetPieceValue(sacrifice);
                    double recaptureValue = 0;
                    if (Move.IsCapture(Path[i+1]))
                    {
                        string translatedMove2 = Move.TranslateEngineToGUI(Path[i+1]);
                        Piece recapture = States[i+1].Board.GetTile(translatedMove2[2], int.Parse(translatedMove2.Substring(3, 1))).PieceOnTile;
                        recaptureValue = Evaluation.GetPieceValue(recapture);
                    }
                    if (sacrificeValue > recaptureValue)
                    {
                        EvalPositions[i].SacrificeMaterial = 5;
                        PositionDetails detail = new PositionDetails();
                        detail.Theme = "SacrificeMaterial";
                        detail.InvolvedPieces.Add(sacrifice);
                        EvalPositions[i].Details.Add(detail);
                    }
                    
                }
            }
        }
    }
}
