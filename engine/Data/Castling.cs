using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Thesis.Pieces;

namespace Thesis.Data
{
    internal class Castling
    {
        public static bool IsLongCastling(string move)
        {
            return move[4] == 'L';
        }

        public static bool IsShortCastling(string move)
        {
            return move[4] == 'S';
        }

        public static string AddShortCastling(ChessBoard board, Piece king)
        {
            if (king.Color == "white" && board.GameState.CastleRights.Contains('K') ||
                king.Color == "black" && board.GameState.CastleRights.Contains('k'))
            {
                if (board.GetTile((char)(king.File + 1), king.Rank).IsEmpty())
                {
                    if (board.GetTile((char)(king.File + 2), king.Rank).IsEmpty())
                    {
                        if (!board.GetTile((char)(king.File + 1), king.Rank).IsAttackedBy(king.OppositeColor()) &&
                            !board.GetTile((char)(king.File + 2), king.Rank).IsAttackedBy(king.OppositeColor()))
                        {
                            string from = king.File + king.Rank.ToString();
                            string to = (char)(king.File + 2) + king.Rank.ToString();
                            return from + to + 'S';
                        }
                    }
                }
            }
            return "";
        }

        public static string AddLongCastling(ChessBoard board, Piece king)
        {
            if (king.Color == "white" && board.GameState.CastleRights.Contains('Q') ||
                king.Color == "black" && board.GameState.CastleRights.Contains('q'))
            {
                if (board.GetTile((char)(king.File - 1), king.Rank).IsEmpty())
                {
                    if (board.GetTile((char)(king.File - 2), king.Rank).IsEmpty())
                    {
                        if (board.GetTile((char)(king.File - 3), king.Rank).IsEmpty())
                        {
                            if (!board.GetTile((char)(king.File - 1), king.Rank).IsAttackedBy(king.OppositeColor()) &&
                                !board.GetTile((char)(king.File - 2), king.Rank).IsAttackedBy(king.OppositeColor()))
                            {
                                string from = king.File + king.Rank.ToString();
                                string to = (char)(king.File - 2) + king.Rank.ToString();
                                return from + to + 'L';
                            }
                        }
                    }
                }
            }
            return "";
        }

        //updates the gamestate castle rights after castling
        public static void UpdateCastling(string color, GameState state)
        {
            if (color == "white")
            {
                state.CastleRights = state.CastleRights.Replace("K", "");
                state.CastleRights = state.CastleRights.Replace("Q", "");
            }
            else
            {
                state.CastleRights = state.CastleRights.Replace("k", "");
                state.CastleRights = state.CastleRights.Replace("q", "");
            }
            if (state.CastleRights == "")
            {
                state.CastleRights = "-";
            }
        }

        //updates castling rights after a move if needed (rook or king move)
        public static void CheckCastlingUpdates(string move, GameState state)
        {
            if (state.CastleRights == "-")
            {
                return;
            }
            ChessBoard.Tile? fromTile = state.Board.GetTile(move[0], int.Parse(move.Substring(1, 1)));
            if (fromTile.PieceOnTile is King)
            {
                if (fromTile.PieceOnTile.Color == "white")
                {
                    state.CastleRights = state.CastleRights.Replace("K", "");
                    state.CastleRights = state.CastleRights.Replace("Q", "");
                }
                else
                {
                    state.CastleRights = state.CastleRights.Replace("k", "");
                    state.CastleRights = state.CastleRights.Replace("q", "");
                }
            }
            else if (fromTile.PieceOnTile is Rook)
            {
                if (move[0] == 'a' && move[1] == '1')
                {
                    state.CastleRights = state.CastleRights.Replace("Q", "");
                }
                else if (move[0] == 'h' && move[1] == '1')
                {
                    state.CastleRights = state.CastleRights.Replace("K", "");
                }
                else if (move[0] == 'a' && move[1] == '8')
                {
                    state.CastleRights = state.CastleRights.Replace("q", "");
                }
                else if (move[0] == 'h' && move[1] == '8')
                {
                    state.CastleRights = state.CastleRights.Replace("k", "");
                }
            }
        }
    }
}
