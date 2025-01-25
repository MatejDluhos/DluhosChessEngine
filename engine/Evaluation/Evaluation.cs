using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;
using Thesis.Pieces;

namespace Thesis.Evaluation
{
    internal class Evaluation
    {
        /*
        -evaluates the easthetics of the mate in 3 position
        -the output is a file with the evaluation of the main path and a file with the evaluation of all paths put in average
        -evaluation themes are: 1) win with less material
                                2) sacrifice material
                                3) fork
                                4) pin
                                5) skewer
                                6) x-ray
                                7) discovered attack
                                8) smothered mate - knight checkmate with the king surrounded by his own pieces
                                9) cross-check - a check played in response to a check
                                10) promotion
                                11) switchback - a move that returns a piece to a square it has previously occupied
        */

        //Evaluates all paths and returns them.
        public static List<EvalPath> Evaluate(GameState startingState, List<List<string>> paths)
        {
            List<EvalPath> evalPaths = new List<EvalPath>();
            double WLM = EvaluateWinWithLessMaterial(startingState);
            foreach (List<string> path in paths)
            {
                EvalPath evalPath = new EvalPath(startingState, path);
                evalPath.EvaluateSwitchback();
                evalPath.Evaluate();
                evalPath.WLM = WLM;
                evalPaths.Add(evalPath);
            }
            EvalPath? averagePath = null;
            if (evalPaths.Count > 1) 
            {
                averagePath = CreateAveragePath(evalPaths);
            }
            EvalPath? mainPath = evalPaths[0];

            File.WriteAllText("eval.txt", GenerateFileContent(mainPath, averagePath));
            return evalPaths;
        }

        private static EvalPath CreateAveragePath(List<EvalPath> evalPaths)
        {
            EvalPath averagePath = new EvalPath();
            for (int i = 0; i < 5; i++)
            {
                averagePath.EvalPositions.Add(new EvalPosition());
            }
            int pathCount = evalPaths.Count;
            for (int i = 0; i < pathCount; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    averagePath.EvalPositions[j].SacrificeMaterial += evalPaths[i].EvalPositions[j].SacrificeMaterial;
                    averagePath.EvalPositions[j].Fork += evalPaths[i].EvalPositions[j].Fork;
                    averagePath.EvalPositions[j].Pin += evalPaths[i].EvalPositions[j].Pin;
                    averagePath.EvalPositions[j].Skewer += evalPaths[i].EvalPositions[j].Skewer;
                    averagePath.EvalPositions[j].XRay += evalPaths[i].EvalPositions[j].XRay;
                    averagePath.EvalPositions[j].DiscoveredAttack += evalPaths[i].EvalPositions[j].DiscoveredAttack;
                    averagePath.EvalPositions[j].SmotheredMate += evalPaths[i].EvalPositions[j].SmotheredMate;
                    averagePath.EvalPositions[j].CrossCheck += evalPaths[i].EvalPositions[j].CrossCheck;
                    averagePath.EvalPositions[j].Promotion += evalPaths[i].EvalPositions[j].Promotion;
                    averagePath.EvalPositions[j].Switchback += evalPaths[i].EvalPositions[j].Switchback;
                }
            }
            for (int i = 0; i < 5; i++)
            {
                averagePath.EvalPositions[i].SacrificeMaterial /= pathCount;
                averagePath.EvalPositions[i].Fork /= pathCount;
                averagePath.EvalPositions[i].Pin /= pathCount;
                averagePath.EvalPositions[i].Skewer /= pathCount;
                averagePath.EvalPositions[i].XRay /= pathCount;
                averagePath.EvalPositions[i].DiscoveredAttack /= pathCount;
                averagePath.EvalPositions[i].SmotheredMate /= pathCount;
                averagePath.EvalPositions[i].CrossCheck /= pathCount;
                averagePath.EvalPositions[i].Promotion /= pathCount;
                averagePath.EvalPositions[i].Switchback /= pathCount;
            }
            return averagePath;
        }


        //Generates the content of the file that will contain the evaluation of the main or average path.
        private static string GenerateFileContent(EvalPath mainPath, EvalPath? averagePath)
        {
            string resString = "AESTHETICS EVALUATION\n";
            if (averagePath != null)
            {
                resString += averagePath.ToTable();
            }
            resString += "\n\n";
            resString += mainPath.ToTable();
            return resString;
        }

        //returns the material difference between the two players if the winning player has less material, otherwise 0.
        private static double EvaluateWinWithLessMaterial(GameState startingState)
        {
            string winningColor = startingState.ToMove == 'w' ? "white" : "black";
            double whiteMaterial = 0;
            double blackMaterial = 0;

            foreach (Piece piece in startingState.Board.WhitePieces)
            {
                whiteMaterial += GetPieceValue(piece);
            }
            whiteMaterial -= 10;
            foreach (Piece piece in startingState.Board.BlackPieces)
            {
                blackMaterial += GetPieceValue(piece);
            }
            blackMaterial -= 10;
            double res;
            if (winningColor == "white")
            {
                res = blackMaterial - whiteMaterial;
            }
            else
            {
                res = whiteMaterial - blackMaterial;
            }
            if (res > 0)
            {
                return res;
            }
            else
            {
                return 0;
            }
        }

        public static double GetPieceValue(Piece piece)
        {
            switch (piece)
            {
                case Pawn :
                    return 1;
                case Knight :
                    return 3;
                case Bishop :
                    return 3;
                case Rook :
                    return 5;
                case Queen :
                    return 9;
                default:
                    return 10;
            }
        }

        public static string BeforeAfterBoardsToString(GameState beforeState, GameState afterState)
        {
            string resString = "";
            ChessBoard beforeBoard = beforeState.Board;
            ChessBoard afterBoard = afterState.Board;
            resString += "\t    A B C D E F G H                     A B C D E F G H\n\n";
            int rowIndex = 8;
            for (int row = 8; row > 0; row--)
            {
                resString += "\t" + rowIndex + "   ";
                for (int col = 'a'; col <= 'h'; col++)
                {
                    
                    if (beforeBoard.GetTile((char)col, row).PieceOnTile == null)
                    {
                        resString += ". ";
                    }
                    else
                    {
                        resString += $"{beforeBoard.GetTile((char)col, row).PieceOnTile.GetSymbol()} ";
                    }
                }
                if (rowIndex == 5 || rowIndex == 4)
                {
                    resString += "     → → →      ";
                }
                else
                {
                    resString += "                ";
                }
                resString += rowIndex + "   ";
                for (int col = 'a'; col <= 'h'; col++)
                {
                    
                    if (afterBoard.GetTile((char)col, row).PieceOnTile == null)
                    {
                        resString += ". ";
                    }
                    else
                    {
                        resString += $"{afterBoard.GetTile((char)col, row).PieceOnTile.GetSymbol()} ";
                    }
                    if (col == 'h')
                    {
                        resString += "\n";
                    }
                }
                rowIndex--;
            }
            resString += "\n\n";
            return resString;
        }
    }
}
