using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thesis.Data;

namespace Thesis.Pieces
{
    internal class Rook : Piece
    {
        public Rook(string color, char file, int rank, ChessBoard board) : base(color, file, rank, board)
        {
            UpdateAttackMap();
        }

        public override string PossibleMoves(List<ChessBoard.Tile>? targetTiles = null)
        {
            return AddLineMoves("orthogonal", targetTiles);
        }

        //update the attack map of the piece after its move
        public override void UpdateAttackMap()
        {
            AttackMap = new AttackMap();
            UpdateAttackMapChoose("orthogonal");
        }

        //modify the attack map of the piece after the move of another piece
        public override void ModifyAttackMap(char file, int rank, string situation)
        {
            ModifyAttackMapChoose(file, rank, situation, "orthogonal");
        }
    }
}
