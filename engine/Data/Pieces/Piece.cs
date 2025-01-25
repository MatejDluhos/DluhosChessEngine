using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;

namespace Thesis.Pieces
{
    internal class Piece
    {
        public string Color;
        public char File;
        public int Rank;
        public ChessBoard Board;
        public AttackMap AttackMap;

        public Piece(string color, char file, int rank, ChessBoard board)
        {
            Color = color;
            File = file;
            Rank = rank;
            Board = board;
            AttackMap = new AttackMap();
            List<Piece> toModify = board.FindPiecesToModify(file, rank);
            foreach (Piece piece in toModify)
            {
                piece.ModifyAttackMap(file, rank, "dest");
            }
        }

        //Creates a specific piece based on the argument 'symbol' and puts it on the board.
        public static Piece CreatePieceBySymbol(char symbol, char file, int rank, ChessBoard board)
        {
            string color;
            if (char.IsUpper(symbol))
            {
                color = "white";
            }
            else
            {
                color = "black";
            }
            Piece res = null;
            switch (symbol)
            {
                case 'p' or 'P':
                    res = new Pawn(color, file, rank, board);
                    break;
                case 'n' or 'N':
                    res = new Knight(color, file, rank, board);
                    break;
                case 'r' or 'R':
                    res = new Rook(color, file, rank, board);
                    break;
                case 'q' or 'Q':
                    res = new Queen(color, file, rank, board);
                    break;
                case 'k' or 'K':
                    res = new King(color, file, rank, board);
                    break;
                case 'b' or 'B':
                    res = new Bishop(color, file, rank, board);
                    break;
                default:
                    throw new ArgumentException("Invalid symbol.");

            }
            return res;
        }

        public bool CanAttack(char file, int rank)
        {
            return AttackMap[file, rank];
        }

        //Returns the symbol representing the piece.
        public char GetSymbol()
        {
            switch (this)
            {
                case Pawn:
                    if (Color == "white") { return 'P'; }
                    else { return 'p'; }
                case Knight:
                    if (Color == "white") { return 'N'; }
                    else { return 'n'; }
                case Bishop:
                    if (Color == "white") { return 'B'; }
                    else { return 'b'; }
                case Rook:
                    if (Color == "white") { return 'R'; }
                    else { return 'r'; }
                case Queen:
                    if (Color == "white") { return 'Q'; }
                    else { return 'q'; }
                case King:
                    if (Color == "white") { return 'K'; }
                    else { return 'k'; }
                default:
                    return '.';
            }

        }

        public string OppositeColor()
        {
            return Color == "white" ? "black" : "white";
        }

        //Returns a string of all possible moves for the piece.
        //Optional parameter targetTiles only for purposes of reducing moves during check.
        //Implemented in subclasses.
        public virtual string PossibleMoves(List<ChessBoard.Tile>? targetTiles = null)
        {
            return "";
        }

        //Updates the attack map of the piece after its move.
        //Implemented in subclasses.
        public virtual void UpdateAttackMap()
        {
            return;
        }

        //Modifies the attack map of the piece after the move of another piece.
        //Implemented in subclasses.
        public virtual void ModifyAttackMap(char file, int rank, string situation)
        {
            return;
        }

        //helper function for adding line moves to the move string
        //spec: "diagonal" or "orthogonal"
        //used by rook, bishop and queen
        protected string AddLineMoves(string spec, List<ChessBoard.Tile>? targetTiles = null)
        {
            string moveString = "";
            int[][] diagonals = { new int[] { -1, -1 }, new int[] { -1, 1 }, new int[] { 1, -1 }, new int[] { 1, 1 } };
            int[][] orthogonals = { new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { 0, 1 } };
            int[][] chosen;
            if (spec == "diagonal")
            {
                chosen = diagonals;
            }
            else if (spec == "orthogonal")
            {
                chosen = orthogonals;
            }
            else
            {
                throw new ArgumentException("spec must be 'diagonal' or 'orthogonal'");
            }
            foreach (int[] offsetInstance in chosen)
            {
                int currentFileOffset = 0;
                int currentRankOffset = 0;
                int fileOffset = offsetInstance[0];
                int rankOffset = offsetInstance[1];
                while (File + currentFileOffset <= 'h' && Rank + currentRankOffset <= 8)
                {
                    currentFileOffset += fileOffset;
                    currentRankOffset += rankOffset;

                    ChessBoard.Tile? searchedTile = Board.GetTile((char)(File + currentFileOffset), Rank + currentRankOffset);
                    if (searchedTile == null)
                    {
                        break;
                    }
                    bool shouldProcess = targetTiles == null || targetTiles.Contains(searchedTile);
                    Piece? pieceToTake = searchedTile.PieceOnTile;
                    string from = File + Rank.ToString();
                    string to = (char)(File + currentFileOffset) + (Rank + currentRankOffset).ToString();
                    if (searchedTile.PieceOnTile != null)
                    {
                        if (searchedTile.PieceOnTile.Color == Color)
                        {
                            break;
                        }
                        if (shouldProcess)
                        {
                            pieceToTake = searchedTile.PieceOnTile;
                            from = File + Rank.ToString();
                            to = (char)(File + currentFileOffset) + (Rank + currentRankOffset).ToString();
                            moveString += from + to + pieceToTake.GetSymbol().ToString();
                        }
                        break;
                    }
                    if (shouldProcess)
                    {
                        pieceToTake = searchedTile.PieceOnTile;
                        from = File + Rank.ToString();
                        to = (char)(File + currentFileOffset) + (Rank + currentRankOffset).ToString();
                        if (pieceToTake == null)
                        {
                            moveString += from + to + " ";
                        }
                        else
                        {
                            moveString += from + to + pieceToTake.GetSymbol().ToString();
                            break;
                        }
                    }
                    
                }
            }
            return moveString;
        }

