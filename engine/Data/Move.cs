using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Thesis.Pieces;

namespace Thesis.Data
{
    internal class Move
    {
        //translates the move string from GUI to engine format
        public static string TranslateGUIToEngine(string move, GameState state)
        {
            string from = move.Substring(0, 2);
            string to = move.Substring(2, 2);
            char promotion = 'x';
            if (move.Length == 5)
            {
                promotion = move[4];
            }
            if (promotion == 'x')
            {
                //check if move is castling
                if (from == "e1" || from == "e8")
                {
                    if (to == "g1" || to == "g8")
                    {
                        return from + to + "S";
                    }
                    else if (to == "c1" || to == "c8")
                    {
                        return from + to + "L";
                    }
                }
                //check if move is en passant
                if (state.EnPassant != "-")
                {
                    if (to == state.EnPassant)
                    {
                        return from + to + 'E';
                    }
                }
                ChessBoard.Tile toTile = state.Board.GetTile(to[0], int.Parse(to.Substring(1)));
                if (toTile.PieceOnTile != null)
                {
                    return from + to + toTile.PieceOnTile.GetSymbol().ToString();
                }
                //basic move
                return from + to + " ";
            }
            else
            {
                ChessBoard.Tile? toTile = state.Board.GetTile(to[0], int.Parse(to.Substring(1)));
                if (toTile.PieceOnTile != null)
                {
                    return from[0].ToString() + to[0].ToString() + toTile.PieceOnTile.GetSymbol().ToString()
                        + promotion.ToString() + 'P';
                }
                else
                {
                    return from[0].ToString() + to[0].ToString() + ' ' + promotion.ToString() + 'P';
                }
            }
        }

        //translates the move string from engine to GUI format
        public static string TranslateEngineToGUI(string move)
        {
            if (move[4] == 'P' && !char.IsDigit(move[1]))
            {
                string color = char.IsUpper(move[3]) ? "white" : "black";
                string fromFile = move.Substring(0, 1);
                string toFile = move.Substring(1, 1);
                string fromRank = color == "white" ? "7" : "2";
                string toRank = color == "white" ? "8" : "1";
                char promotion = char.ToLower(char.Parse(move.Substring(3, 1)));
                return fromFile + fromRank + toFile + toRank + promotion;
            }
            else
            {
                return move.Substring(0, 4);
            }
        }

        //makes a new GameState representing the position after the move and pushes it to the stack
        //does not check the validity of the move (move generation by PossibleMoves does that)
        public static void MakeMove(string move, GameStateStack stack)
        {
            GameState oldState = stack.Peek();
            GameState resultState = oldState.Copy();
            
            if (move[4] == 'E')
            {
                MakeMoveEnPassant(move, resultState);
            }
            else if (move[4] == 'S' || move[4] == 'L')
            {
                MakeMoveCastling(move, resultState);
            }
            else if (!char.IsDigit(move[1]))
            {
                MakeMovePromotion(move, resultState);
            }
            else
            {
                MakeMoveNormal(move, resultState);
            }
            if (resultState.ToMove == 'b')
            {
                resultState.FullMoves++;
            }
            resultState.ToMove = resultState.ToMove == 'w' ? 'b' : 'w';
            stack.Push(resultState);
        }

        //handles en passant moves
        //updates the halfmove counter and en passant square
        private static void MakeMoveEnPassant(string move, GameState state)
        {
            ChessBoard.Tile fromTile = state.Board.GetTile(move[0], int.Parse(move.Substring(1, 1)));
            ChessBoard.Tile toTile = state.Board.GetTile(move[2], int.Parse(move.Substring(3, 1)));
            ChessBoard.Tile capturedTile = fromTile.PieceOnTile.Color == "white"
                ? state.Board.GetTile(move[2], int.Parse(move.Substring(3, 1)) - 1)
                : state.Board.GetTile(move[2], int.Parse(move.Substring(3, 1)) + 1);
            MovePiece(fromTile, toTile);
            capturedTile.PieceOnTile.Board.RemovePiece(capturedTile.PieceOnTile.File, capturedTile.PieceOnTile.Rank);
            state.EnPassant = "-";
            state.HalfMoves = 0;
        }

        //handles castling moves
        //updates the halfmove counter and en passant
        private static void MakeMoveCastling(string move, GameState state)
        {
            ChessBoard.Tile kingFromTile = state.Board.GetTile(move[0], int.Parse(move.Substring(1, 1)));
            ChessBoard.Tile kingToTile = state.Board.GetTile(move[2], int.Parse(move.Substring(3, 1)));
            ChessBoard.Tile rookFromTile;
            ChessBoard.Tile rookToTile;
            if (move[4] == 'S')
            {
                rookFromTile = state.Board.GetTile('h', kingFromTile.Rank);
                rookToTile = state.Board.GetTile('f', kingFromTile.Rank);
            }
            else
            {
                rookFromTile = state.Board.GetTile('a', kingFromTile.Rank);
                rookToTile = state.Board.GetTile('d', kingFromTile.Rank);
            }
            MovePiece(kingFromTile, kingToTile);
            MovePiece(rookFromTile, rookToTile);
            state.HalfMoves++;
            state.EnPassant = "-";
            Castling.UpdateCastling(kingToTile.PieceOnTile.Color, state);
        }

