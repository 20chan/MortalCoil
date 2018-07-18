using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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
            var driver = new ChromeDriver();
            driver.Url = "http://www.hacker.org/forum/login.php";
            driver.FindElementByName("username").SendKeys(Properties.Settings.Default.ID);
            driver.FindElementByName("password").SendKeys(Properties.Settings.Default.Password);
            driver.FindElementByName("login").Click();
            driver.Url = "http://www.hacker.org/coil/index.php";
            while (true)
            {
                // var raw = "x=6&y=4&board=...X...X........X...X...";
                // var raw = Console.ReadLine();
                var e = driver.FindElementByTagName("embed");
                var raw = e.GetAttribute("flashvars");
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
                var now = DateTime.Now;
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
                Console.WriteLine($"Elapsed time: {(DateTime.Now - now).TotalMilliseconds} ms");
                Console.WriteLine($"Starts at {curx}, {cury}");
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                        Console.Write($"{map[y, x],3}");
                    Console.WriteLine();
                }
                driver.Url = $"http://www.hacker.org/coil/index.php?x={curx}&y={cury}&path={string.Join("", stack.Reverse())}";
            }
        }

        static bool Solve(int x, int y)
        {
            map[y, x] = 1;
            var res = Solve(1, x, y, Direction.None);
            if (res)
                return true;
            map[y, x] = BLANK;
            return false;
        }

        static bool Solve(int depth, int x, int y, Direction from)
        {
            var list = new List<(int x, int y)>();
            if (IsBlank(x - 1, y))
                list.Add((x - 1, y));
            if (IsBlank(x + 1, y))
                list.Add((x + 1, y));
            if (IsBlank(x, y - 1))
                list.Add((x, y - 1));
            if (IsBlank(x, y + 1))
                list.Add((x, y + 1));

            foreach (var s in list)
                foreach (var d in list)
                    if (!IsReachable(s.x, s.y, d.x, d.y))
                        return false;

            (int x, int y)[] deltas =
            {
                (0, -1), (0, 1), (-1, 0), (1, 0)
            };

            foreach (var dir in AvailableDirections(from))
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
                var res = Solve(depth + 1, curx, cury, dir);
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

        static bool IsBlank(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height && map[y, x] == BLANK;
        }

        static bool IsReachable(int sx, int sy, int dx, int dy)
        {
            int s = -1, d = -1;
            int size = Width * Height + 1;
            var g = new Graph(size);

            int k = 0;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    if (map[y, x] == BLANK)
                    {
                        if (IsBlank(x + 1, y))
                            g.Add(k, k + 1);
                        if (IsBlank(x - 1, y))
                            g.Add(k, k - 1);
                        if (IsBlank(x, y + 1))
                            g.Add(k, k + Width);
                        if (IsBlank(x, y - 1))
                            g.Add(k, k - Width);
                    }

                    if (x == sx && y == sy)
                        s = k;
                    if (x == dx && y == dy)
                        d = k;

                    k++;
                }
            return g.BFS(s, d);
        }
        
        enum Direction
        {
            U, D, L, R, None
        }

        static Direction[] AvailableDirections(Direction dir)
        {
            if (dir == Direction.None)
                return new[] { Direction.U, Direction.D, Direction.L, Direction.R };
            else if (dir == Direction.L || dir == Direction.R)
                return new[] { Direction.U, Direction.D };
            else
                return new[] { Direction.L, Direction.R };
        }
    }
}
