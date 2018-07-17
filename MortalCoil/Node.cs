using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortalCoil
{
    public class Node
    {
        public Node Left, Right, Up, Down;
        public char[,] Map;
        public int X, Y;

        public Node(int x, int y, char[,] map)
            => (X, Y, Map) = (x, y, map);
    }
}
