using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Pieces;

namespace Thesis.Data
{
    internal class ChessBoard
    {
        public Tile[,] Board;
        public GameState GameState;
        public List<Piece> WhitePieces;
        public List<Piece> BlackPieces;

        internal class Tile
        {
            public char File { get; set; }
            public int Rank { get; set; }
            public Piece? PieceOnTile { get; set; }
            public ChessBoard Board { get; set; }

            public Tile(char file, int rank, ChessBoard board, Piece? pieceOnTile = null)
            {
                File = file;
                Rank = rank;
                PieceOnTile = pieceOnTile;
                Board = board;
            }

            public bool IsEmpty()
            {
                return PieceOnTile == null;
            }

            public bool IsAttackedBy(string color)
            {
                foreach (Piece piece in color == "white" ? Board.WhitePieces : Board.BlackPieces)
                {
                    if (piece.AttackMap[File, Rank])
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        

        //Creates a new empty chess board.
        public ChessBoard(GameState gameState){
            Board = new Tile[8, 8];
            for(char file = 'a'; file <= 'h'; file++){
                for(int rank = 1; rank <= 8; rank++){

                    int column = file - 'a';
                    int row = 8 - rank;

                    Tile newTile = new Tile(file, rank, this);
                    Board[column, row] = newTile;
                }
            }
            GameState = gameState;
            WhitePieces = new List<Piece>();
            BlackPieces = new List<Piece>();
        }

        //Sets up a position on an empy board specified by FEN.
        //FEN form: 8 strings divided by '/' representing ranks from 8 to 1
        //example: "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR"
        public void SetFENPosition(string FEN)
        {
            string[] ranks = FEN.Split('/');
            int rankIndex = 8;

            foreach (string rank in ranks)
            {
                int fileIndex = 1;

                foreach (char symbol in rank)
                {
                    if (char.IsDigit(symbol))
                    {
                        fileIndex += (int)char.GetNumericValue(symbol);
                    }
                    else
                    {
                        SetTile((char)('a' + fileIndex - 1), rankIndex, Piece.CreatePieceBySymbol(symbol, (char)('a' + fileIndex - 1), rankIndex, this));
                        fileIndex++;
                    }
                }
                rankIndex--;
            }
        }

        //Print for testing purposes.
        public void PrintBoard()
        {
            for(int row = 0; row <= 7; row++)
            {
                for(int col = 0; col <= 7; col++)
                {
                    if (Board[col, row].PieceOnTile == null){
                        Console.Write(". ");
                    }
                    else
                    {
                        Console.Write(Board[col, row].PieceOnTile.GetSymbol() + " ");
                    }
                }
                Console.Write("\n");
            }
            Console.WriteLine();
        }

        //Returns a board tile given a file and rank. Example: GetTile('a', 1)
        public Tile? GetTile(char file, int rank)
        {
            if (file < 'a' || file > 'h' || rank < 1 || rank > 8)
            {
                return null;
            }
            int column = file - 'a';
            int row = 8 - rank;
            return Board[column, row];
        }

        //Sets a piece on a tile given a file, rank, and piece. Example: SetTile('a', 1, new Rook("white", 'a', 1))
        //Also adds the piece to the list of pieces of the same color.
        public void SetTile(char file, int rank, Piece? piece)
        {
            int column = file - 'a';
            int row = 8 - rank;
            Board[column, row].PieceOnTile = piece;
            if (piece != null)
            {
                if (piece.Color == "white")
                {
                    WhitePieces.Add(piece);
                }
                else
                {
                    BlackPieces.Add(piece);
                }
            }
        }

        //Checks if a threat piece can be captured by any piece of opposite color.
        public bool CanBeCaptured(Piece threat)
        {
            List<Piece> pieces = threat.Color == "white" ? BlackPieces : WhitePieces;
            List<Piece> pinnedPieces = GameState.FindPinnedPieces();
            foreach (Piece piece in pieces)
            {
                if (piece.AttackMap[threat.File, threat.Rank])
                {
                    if (piece.GetType() == typeof(King))
                    {
                        if (!GetTile(threat.File, threat.Rank).IsAttackedBy(threat.Color))
                        {
                            return true;
                        }
                    }
                    if (pinnedPieces.Contains(piece))
                    {
                        continue;
                    }
                    if (piece.GetType() != typeof(King))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        //Checks if the path between a threat and target can be blocked by any piece of the targeted side.
        public bool CanBeBlocked(Piece threat, Piece target)
        {
            if (threat is Knight || threat is Pawn || threat is King)
            {
                return false;
            }
            string blockingColor = target.Color;
            List<Piece> blockingPieces = blockingColor == "white" ? WhitePieces : BlackPieces;
            int fileOffset = threat.File < target.File ? -1 : threat.File == target.File ? 0 : 1;
            int rankOffset = threat.Rank < target.Rank ? -1 : threat.Rank == target.Rank ? 0 : 1;
            char currentFile = (char)(target.File + fileOffset);
            int currentRank = target.Rank + rankOffset;

            while (currentFile != threat.File || currentRank != threat.Rank)
            {
                for (int i = 0; i < blockingPieces.Count; i++)
                {
                    Piece piece = blockingPieces[i];
                    if (piece.AttackMap[currentFile, currentRank])
                    {
                        if (piece.GetType() == typeof(King))
                        {
                            continue;
                        }
                        RemovePiece(piece.File, piece.Rank);
                        SetTile(currentFile, currentRank, piece);
                        if (!GameState.IsCheck())
                        {
                            SetTile(piece.File, piece.Rank, piece);
                            RemovePiece(currentFile, currentRank);
                            return true;
                        }
                        SetTile(piece.File, piece.Rank, piece);
                        RemovePiece(currentFile, currentRank);
                    }
                }
                currentFile = (char)(currentFile +fileOffset);
                currentRank += rankOffset;
            }
            return false;
        }

        public ChessBoard Copy(GameState state)
        {
            ChessBoard newBoard = new ChessBoard(state);
            for (char file = 'a'; file <= 'h'; file++)
            {
                for (int rank = 1; rank <= 8; rank++)
                {
                    Tile tile = GetTile(file, rank);
                    Piece? piece = tile.PieceOnTile;
                    if (piece != null)
                    {
                        char symbol = piece.GetSymbol();
                        newBoard.SetTile(file, rank, Piece.CreatePieceBySymbol(symbol, file, rank, newBoard));
                    }
                }
            }
            return newBoard;
        }

        public List<Piece> FindPiecesToModify(char file, int rank)
        {
            List<Piece> foundPieces = new List<Piece>();
            List<Piece> allPieces = WhitePieces.Concat(BlackPieces).ToList();
            foreach (Piece piece in allPieces)
            {
                if (piece is Rook || piece is Queen || piece is Bishop)
                {
                    if (piece.AttackMap[file, rank])
                    {
                        foundPieces.Add(piece);
                    }
                }
            }
            return foundPieces;
        }

        public void RemovePiece(char file, int rank)
        {
            Piece? piece = GetTile(file, rank).PieceOnTile;
            if (piece != null)
            {
                if (piece.Color == "white")
                {
                    WhitePieces.Remove(piece);
                }
                else
                {
                    BlackPieces.Remove(piece);
                }
                SetTile(file, rank, null);
                List<Piece> toModify = FindPiecesToModify(file, rank);
                foreach (Piece pieceToModify in toModify)
                {
                    pieceToModify.ModifyAttackMap(file, rank, "origin");
                }
            }
        }
    }
}