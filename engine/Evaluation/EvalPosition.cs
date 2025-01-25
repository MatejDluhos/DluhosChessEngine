using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;
using Thesis.Pieces;

namespace Thesis.Evaluation
{
    internal class EvalPosition
    {
        public double SacrificeMaterial { get; set; } = 0;      //eval on move 1, 2, 3, 4
        public double Fork { get; set; } = 0;                   //eval on move 1, 2, 3, 4
        public double Pin { get; set; } = 0;                    //eval on move 1, 2, 3, 4
        public double Skewer { get; set; } = 0;                 //eval on move 1, 2, 3, 4
        public double XRay { get; set; } = 0;                   //eval on move 1, 2, 3, 4
        public double DiscoveredAttack { get; set; } = 0;       //eval on move 1, 2, 3, 4
        public double SmotheredMate { get; set; } = 0;          //eval on move 5
        public double CrossCheck { get; set; } = 0;             //eval on move 1, 2, 3, 4
        public double Promotion { get; set; } = 0;              //eval on move 1, 2, 3, 4
        public double Switchback { get; set; } = 0;             
        public GameState BeforeState { get; set; }
        public GameState AfterState { get; set; }
        public string Move { get; set; }
        //PosNumber starts at 1
        public int PosNumber { get; set; }
        public List<PositionDetails> Details { get; set; }

        public EvalPosition()
        {
            BeforeState = new GameState();
            AfterState = new GameState();
            Move = "";
            PosNumber = 0;
            Details = new List<PositionDetails>();
        }

        public EvalPosition(GameState beforeState, GameState afterState, string move, int posNumber)
        {
            BeforeState = beforeState;
            AfterState = afterState;
            Move = move;
            PosNumber = posNumber;
            Details = new List<PositionDetails>();
        }

        public double Value()
        {
            return SacrificeMaterial + Fork + Pin + Skewer + XRay + DiscoveredAttack +
                   SmotheredMate + CrossCheck + Promotion + Switchback;
        }

        public double GetThemeValue(string theme)
        {
            theme = theme.Trim();
            switch (theme)
            {
                case "SacrificeMaterial":
                    return SacrificeMaterial;
                case "Fork":
                    return Fork;
                case "Pin":
                    return Pin;
                case "Skewer":
                    return Skewer;
                case "XRay":
                    return XRay;
                case "DiscoveredAttack":
                    return DiscoveredAttack;
                case "SmotheredMate":
                    return SmotheredMate;
                case "CrossCheck":
                    return CrossCheck;
                case "Promotion":
                    return Promotion;
                case "Switchback":
                    return Switchback;
                default:
                    return 0;
            }
        }

        public string ToStringDoubleBoard()
        {
            string resString = "";
            resString += "--------------------------------------------------------------------------\n";
            resString += $"\n\t\t\t\t {Data.Move.TranslateEngineToGUI(Move)}\n";
            resString += Evaluation.BeforeAfterBoardsToString(BeforeState, AfterState);
            foreach (PositionDetails detail in Details)
            {
                resString += detail.GetDetails();
            }
            return resString;
        }

        public void Evaluate()
        {
            if (PosNumber == 5)
            {
                EvaluateSmotheredMate();
                return;
            }
            else
            {
                EvaluateFork();
                EvaluatePinSkewerXRay();
                EvaluateDiscoveredAttack();
                EvaluateCrossCheck();
                EvaluatePromotion();
            }
        }

        private void EvaluateFork()
        {
            string translatedMove = Data.Move.TranslateEngineToGUI(Move);
            Piece forkingPiece = AfterState.Board.GetTile(translatedMove[2], int.Parse(translatedMove.Substring(3, 1))).PieceOnTile;
            List<Piece> forkCandidates = forkingPiece.Color == "white" ? AfterState.Board.BlackPieces : AfterState.Board.WhitePieces;
            List<Piece> forkedPieces = new List<Piece>();
            foreach (Piece candidate in forkCandidates)
            {
                if (forkingPiece.AttackMap[candidate.File, candidate.Rank])
                {
                    forkedPieces.Add(candidate);
                }
            }
            if (forkedPieces.Count > 1)
            {
                if (forkingPiece.GetType() == typeof(Knight))
                {
                    Fork = 8;
                }
                else if (forkingPiece.GetType() == typeof(Pawn))
                {
                    Fork = 7;
                }
                else
                {
                    Fork = 6;
                }
                foreach (Piece _ in forkedPieces.Skip(2))
                {
                    Fork += 2;
                }
                PositionDetails detail = new PositionDetails();
                detail.Theme = "Fork";
                detail.InvolvedPieces.Add(forkingPiece);
                foreach (Piece piece in forkedPieces)
                {
                    detail.InvolvedPieces.Add(piece);
                }
                Details.Add(detail);
            }
        }

