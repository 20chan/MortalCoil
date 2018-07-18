using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MortalCoil
{
    public class Graph
    {
        List<int>[] adj;
        public Graph(int len)
        {
            adj = new List<int>[len];
            for (int i = 0; i < len; i++)
                adj[i] = new List<int>();
        }

        public void Add(int s, int d)
        {
            adj[s].Add(d);
            adj[d].Add(s);
        }

        public bool BFS(int s, int d)
        {
            if (s == d)
                return true;

            var visited = new bool[adj.Length];
            for (int i = 0; i < visited.Length; i++)
                visited[i] = false;

            visited[s] = true;
            var queue = new Queue<int>();
            queue.Enqueue(s);

            while(queue.Count > 0)
            {
                s = queue.Dequeue();
                foreach (var i in adj[s])
                {
                    if (i == d)
                        return true;

                    if (!visited[i])
                    {
                        visited[i] = true;
                        queue.Enqueue(i);
                    }
                }
            }
            return false;
        }
    }
}
