using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thesis.Data
{
    internal class AttackMap
    {
        private bool[,] map;

        public AttackMap()
        {
            map = new bool[8, 8];
        }

        //Indexer to access the map using chess notation.
        public bool this[char column, int row]
        {
            get
            {
                (int rowIndex, int colIndex) = ConvertChessNotation(column, row);
                return map[rowIndex, colIndex];
            }
            set
            {
                (int rowIndex, int colIndex) = ConvertChessNotation(column, row);
                map[rowIndex, colIndex] = value;
            }
        }

        //Method to convert chess notation to array indices.
        private (int row, int col) ConvertChessNotation(char column, int row)
        {
            if (column < 'a' || column > 'h')
                throw new ArgumentException("Invalid column");
            if (row < 1 || row > 8)
                throw new ArgumentException("Invalid row");

            int colIndex = column - 'a';
            int rowIndex = 8 - row;

            return (rowIndex, colIndex);
        }

        //Example method to print the board.
        public void PrintAttackMap()
        {
            for (int i = 0; i < map.GetLength(0); i++)
            {
                for (int j = 0; j < map.GetLength(1); j++)
                {
                    Console.Write(map[i, j] ? "1 " : "0 ");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