        private void EvaluatePinSkewerXRay()
        {
            List<List<Piece>> piecesInLineBefore = BeforeState.FindPiecesInLine();
            List<List<Piece>> piecesInLineAfter = AfterState.FindPiecesInLine();
            List<List<Piece>> newLines = new List<List<Piece>>();
            foreach (List<Piece> lineAfter in piecesInLineAfter)
            {
                bool contains = false;
                foreach (List<Piece> lineBefore in piecesInLineBefore)
                {
                    if (lineAfter[0] == lineBefore[0] && lineAfter[1] == lineBefore[1] && lineAfter[2] == lineBefore[2])
                    {
                        contains = true;
                        break;
                    }
                }
                if (!contains)
                {
                    newLines.Add(lineAfter);
                }
            }
            foreach (List<Piece> line in newLines)
            {
                PositionDetails detail = new PositionDetails();
                if (line[0].Color != line[1].Color && line[0].Color == line[2].Color)
                {
                    if (line[1].AttackMap[line[2].File, line[2].Rank] && line[2].GetType() != typeof(King))
                    {
                        XRay += 5;
                        detail.Theme = "XRay";
                    }
                }
                else if (line[0].Color != line[1].Color && line[0].Color != line[2].Color)
                {
                    double closerPieceValue = Evaluation.GetPieceValue(line[1]);
                    double furtherPieceValue = Evaluation.GetPieceValue(line[2]);
                    if (closerPieceValue < furtherPieceValue)
                    {
                        if (line[1].GetType() != typeof(King))
                        {
                            if (furtherPieceValue == 10)
                            {
                                Pin += 10;
                            }
                            else if (furtherPieceValue == 5 || furtherPieceValue == 3)
                            {
                                Pin += 5;
                            }
                            else
                            {
                                Pin += 2;
                            }
                            detail.Theme = "Pin";
                        }
                    }
                    if (closerPieceValue >= furtherPieceValue || line[1].GetType() == typeof(King))
                    {
                        if (closerPieceValue == 10)
                        {
                            Skewer += 10;
                        }
                        else if (furtherPieceValue == 5 || furtherPieceValue == 3)
                        {
                            Skewer += 5;
                        }
                        else
                        {
                            Skewer += 2;
                        }
                        detail.Theme = "Skewer";
                    }
                }
                if (detail.Theme != "")
                {
                    detail.InvolvedPieces.Add(line[0]);
                    detail.InvolvedPieces.Add(line[1]);
                    detail.InvolvedPieces.Add(line[2]);
                    Details.Add(detail);
                }
            }
        }

        private void EvaluateDiscoveredAttack()
        {
            string translatedMove = Data.Move.TranslateEngineToGUI(Move);
            string color = BeforeState.Board.GetTile(translatedMove[0], int.Parse(translatedMove.Substring(1, 1))).PieceOnTile.Color;
            List<Piece> beforePieces = color == "white" ? BeforeState.Board.WhitePieces : BeforeState.Board.BlackPieces;
            List<Piece> afterPieces = color == "white" ? AfterState.Board.WhitePieces : AfterState.Board.BlackPieces;
            List<Piece> beforeCandidates = new List<Piece>();
            List<Piece> afterCandidates = new List<Piece>();
            foreach (Piece piece in beforePieces)
            {
                if (piece.GetType() == typeof(Bishop) || piece.GetType() == typeof(Rook) || piece.GetType() == typeof(Queen))
                {
                    beforeCandidates.Add(piece);
                }
            }
            foreach (Piece piece in afterPieces)
            {
                if (piece.GetType() == typeof(Bishop) || piece.GetType() == typeof(Rook) || piece.GetType() == typeof(Queen))
                {
                    afterCandidates.Add(piece);
                }
            }
            foreach (Piece beforeCandidate in beforeCandidates)
            {
                foreach (Piece afterCandidate in afterCandidates)
                {
                    if (beforeCandidate == afterCandidate)
                    {
                        List<Piece> beforeCaptures = beforeCandidate.FindCapturablePieces();
                        List<Piece> afterCaptures = afterCandidate.FindCapturablePieces();
                         
                        foreach (Piece afterCapture in afterCaptures)
                        {
                            bool found = false;
                            foreach (Piece beforeCapture in beforeCaptures)
                            {
                                if (afterCapture == beforeCapture)
                                {
                                    found = true;
                                    break;
                                }
                            }
                            if (!found)
                            {
                                DiscoveredAttack += 5;
                                PositionDetails detail = new PositionDetails();
                                detail.Theme = "DiscoveredAttack";
                                detail.InvolvedPieces.Add(beforeCandidate);
                                detail.InvolvedPieces.Add(afterCapture);
                                Details.Add(detail);
                                break;
                            }
                        }
                    }
                }
            }
            

        }