        //handles promotion moves
        //updates the halfmove counter and en passant square
        private static void MakeMovePromotion(string move, GameState state)
        {
            int fromRank = state.ToMove == 'w' ? 7 : 2;
            int toRank = state.ToMove == 'w' ? 8 : 1;
            char fromFile = move[0];
            char toFile = move[1];
            state.Board.RemovePiece(fromFile, fromRank);
            if (move[2] != ' ')
            {
                state.Board.RemovePiece(toFile, toRank);
            }
            Piece pieceToAdd = Piece.CreatePieceBySymbol(move[3], toFile, toRank, state.Board);
            state.Board.SetTile(toFile, toRank, pieceToAdd);
            state.HalfMoves = 0;
            state.EnPassant = "-";
        }

        //handles a basic move (not en passant, castling or promotion)
        //updates the halfmove counter and en passant square if needed
        private static void MakeMoveNormal(string move, GameState state)
        {
            ChessBoard.Tile fromTile = state.Board.GetTile(move[0], int.Parse(move.Substring(1, 1)));
            ChessBoard.Tile toTile = state.Board.GetTile(move[2], int.Parse(move.Substring(3, 1)));
            Castling.CheckCastlingUpdates(move, state);
            if (toTile.PieceOnTile != null)
            {
                CapturePiece(fromTile, toTile);
                state.HalfMoves = 0;
                state.EnPassant = "-";
            }
            else
            {
                if (IsPawnMove(move, state.Board))
                {
                    state.HalfMoves = 0;
                    if (Math.Abs(fromTile.Rank - toTile.Rank) == 2)
                    {
                        ChessBoard.Tile? enPassantTileLeft = state.Board.GetTile((char)(toTile.File - 1), toTile.Rank);
                        ChessBoard.Tile? enPassantTileRight = state.Board.GetTile((char)(toTile.File + 1), toTile.Rank);
                        foreach (ChessBoard.Tile? enPassantTile in new[] { enPassantTileLeft, enPassantTileRight })
                        {
                            if (enPassantTile != null && enPassantTile.PieceOnTile != null && enPassantTile.PieceOnTile is Pawn)
                            {
                                string color = fromTile.PieceOnTile.Color == "white" ? "black" : "white";
                                if (enPassantTile.PieceOnTile.Color == color)
                                {
                                    char resFile = toTile.File;
                                    int resRank = toTile.Rank + (fromTile.PieceOnTile.Color == "white" ? -1 : 1);
                                    state.EnPassant = resFile.ToString() + resRank.ToString();
                                }
                            }
                        }
                    }
                    else
                    {
                        state.EnPassant = "-";
                    }
                }
                else
                {
                    state.HalfMoves++;
                    state.EnPassant = "-";
                }
                MovePiece(fromTile, toTile);
            }
        }

        //moves a piece from one tile to another
        //includes updating the attack maps of all affected pieces
        //meant as a helper function for MakeMove
        public static void MovePiece(ChessBoard.Tile fromTile, ChessBoard.Tile toTile)
        {
            toTile.PieceOnTile = fromTile.PieceOnTile;
            fromTile.PieceOnTile = null;
            toTile.PieceOnTile.File = toTile.File;
            toTile.PieceOnTile.Rank = toTile.Rank;
            toTile.PieceOnTile.UpdateAttackMap();
            List<Piece> toModify = toTile.Board.FindPiecesToModify(toTile.File, toTile.Rank);
            foreach (var piece in toModify)
            {
                piece.ModifyAttackMap(toTile.File, toTile.Rank, "dest");
            }
            toModify = fromTile.Board.FindPiecesToModify(fromTile.File, fromTile.Rank);
            foreach (var piece in toModify)
            {
                piece.ModifyAttackMap(fromTile.File, fromTile.Rank, "origin");
            }
        }

        //captures a piece and moves the capturing piece to the new tile
        //includes updating the attack maps of all affected pieces
        //meant as a helper function for the main MakeMove function
        public static void CapturePiece(ChessBoard.Tile fromTile, ChessBoard.Tile toTile)
        {
            Piece capturedPiece = toTile.PieceOnTile;
            Piece capturingPiece = fromTile.PieceOnTile;
            if (capturedPiece.Color == "white")
            {
                capturedPiece.Board.WhitePieces.Remove(capturedPiece);
            }
            else
            {
                capturedPiece.Board.BlackPieces.Remove(capturedPiece);
            }
            toTile.PieceOnTile = capturingPiece;
            fromTile.PieceOnTile = null;
            toTile.PieceOnTile.File = toTile.File;
            toTile.PieceOnTile.Rank = toTile.Rank;
            toTile.PieceOnTile.UpdateAttackMap();
            
            List<Piece> toModify = fromTile.Board.FindPiecesToModify(fromTile.File, fromTile.Rank);
            foreach (var piece in toModify)
            {
                piece.ModifyAttackMap(fromTile.File, fromTile.Rank, "origin");
            }
        }

