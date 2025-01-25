using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;

namespace Thesis.Pieces
{
    internal class King : Piece
    {
        public King(string color, char file, int rank, ChessBoard board) : base(color, file, rank, board)
        {
            UpdateAttackMap();
        }

        public override string PossibleMoves(List<ChessBoard.Tile>? targetTiles = null)
        {
            string moveString = "";
            string from = File + Rank.ToString();
            for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
            {
                for (int rankOffset = -1; rankOffset <= 1; rankOffset++)
                {
                    if (fileOffset == 0 && rankOffset == 0)
                    {
                        continue;
                    }
                    if (File + fileOffset < 'a' || File + fileOffset > 'h' || Rank + rankOffset < 1 || Rank + rankOffset > 8)
                    {
                        continue;
                    }
                    ChessBoard.Tile? searchedTile = Board.GetTile((char)(File + fileOffset), Rank + rankOffset);
                    if (searchedTile.IsAttackedBy(OppositeColor()))
                    {
                        continue;
                    }
                    string to = (char)(File + fileOffset) + (Rank + rankOffset).ToString();
                    //basic move
                    if (searchedTile.IsEmpty())
                    {
                        int fileDirection = fileOffset == 0 ? 0 : -fileOffset / Math.Abs(fileOffset);
                        int rankDirection = rankOffset == 0 ? 0 : -rankOffset / Math.Abs(rankOffset);
                        //is king still attacked after moving (from the direction of the move)
                        if (!AfterMoveAttacked(fileDirection, rankDirection))
                        {
                            moveString += from + to + " ";
                        }
                    }
                    //capture
                    else if (searchedTile.PieceOnTile.Color != Color)
                    {
                        int fileDirection = fileOffset == 0 ? 0 : -fileOffset / Math.Abs(fileOffset);
                        int rankDirection = rankOffset == 0 ? 0 : -rankOffset / Math.Abs(rankOffset);
                        //is king still attacked after moving (from the direction of the move)
                        if (!AfterMoveAttacked(fileDirection, rankDirection))
                        {
                            moveString += from + to + searchedTile.PieceOnTile.GetSymbol().ToString();
                        }
                    }
                }
            }
            moveString += Castling.AddShortCastling(Board, this);
            moveString += Castling.AddLongCastling(Board, this);
            return moveString;
        }

        //tries to find a rook/queen/bishop attacking from the direction of the move
        //ex: if king moves to the right, it checks if there is a rook/queen attacking from the left
        private bool AfterMoveAttacked(int fileDirection, int rankDirection)
        {
            char fileOffset = (char)(fileDirection + File);
            int rankOffset = rankDirection + Rank;
            while (true)
            {
                if (fileOffset < 'a' || fileOffset > 'h' || rankOffset < 1 || rankOffset > 8)
                {
                    return false;
                }
                ChessBoard.Tile? searchedTile = Board.GetTile(fileOffset, rankOffset);
                if (searchedTile.PieceOnTile != null)
                {
                    if (searchedTile.PieceOnTile.Color == Color)
                    {
                        return false;
                    }
                    if ((searchedTile.PieceOnTile is Rook || searchedTile.PieceOnTile is Queen)
                        && (fileDirection == 0 || rankDirection == 0))
                    {
                        return true;
                    }
                    else if ((searchedTile.PieceOnTile is Bishop || searchedTile.PieceOnTile is Queen)
                        && (fileDirection != 0 && rankDirection != 0))
                    {
                        return true;
                    }
                    return false;
                }
                fileOffset = (char)(fileOffset + fileDirection);
                rankOffset = rankOffset + rankDirection;
            }
            
        }

        public override void UpdateAttackMap()
        {
            AttackMap = new AttackMap();
            for (int fileOffset = -1; fileOffset <= 1; fileOffset++)
            {
                for (int rankOffset = -1; rankOffset <= 1; rankOffset++)
                {
                    if (fileOffset == 0 && rankOffset == 0)
                    {
                        continue;
                    }
                    if (File + fileOffset < 'a' || File + fileOffset > 'h' || Rank + rankOffset < 1 || Rank + rankOffset > 8)
                    {
                        continue;
                    }
                    ChessBoard.Tile? searchedTile = Board.GetTile((char)(File + fileOffset), Rank + rankOffset);
                    if (searchedTile == null)
                    {
                        continue;
                    }
                    AttackMap[searchedTile.File, searchedTile.Rank] = true;
                }
            }
        }
    }
}
