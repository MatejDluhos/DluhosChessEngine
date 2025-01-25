using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Thesis.Pieces;

namespace Thesis.Data
{
    internal class GameState
    {
        //HalfMove: Increment after a move of a pawn or a capture, reset to 0 after a non-pawn move without capture
        public byte HalfMoves { get; set; }
        //FullMove: Start at 1, increment after every black move
        public byte FullMoves { get; set; }
        //'w' if white to move, 'b' if black to move
        public char ToMove { get; set; }
        //'KQkq' at start, remove coresponding letter as rights are lost, '-' if no rights
        public string CastleRights { get; set; }
        //Square behind the pawn that can be captured en passant, '-' if no such square
        public string EnPassant { get; set; }
        public ChessBoard Board { get; set; }

        //default constructor of GameState
        public GameState()
        {
            Board = new ChessBoard(this);
            ToMove = 'w';
            CastleRights = "";
            EnPassant = "";
            HalfMoves = 0;
            FullMoves = 0;
        }

        //constructor of GameState when provided with a FEN string (used with UCI)
        public GameState(string FEN)
        {
            if (!IsValidFEN(FEN))
            {
                throw new ArgumentException("Invalid FEN string");
            }
            Board = new ChessBoard(this);
            string[] fenParts = FEN.Split(' ');
            Board.SetFENPosition(fenParts[0]);
            ToMove = fenParts[1][0];
            CastleRights = fenParts[2];
            EnPassant = fenParts[3];
            HalfMoves = byte.Parse(fenParts[4]);
            FullMoves = byte.Parse(fenParts[5]);
        }


        //checks for FEN string validity
        private bool IsValidFEN(string FEN)
        {
            //TODO
            return true;
        }

        public bool IsCheck()
        {
            List<Piece> pieces = (ToMove == 'w') ? Board.WhitePieces : Board.BlackPieces;
            foreach (Piece piece in pieces)
            {
                if (piece is King)
                {
                    return Board.GetTile(piece.File, piece.Rank).IsAttackedBy(piece.OppositeColor());
                }
            }
            return false;
        }

        public bool IsCheckmate()
        {
            if (!IsCheck())
            {
                return false;
            }
            Piece? king = null;
            //can the king escape?
            List<Piece> pieces = (ToMove == 'w') ? Board.WhitePieces : Board.BlackPieces;
            foreach (Piece piece in pieces)
            {
                if (piece.GetType() == typeof(King))
                {
                    if (piece.PossibleMoves() != "")
                    {
                        return false;
                    }
                    king = piece;
                }
            }
            //identify the threats
            List<Piece> threats = new List<Piece>(); ;
            List<Piece> suspects = king.Color == "white" ? Board.BlackPieces : Board.WhitePieces;
            foreach (Piece suspect in suspects)
            {
                if (suspect.AttackMap[king.File, king.Rank])
                {
                    threats.Add(suspect);
                }
            }
            Piece threat = threats[0];
            return !Board.CanBeCaptured(threat) && !Board.CanBeBlocked(threat, king);
        }


        public void LoadFEN(string FEN)
        {
            if (!IsValidFEN(FEN))
            {
                throw new ArgumentException("Invalid FEN string");
            }
            string[] fenParts = FEN.Split(' ');
            Board.SetFENPosition(fenParts[0]);
            ToMove = fenParts[1][0];
            CastleRights = fenParts[2];
            EnPassant = fenParts[3];
            HalfMoves = byte.Parse(fenParts[4]);
            FullMoves = byte.Parse(fenParts[5]);
        }

