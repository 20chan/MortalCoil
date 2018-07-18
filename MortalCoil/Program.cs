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

                cache = new bool[Height, Width][,];
                for (int y = 0; y < Height; y++)
                {
                    for (int x = 0; x < Width; x++)
                        cache[y, x] = new bool[Height - y, Width - x];
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
            if (depth % 5 == 0)
            {
                var imp = IsImpossible(x, y);
                if (imp)
                    return false;
            }
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

        static bool[,][,] cache;
        static void ClearCache()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    for (int yy = 0; yy < Height - y; yy++)
                        for (int xx = 0; xx < Width - x; xx++)
                            cache[y, x][yy, xx] = false;
        }

        static bool IsImpossible(int x, int y)
        {
            ClearCache();
            return IsImpossible(x, y, x, y, 0, 0);
        }

        static bool IsImpossible(int px, int py, int curx, int cury, int width, int height)
        {
            // Check any area that is covered by wall or trail that player is not near

            if (cache[cury, curx][height, width])
                return false;
            cache[cury, curx][height, width] = true;

            if (curx + width >= Width || cury + height >= Height)
                return false;

            if (IsImpossible(px, py, curx, cury, width + 1, height))
                return true;
            if (IsImpossible(px, py, curx, cury, width, height + 1))
                return true;
            if (IsImpossible(px, py, curx + 1, cury, 0, 0))
                return true;
            if (IsImpossible(px, py, curx, cury + 1, 0, 0))
                return true;
            
            for (int x = curx; x < curx + width; x++)
                if (map[cury, x] == BLANK || map[cury + height, x] == BLANK)
                    return false;
            for (int y = cury; y < cury + height; y++)
                if (map[y, curx] == BLANK || map[y, curx + width] == BLANK)
                    return false;

            System.Diagnostics.Debugger.Break();
            return true;
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