        //Reverts the last move made on a stack
        public static void UndoMove(GameStateStack stack)
        {
            stack.Pop();
        }

        //checks if a move is a pawn move
        public static bool IsPawnMove(string move, ChessBoard board)
        {
            if (move[4] == 'P')
            {
                return true;
            }
            return board.GetTile(move[0], int.Parse(move.Substring(1, 1))).PieceOnTile is Pawn
                || board.GetTile(move[2], int.Parse(move.Substring(3, 1))).PieceOnTile is Pawn;
        }

        //checks if a move is a capture
        public static bool IsCapture(string move)
        {
            if (char.IsLetter(move[4]) && move[4] != 'S' && move[4] != 'L' && move[4] != 'P')
            {
                return true;
            }
            if (move[4] == 'P')
            {
                return char.IsLetter(move[2]);
            }
            return false;
        }
        
        public static MoveTree FindMate(GameStateStack stack, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            GameState position = stack.Peek();
            if (depth == 0)
            {
                if (position.IsCheckmate())
                {
                    return new MoveTree(1, new List<List<string>> { new List<string>() });
                }
                return new MoveTree(0, new List<List<string>>());
            }
            string possibleMoves = position.PossibleMoves();
            possibleMoves = OrderMoves(possibleMoves, position);

            if (maximizingPlayer)
            {
                MoveTree bestEval = new MoveTree(0, new List<List<string>>());
                for (int i = 0; i < possibleMoves.Length; i += 5)
                {
                    string move = possibleMoves.Substring(i, 5);
                    MakeMove(move, stack);
                    MoveTree eval = FindMate(stack, depth - 1, alpha, beta, false);
                    UndoMove(stack);

                    if (eval.Evaluation > bestEval.Evaluation)
                    {
                        bestEval.Evaluation = eval.Evaluation;
                        bestEval.Paths = new List<List<string>>();
                    }

                    if (eval.Evaluation == bestEval.Evaluation)
                    {
                        foreach (var path in eval.Paths)
                        {
                            var newPath = new List<string> { move };
                            newPath.AddRange(path);
                            bestEval.Paths.Add(newPath);
                        }
                    }

                    alpha = Math.Max(alpha, eval.Evaluation);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                return bestEval;
            }
            else
            {
                MoveTree bestEval = new MoveTree(1, new List<List<string>>());
                for (int i = 0; i < possibleMoves.Length; i += 5)
                {
                    string move = possibleMoves.Substring(i, 5);
                    MakeMove(move, stack);
                    MoveTree eval = FindMate(stack, depth - 1, alpha, beta, true);
                    UndoMove(stack);

                    if (eval.Evaluation < bestEval.Evaluation)
                    {
                        bestEval.Evaluation = eval.Evaluation;
                        bestEval.Paths = new List<List<string>>();
                    }

                    if (eval.Evaluation == bestEval.Evaluation)
                    {
                        foreach (var path in eval.Paths)
                        {
                            var newPath = new List<string> { move };
                            newPath.AddRange(path);
                            bestEval.Paths.Add(newPath);
                        }
                    }

                    beta = Math.Min(beta, eval.Evaluation);
                    if (beta <= alpha)
                    {
                        break;
                    }
                }
                return bestEval;
            }
        }

        //Orders possible moves to improve alpha-beta pruning efficiency.
        public static string OrderMoves(string originalMoveString, GameState position)
        {
            if (originalMoveString.Length <= 5)
            {
                return originalMoveString;
            }
            GameStateStack stack = new GameStateStack();
            stack.Push(position);
            List<Tuple<string, int>> moves = new List<Tuple<string, int>>();
            for (int i = 0; i < originalMoveString.Length; i += 5)
            {
                moves.Add(Tuple.Create(originalMoveString.Substring(i, 5), 0));
            }
            for (int i = 0; i < moves.Count; i++)
            {
                if (IsCapture(moves[i].Item1))
                {
                    string move = moves[i].Item1;
                    char captured = move[4] != 'P' ? move[4] : move[2];
                    switch (char.ToUpper(captured))
                    {
                        case 'P':
                            moves[i] = Tuple.Create(move, moves[i].Item2 + 100);
                            break;
                        case 'R':
                            moves[i] = Tuple.Create(move, moves[i].Item2 + 500);
                            break;
                        case 'N':
                        case 'B':
                            moves[i] = Tuple.Create(move, moves[i].Item2 + 300);
                            break;
                        case 'Q':
                            moves[i] = Tuple.Create(move, moves[i].Item2 + 900);
                            break;
                    }
                }
                MakeMove(moves[i].Item1, stack);
                GameState state = stack.Peek();
                if (state.IsCheck())
                {
                    moves[i] = Tuple.Create(moves[i].Item1, moves[i].Item2 + 1000);
                }
                UndoMove(stack);
            }
            moves.Sort((x, y) => y.Item2.CompareTo(x.Item2));
            string orderedMoves = "";
            foreach (var move in moves)
            {
                orderedMoves += move.Item1;
            }
            return orderedMoves;
        }
    }
}
