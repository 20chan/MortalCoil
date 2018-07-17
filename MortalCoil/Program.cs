using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortalCoil
{
    class Program
    {
        const int BLANK = 0, WALL = -1;

        static int Width, Height;
        static int[,] map;
        static Stack<Direction> stack = new Stack<Direction>();
        static void Main(string[] args)
        {
            while (true)
            {
                // var raw = "x=6&y=4&board=...X...X........X...X...";
                var raw = Console.ReadLine();
                stack = new Stack<Direction>();

                var splited = raw.Split('&');
                Width = Convert.ToInt32(splited[0].Split('=')[1]);
                Height = Convert.ToInt32(splited[1].Split('=')[1]);
                var data = splited[2].Split('=')[1];

                map = new int[Height, Width];

                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        var ch = data[y * Width + x];
                        if (ch == '.')
                            map[y, x] = BLANK;
                        if (ch == 'X')
                            map[y, x] = WALL;
                    }

                int curx = 0, cury = 0;
                for (cury = 0; cury < Height; cury++)
                    for (curx = 0; curx < Width; curx++)
                        if (map[cury, curx] == BLANK)
                    {
                            var result = Solve(curx, cury);
                            if (result)
                                goto SOLVED;
                    }
                
                Console.WriteLine("Fail..");
                continue;

                SOLVED:
                Console.WriteLine("Solved!");
                Console.WriteLine($"Starts at {curx}, {cury}");
                Console.WriteLine($"http://www.hacker.org/coil/index.php?x={curx}&y={cury}&path={string.Join("", stack.Reverse())}");
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                        Console.Write($"{map[y, x],3}");
                    Console.WriteLine();
                }
            }
        }

        static bool Solve(int x, int y)
        {
            map[y, x] = 1;
            var res = Solve(1, x, y);
            if (res)
                return true;
            map[y, x] = BLANK;
            return false;
        }

        static bool Solve(int depth, int x, int y)
        {
            (int x, int y)[] deltas =
            {
                (0, -1), (0, 1), (-1, 0), (1, 0)
            };

            for (Direction dir = Direction.U; dir <= Direction.R; dir++)
            {
                int dx = deltas[(int)dir].x, dy = deltas[(int)dir].y;

                int curx = x, cury = y;
                int cnt = 0;

                while (true)
                {
                    int nextx = curx + dx, nexty = cury + dy;
                    if (nextx < 0 || nextx >= Width)
                        break;
                    if (nexty < 0 || nexty >= Height)
                        break;
                    if (map[nexty, nextx] != BLANK)
                        break;

                    map[nexty, nextx] = depth;

                    curx = nextx;
                    cury = nexty;
                    cnt++;
                }
                if (cnt == 0)
                    continue;

                stack.Push(dir);
                // Console.WriteLine($"{string.Join(",", stack)}");
                if (Finished())
                    return true;
                var res = Solve(depth + 1, curx, cury);
                if (res)
                    return true;
                stack.Pop();
                // Rewind
                for (int i = 0; i < cnt; i++)
                {
                    map[cury, curx] = BLANK;
                    curx -= dx;
                    cury -= dy;
                }
            }
            return false;
        }

        static bool Finished()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    if (map[y, x] == BLANK)
                        return false;
            return true;
        }

        enum Direction
        {
            U, D, L, R
        }
    }
}
