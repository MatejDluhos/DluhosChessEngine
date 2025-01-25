using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Evaluation
{
    internal class PositionDetails
    {
        public string Theme { get; set; }
        public List<Pieces.Piece> InvolvedPieces { get; set; }

        public PositionDetails()
        {
            Theme = "";
            InvolvedPieces = new List<Pieces.Piece>();
        }

        public string GetDetails()
        {
            string resString;
            switch (Theme)
            {
                case "SacrificeMaterial":
                    return $"Sacrifice Material: Sacrificed {InvolvedPieces[0]}.\n";
                case "Fork":
                    resString = $"Fork: {InvolvedPieces[0]} forks: ";
                    foreach (Pieces.Piece piece in InvolvedPieces.Skip(1))
                    {
                        resString += $"{piece}";
                        if (piece != InvolvedPieces.Last())
                        {
                            resString += ", ";
                        }
                    }
                    return resString + ".\n";
                case "Pin":
                    return $"Pin: {InvolvedPieces[0]} pins {InvolvedPieces[1]} to a {InvolvedPieces[2]}.\n";
                case "Skewer":
                    return $"Skewer: {InvolvedPieces[0]} skewers {InvolvedPieces[1]}.\n";
                case "XRay":
                    return $"XRay: {InvolvedPieces[0]} x-ray defends {InvolvedPieces[2]} through {InvolvedPieces[1]}.\n";
                case "DiscoveredAttack":
                    return $"Discovered Attack: {InvolvedPieces[0]} discovers an attack on {InvolvedPieces[1]}.\n";
                case "SmotheredMate":
                    return $"Smothered Mate: {InvolvedPieces[0]} mates the opponent.\n";
                case "CrossCheck":
                    return $"Cross Check: {InvolvedPieces[0]} cross checks the opponent.\n";
                case "Promotion":
                    return $"Promotion: {InvolvedPieces[0]} promotes to {InvolvedPieces[1]}.\n";
            }
            return "";
        }
    }
}