        public void SetStartPos()
        {
            LoadFEN("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
        }

        public override string ToString()
        {
            return $"Turn of: {ToMove}\nCastle Rights: {CastleRights}\n" +
                $"En Passant: {EnPassant}\nHalf Moves: {HalfMoves}\nFull Moves: {FullMoves}\n";
        }

        public string PossibleMoves()
        {
            List<Piece> pinnedPieces = FindPinnedPieces();
            List<ChessBoard.Tile>? targetTiles = null;
            if (IsCheck())
            {
                targetTiles = FindTargetTiles();
                //if is check and targetTiles are null, then there are more than 1 threat, meaning the only option is to move the king
                if (targetTiles.Count == 0)
                {
                    List<Piece> kingCandidates = ToMove == 'w' ? Board.WhitePieces : Board.BlackPieces;
                    foreach (Piece piece in kingCandidates)
                    {
                        if (piece.GetType() == typeof(King))
                        {
                            return piece.PossibleMoves();
                        }
                    }
                }
            }
            string moveString = "";
            List<Piece> pieces;
            pieces = (ToMove == 'w') ? Board.WhitePieces : Board.BlackPieces;
            GameStateStack stack = new GameStateStack();
            stack.Push(this);
            foreach (Piece piece in pieces)
            {   
                if (pinnedPieces.Contains(piece))
                {
                    string pinString = piece.PossibleMoves();
                    if (pinString != "")
                    {
                        for (int i = 0; i < pinString.Length; i+=5)
                        {
                            string move = pinString.Substring(i, 5);
                            Move.MakeMove(move, stack);
                            GameState afterMove = stack.Peek();
                            afterMove.ToMove = (afterMove.ToMove == 'w') ? 'b' : 'w';
                            if (!afterMove.IsCheck())
                            {
                                moveString += move;
                            }
                            Move.UndoMove(stack);
                        }
                    }
                }
                else
                {
                    moveString += piece.PossibleMoves(targetTiles);
                }
            }
            return moveString;
        }

        //Finds and returns the tiles between the king in check and the threat piece.
        public List<ChessBoard.Tile> FindTargetTiles()
        {
            //find the king
            Piece? king = null;
            List<Piece> pieces = (ToMove == 'w') ? Board.WhitePieces : Board.BlackPieces;
            foreach (Piece piece in pieces)
            {
                if (piece.GetType() == typeof(King))
                {
                    king = piece;
                }
            }
            //find the threat piece
            List<Piece> threats = new List<Piece>(); ;
            List<Piece> suspects = king.Color == "white" ? Board.BlackPieces : Board.WhitePieces;
            foreach (Piece suspect in suspects)
            {
                if (suspect.AttackMap[king.File, king.Rank])
                {
                    threats.Add(suspect);
                }
            }
            if (threats.Count > 1)
            {
                return new List<ChessBoard.Tile>();
            }
            Piece threat = threats[0];
            
            //find the tiles between the king and the threat
            List<ChessBoard.Tile> targetTiles = new List<ChessBoard.Tile>();
            if (threat is not Bishop && threat is not Rook && threat is not Queen)
            {
                targetTiles.Add(Board.GetTile(threat.File, threat.Rank));
                return targetTiles;
            }
            int fileDiff = threat.File - king.File;
            int rankDiff = threat.Rank - king.Rank;
            int fileStep = fileDiff == 0 ? 0 : fileDiff / Math.Abs(fileDiff);
            int rankStep = rankDiff == 0 ? 0 : rankDiff / Math.Abs(rankDiff);
            char file = (char)(king.File + fileStep);
            int rank = king.Rank + rankStep;
            while (file != threat.File || rank != threat.Rank)
            {
                targetTiles.Add(Board.GetTile(file, rank));
                file = (char)(file + fileStep);
                rank += rankStep;
            }
            targetTiles.Add(Board.GetTile(file, rank));
            return targetTiles;
        }

        public List<Piece> FindPinnedPieces()
        {
            List<Piece> pinnedPieces = new List<Piece>();
            List<Piece> pieces = (ToMove == 'w') ? Board.WhitePieces : Board.BlackPieces;
            Piece? king = pieces.FirstOrDefault(p => p.GetType() == typeof(King));

            if (king == null)
            {
                return pinnedPieces;
            }
            List<(int, int)> directions = new List<(int, int)>
            {(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1)};

            foreach ((int, int) direction in directions)
            {
                char file = king.File;
                int rank = king.Rank;
                bool pinSuspect = false;
                ChessBoard.Tile? suspectTile = null;

                while (true)
                {
                    file = (char)(file + direction.Item1);
                    rank += direction.Item2;

                    if (file < 'a' || file > 'h' || rank < 1 || rank > 8)
                    {
                        break;
                    }
                    ChessBoard.Tile tile = Board.GetTile(file, rank);

                    if (tile.PieceOnTile != null)
                    {
                        Piece currentPiece = tile.PieceOnTile;
                        if (currentPiece.Color != king.Color)
                        {
                            if (!pinSuspect)
                            {
                                break;
                            }
                            else
                            {
                                if (currentPiece.AttackMap[suspectTile.File, suspectTile.Rank]
                                    && currentPiece.GetType() != typeof(King))
                                {
                                    pinnedPieces.Add(suspectTile.PieceOnTile);
                                }
                                break;
                            }
                        }
                        else
                        {
                            if (pinSuspect)
                            {
                                break;
                            }
                            else
                            {
                                pinSuspect = true;
                                suspectTile = tile;
                            }
                        }
                    }
                }
            }
            return pinnedPieces;
        }

        public GameState Copy()
        {
            GameState copy = new GameState();
            copy.Board = Board.Copy(copy);
            copy.ToMove = ToMove;
            copy.CastleRights = CastleRights;
            copy.EnPassant = EnPassant;
            copy.HalfMoves = HalfMoves;
            copy.FullMoves = FullMoves;
            return copy;
        }

        //helper method for finding skewers, pins and xrays
        //Returns a list of three pieces each, first one being the attacker, 
        //second is the closer one to attacker and third is the further one.
        public List<List<Piece>> FindPiecesInLine()
        {
            List<List<Piece>> piecesInLine = new List<List<Piece>>();
            List<List<Piece>> allPieces = new List<List<Piece>>() { Board.WhitePieces, Board.BlackPieces };
            List<Piece> attackers = new List<Piece>();
            foreach (List<Piece> pieces in allPieces)
            {
                foreach (Piece piece in pieces)
                {
                    if (piece.GetType() == typeof(Queen) || piece.GetType() == typeof(Rook) || piece.GetType() == typeof(Bishop))
                    {
                        attackers.Add(piece);
                    }
                }
            }
            int[][] orthogonalOffsets = new int[][] { new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 1, 0 } };           
            int[][] diagonalOffsets = new int[][] { new int[] { -1, -1 }, new int[] { -1, 1 }, new int[] { 1, -1 }, new int[] { 1, 1 } };
            foreach (Piece attacker in attackers)
            {
                int[][] offsets;
                if (attacker is Rook)
                {
                    offsets = orthogonalOffsets;
                }
                else if (attacker is Bishop)
                {
                    offsets = diagonalOffsets;
                }
                else
                {
                    offsets = new int[][] { new int[] { 0, -1 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 1, 0 },
                                           new int[] { -1, -1 }, new int[] { -1, 1 }, new int[] { 1, -1 }, new int[] { 1, 1 } };
                }
                List<Piece> piecesInLineForAttacker = new List<Piece>();
                foreach (int[] offset in offsets)
                {
                    piecesInLineForAttacker.Add(attacker);
                    char file = attacker.File;
                    int rank = attacker.Rank;
                    while (true)
                    {
                        file = (char)(file + offset[0]);
                        rank += offset[1];
                        if (file < 'a' || file > 'h' || rank < 1 || rank > 8)
                        {
                            break;
                        }
                        ChessBoard.Tile tile = Board.GetTile(file, rank);
                        if (tile.PieceOnTile != null)
                        {
                            Piece piece = tile.PieceOnTile;
                            piecesInLineForAttacker.Add(piece);
                            if (piecesInLineForAttacker.Count == 3)
                            {
                                piecesInLine.Add(piecesInLineForAttacker);
                                piecesInLineForAttacker = new List<Piece>();
                                piecesInLineForAttacker.Add(attacker);
                                break;
                            }
                        }
                    }
                    piecesInLineForAttacker = new List<Piece>();
                }
            }
            return piecesInLine;
        }
    }
}
