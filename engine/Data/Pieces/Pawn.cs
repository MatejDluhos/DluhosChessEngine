using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;

namespace Thesis.Pieces
{
    internal class Pawn : Piece
    {
        public Pawn(string color, char file, int rank, ChessBoard board) : base(color, file, rank, board)
        {
            UpdateAttackMap();
        }
        public override string PossibleMoves(List<ChessBoard.Tile>? targetTiles = null)
        {
            string moveString = "";
            string from = File + Rank.ToString(), to;
            int ahead = (Color == "white") ? 1 : -1;
            string opponentColor = (Color == "white") ? "black" : "white";
            ChessBoard.Tile? searchedTile;
            bool shouldProcess;

            //move by 2
            if (Rank == 2 || Rank == 7)
            {
                if (Board.GetTile(File, Rank + ahead) != null)
                {
                    if (Board.GetTile(File, Rank + ahead).IsEmpty())
                    {
                        if (Board.GetTile(File, Rank + ahead * 2) != null)
                        {
                            if (Board.GetTile(File, Rank + ahead * 2).IsEmpty())
                            {
                                searchedTile = Board.GetTile(File, Rank + ahead * 2);
                                shouldProcess = targetTiles == null || targetTiles.Contains(searchedTile);
                                if (shouldProcess)
                                {
                                    to = File + (Rank + ahead * 2).ToString();
                                    moveString += from + to + " ";
                                }
                            }
                        }
                    }
                }
            }
            //move by 1
            searchedTile = Board.GetTile(File, Rank + ahead);
            shouldProcess = targetTiles == null || targetTiles.Contains(searchedTile);
            if (searchedTile.IsEmpty())
            {
                //promotion
                if (Rank + ahead == 8 || Rank + ahead == 1)
                {
                    char[] promotions = (Color == "white") ? new char[] { 'Q', 'R', 'B', 'N' } : new char[] { 'q', 'r', 'b', 'n' };
                    if (shouldProcess)
                    {
                        foreach (char promotion in promotions)
                        {
                            moveString += File.ToString() + File + " " + promotion + "P";
                        }
                    }
                }
                else
                {
                    if (shouldProcess)
                    {
                        to = File + (Rank + ahead).ToString();
                        moveString += from + to + " ";
                    }
                }
                    
            }
            //capture
            ChessBoard.Tile? left = Board.GetTile((char)(File - 1), Rank + ahead);
            ChessBoard.Tile? right = Board.GetTile((char)(File + 1), Rank + ahead);
            foreach (ChessBoard.Tile? side in new ChessBoard.Tile?[] {left, right})
            {
                if (side != null)
                {
                    if (side.PieceOnTile != null)
                    {
                        if (side.PieceOnTile.Color == opponentColor)
                        {
                            shouldProcess = targetTiles == null || targetTiles.Contains(side);
                            if (shouldProcess)
                            {
                                //promotion
                                if (Rank + ahead == 8 || Rank + ahead == 1)
                                {
                                    char[] promotions = (Color == "white") ? new char[] { 'Q', 'R', 'B', 'N' } : new char[] { 'q', 'r', 'b', 'n' };
                                    foreach (char promotion in promotions)
                                    {
                                        moveString += File.ToString() + side.File + side.PieceOnTile.GetSymbol().ToString() + promotion + "P";
                                    }
                                }
                                else
                                {
                                    to = side.File + (Rank + ahead).ToString();
                                    moveString += from + to + side.PieceOnTile.GetSymbol().ToString();
                                }
                            }
                        }
                    }
                }
            }
            //en passant
            if (Board.GameState.EnPassant != "-")
            {
                char enPassantFile = Board.GameState.EnPassant[0];
                int enPassantRank = int.Parse(Board.GameState.EnPassant[1].ToString());
                if (Math.Abs(enPassantRank - Rank) == 1)
                {
                    if (Math.Abs(enPassantFile - File) == 1)
                    {
                        searchedTile = Board.GetTile(enPassantFile, Rank + ahead);
                        shouldProcess = targetTiles == null || targetTiles.Contains(searchedTile);
                        if (shouldProcess)
                        {
                            to = enPassantFile + (Rank + ahead).ToString();
                            moveString += from + to + "E";
                        }
                    }
                }
            }
            return moveString;
        }


        //update the attack map of the piece after its move
        public override void UpdateAttackMap()
        {
            AttackMap = new AttackMap();
            int ahead = (Color == "white") ? 1 : -1;
            if (File != 'a')
            {
                AttackMap[(char)(File - 1), Rank + ahead] = true;
            }
            if (File != 'h')
            {
                AttackMap[(char)(File + 1), Rank + ahead] = true;
            }
        }
    }
}