        //modify the attack map of the piece after the move of another piece (if affected on a line)
        //used by rook and queen
        //situation: "origin" or "dest", origin = the original placement of the piece, dest = the destination of the piece
        //"dest": the moved piece now blocks the line of this piece, cut the line off
        //"origin": the moved piece no longer blocks the line of this piece, add the line back
        //direction: "orthogonal" or "diagonal"
        public void ModifyAttackMapChoose(char file, int rank, string situation, string direction)
        {
            int fileOffset, rankOffset;
            if (direction == "orthogonal")
            {
                fileOffset = file < File ? -1 : file == File ? 0 : 1;
                rankOffset = rank < Rank ? -1 : rank == Rank ? 0 : 1;
            }
            else
            {
                fileOffset = file < File ? -1 : 1;
                rankOffset = rank < Rank ? -1 : 1;
            }
            int currentFileOffset = fileOffset;
            int currentRankOffset = rankOffset;

            while (true)
            {
                char newFile = (char)(file + currentFileOffset);
                int newRank = rank + currentRankOffset;

                if (newFile < 'a' || newFile > 'h' || newRank < 1 || newRank > 8)
                {
                    break;
                }

                if (situation == "dest")
                {
                    if (!AttackMap[newFile, newRank])
                    {
                        break;
                    }
                    AttackMap[newFile, newRank] = false;
                }
                else if (situation == "origin")
                {
                    if (AttackMap[newFile, newRank])
                    {
                        break;
                    }
                    AttackMap[newFile, newRank] = true;
                    if (Board.GetTile(newFile, newRank).PieceOnTile != null)
                    {
                        break;
                    }
                }
                currentFileOffset += fileOffset;
                currentRankOffset += rankOffset;
            }
        }

        //update the attack map of the piece after its move 
        //direction: "orthogonal" or "diagonal"
        public void UpdateAttackMapChoose(string direction)
        {
            int[][] offsets;
            if (direction == "orthogonal")
            {
                offsets = new int[][] { new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 1, 0 } };
            }
            else
            {
                offsets = new int[][] { new int[] { -1, -1 }, new int[] { -1, 1 }, new int[] { 1, -1 }, new int[] { 1, 1 } };
            }
            for (int i = 0; i < offsets.Length; i++)
            {
                int fileOffset = offsets[i][0];
                int rowOffset = offsets[i][1];
                char file = File;
                int rank = Rank;

                file = (char)(file + fileOffset);
                rank += rowOffset;
                while (file >= 'a' && file <= 'h' && rank >= 1 && rank <= 8)
                {
                    

                    ChessBoard.Tile? searchedTile = Board.GetTile(file, rank);
                    AttackMap[file, rank] = true;
                    if (searchedTile.PieceOnTile != null)
                    {
                        break;
                    }
                    file = (char)(file + fileOffset);
                    rank += rowOffset;
                }
            }
        }

        public override string ToString()
        {
            return $"{Color} {GetType().Name} on {File}{Rank}";
        }

        public List<Piece> FindCapturablePieces()
        {
            List<Piece> opponentPieces = Color == "white" ? Board.BlackPieces : Board.WhitePieces;
            List<Piece> capturablePieces = new List<Piece>();
            foreach (Piece piece in opponentPieces)
            {
                if (AttackMap[piece.File, piece.Rank])
                {
                    capturablePieces.Add(piece);
                }
            }
            return capturablePieces;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Piece other = (Piece)obj;
            return File == other.File && Rank == other.Rank && Color == other.Color && GetType() == other.GetType();
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(File, Rank, Color, GetType());
        }

        public static bool operator == (Piece left, Piece right)
        {
            if (ReferenceEquals(left, null))
            {
                return ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator != (Piece left, Piece right)
        {
            return !(left == right);
        }
    }
}
