using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;

namespace Thesis.Pieces
{
    internal class Knight : Piece
    {
        public Knight(string color, char file, int rank, ChessBoard board) : base(color, file, rank, board)
        {
            UpdateAttackMap();
        }

        public override string PossibleMoves(List<ChessBoard.Tile>? targetTiles = null)
        {
            string moveString = "";
            int[][] offsets = {new int[] { -2, -1 }, new int[] { -2, 1 }, new int[] { -1, -2 }, new int[] { -1, 2 },
                               new int[] { 1, -2 }, new int[] { 1, 2 }, new int[] { 2, -1 }, new int[] { 2, 1 }};
            for (int i = 0; i < offsets.Length; i++)
            {
                int fileOffset = offsets[i][0];
                int rowOffset = offsets[i][1];
                ChessBoard.Tile? searchedTile = Board.GetTile((char)(File + fileOffset), Rank + rowOffset);

                if (searchedTile == null)
                {
                    continue;
                }
                if (searchedTile.PieceOnTile != null)
                {
                    if (searchedTile.PieceOnTile.Color == Color)
                    {
                        continue;
                    }
                }
                if (targetTiles != null)
                {
                    if (!targetTiles.Contains(searchedTile))
                    {
                        continue;
                    }
                }
                Piece? pieceToTake = searchedTile.PieceOnTile;
                string from = File + Rank.ToString();
                string to = (char)(File + fileOffset) + (Rank + rowOffset).ToString();
                if (pieceToTake == null)
                {
                    moveString += from + to + " ";
                }
                else
                {
                    moveString += from + to + pieceToTake.GetSymbol().ToString();
                }
            }
            return moveString;
        }

        //update the attack map of the piece after its move
        public override void UpdateAttackMap()
        {
            AttackMap = new AttackMap();
            int[][] offsets = {new int[] { -2, -1 }, new int[] { -2, 1 }, new int[] { -1, -2 }, new int[] { -1, 2 },
                               new int[] { 1, -2 }, new int[] { 1, 2 }, new int[] { 2, -1 }, new int[] { 2, 1 }};
            foreach (var offset in offsets)
            {
                int fileOffset = offset[0];
                int rankOffset = offset[1];

                char newFile = (char)(File + fileOffset);
                int newRank = Rank + rankOffset;

                if (newFile >= 'a' && newFile <= 'h' && newRank >= 1 && newRank <= 8)
                {
                    AttackMap[newFile, newRank] = true;
                }
            }
        }
    }
}