        private void EvaluateSmotheredMate()
        {
            string translatedMove = Data.Move.TranslateEngineToGUI(Move);
            Piece matingPiece = AfterState.Board.GetTile(translatedMove[2], int.Parse(translatedMove.Substring(3, 1))).PieceOnTile;
            if (PosNumber == 5 && matingPiece.GetType() == typeof(Knight))
            {
                Piece king = null;
                List<Piece> kingCandidates = matingPiece.Color == "white" ? AfterState.Board.BlackPieces : AfterState.Board.WhitePieces;
                foreach (Piece piece in kingCandidates)
                {
                    if (piece.GetType() == typeof(King))
                    {
                        king = piece;
                        break;
                    }
                }
                for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
                {
                    for (int rankOffset = -1; rankOffset <= 1; rankOffset++)
                    {
                        if (fileOffset == 0 && rankOffset == 0)
                        {
                            continue;
                        }
                        if (king.File + fileOffset < 'a' || king.File + fileOffset > 'h' || king.Rank + rankOffset < 1 || king.Rank + rankOffset > 8)
                        {
                            continue;
                        }
                        ChessBoard.Tile? searchedTile = AfterState.Board.GetTile((char)(king.File + fileOffset), king.Rank + rankOffset);
                        if (searchedTile != null && (searchedTile.PieceOnTile == null || searchedTile.PieceOnTile.Color != king.Color))
                        {
                            return;
                        }
                    }
                }
                SmotheredMate += 10;
                PositionDetails detail = new PositionDetails();
                detail.Theme = "SmotheredMate";
                detail.InvolvedPieces = new List<Piece>
                {
                    matingPiece
                };
                Details.Add(detail);
            }
        }

        //10 points for cross check (answering a check with a check).
        private void EvaluateCrossCheck()
        {
            if (BeforeState.IsCheck() && AfterState.IsCheck())
            {
                CrossCheck += 10;
                PositionDetails detail = new PositionDetails();
                detail.Theme = "CrossCheck";
                detail.InvolvedPieces = new List<Piece>();
                string translatedMove = Data.Move.TranslateEngineToGUI(Move);
                detail.InvolvedPieces.Add(AfterState.Board.GetTile(translatedMove[2], int.Parse(translatedMove.Substring(3, 1))).PieceOnTile);
                Details.Add(detail);
            }
        }

        //10 points for promotion, 5 additional points for underpromotion.
        private void EvaluatePromotion()
        {
            if (!char.IsDigit(Move[1]))
            {
                Promotion += 10;
                if (Move[3] != 'Q' || Move[3] != 'q')
                {
                    Promotion += 5;
                }
                PositionDetails detail = new PositionDetails();
                detail.Theme = "Promotion";
                detail.InvolvedPieces = new List<Piece>();
                string translatedMove = Data.Move.TranslateEngineToGUI(Move);
                detail.InvolvedPieces.Add(BeforeState.Board.GetTile(translatedMove[0], int.Parse(translatedMove.Substring(1, 1))).PieceOnTile);
                detail.InvolvedPieces.Add(AfterState.Board.GetTile(translatedMove[2], int.Parse(translatedMove.Substring(3, 1))).PieceOnTile);
                Details.Add(detail);
            }
        }

        //Evaluates either the first or the second position of the switchback.
        //Adds points for accomplishments off the move (check / threat / capture).
        public double AnalyzeSwitchback()
        {
            double switchbackValue = 0;
            string TranslatedMove = Data.Move.TranslateEngineToGUI(Move);
            ChessBoard.Tile toTile = AfterState.Board.GetTile(TranslatedMove[2], int.Parse(TranslatedMove.Substring(3, 1)));
            if (AfterState.IsCheck())
            {
                switchbackValue += 2;
            }
            if (Data.Move.IsCapture(Move))
            {
                switchbackValue += 2;
            }
            if (toTile.PieceOnTile.FindCapturablePieces().Count() > 0)
            {
                foreach (Piece piece in toTile.PieceOnTile.FindCapturablePieces())
                {
                    switchbackValue += 1;
                }
            }
            if (switchbackValue != 0)
            {
                switchbackValue += 2;
            }
            return switchbackValue;
        }
    }


}
