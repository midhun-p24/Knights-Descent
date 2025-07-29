using System;
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    // A tiny generic min-heap priority queue
    private class PriorityQueue<T>
    {
        List<(T item, int priority)> data = new List<(T, int)>();

        public int Count => data.Count;

        public void Enqueue(T item, int priority)
        {
            data.Add((item, priority));
            int ci = data.Count - 1;
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (data[ci].priority >= data[pi].priority) break;
                (data[ci], data[pi]) = (data[pi], data[ci]);
                ci = pi;
            }
        }

        public T Dequeue()
        {
            int li = data.Count - 1;
            var front = data[0];
            data[0] = data[li];
            data.RemoveAt(li);
            --li;
            int pi = 0;
            while (true)
            {
                int lci = 2 * pi + 1, rci = 2 * pi + 2, min = pi;
                if (lci <= li && data[lci].priority < data[min].priority) min = lci;
                if (rci <= li && data[rci].priority < data[min].priority) min = rci;
                if (min == pi) break;
                (data[pi], data[min]) = (data[min], data[pi]);
                pi = min;
            }
            return front.item;
        }
    }

    // --- MODIFIED Dijkstra Algorithm ---
    public static List<Vector2Int> Dijkstra(int[,] grid, Vector2Int start, Vector2Int goal)
    {
        int w = grid.GetLength(0), h = grid.GetLength(1);
        var dist = new Dictionary<Vector2Int, int> { [start] = 0 };
        var prev = new Dictionary<Vector2Int, Vector2Int>();
        var visited = new HashSet<Vector2Int>();
        var pq = new PriorityQueue<Vector2Int>();
        pq.Enqueue(start, 0);

        Vector2Int[] dirs = {
            new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        while (pq.Count > 0)
        {
            var u = pq.Dequeue();
            if (u == goal) break;
            if (!visited.Add(u)) continue;

            foreach (var d in dirs)
            {
                var v = u + d;

                // --- NEW: Check for 2-tile vertical clearance ---
                // Check if the destination is within map bounds, allowing space for the tile above.
                if (v.x < 0 || v.x >= w || v.y < 0 || v.y >= h - 1) continue;

                // Both the target tile (feet) and the tile above it (head) must be walkable.
                if (grid[v.x, v.y] != 1 || grid[v.x, v.y + 1] != 1) continue;

                // --- NEW: Prevent clipping on diagonal moves ---
                bool isDiagonal = (d.x != 0 && d.y != 0);
                if (isDiagonal)
                {
                    // To move diagonally, the path must not be blocked by adjacent walls.
                    // e.g., To move from (x,y) to (x+1, y+1), tiles (x+1, y) and (x, y+1) must be clear.
                    if (grid[u.x + d.x, u.y] != 1 || grid[u.x, u.y + d.y] != 1)
                    {
                        continue;
                    }
                }

                int cost = dist[u] + (isDiagonal ? 14 : 10); // Standard costs for grid pathfinding
                if (!dist.ContainsKey(v) || cost < dist[v])
                {
                    dist[v] = cost;
                    prev[v] = u;
                    pq.Enqueue(v, cost);
                }
            }
        }

        // build path
        if (!prev.ContainsKey(goal)) return null; // No path found
        var path = new List<Vector2Int>();
        var cur = goal;
        while (cur != start)
        {
            path.Add(cur);
            cur = prev[cur];
        }
        path.Reverse();
        return path;
    }
}
