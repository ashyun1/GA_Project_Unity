using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeBFS : MonoBehaviour
{
    int[,] map =
    {
        {1, 1, 1, 1, 1, 1, 1},
        {1, 0, 0, 0, 1, 0, 1},
        {1, 0, 1, 0, 0, 0, 1},
        {1, 0, 1, 1, 1, 0, 1},
        {1, 0, 0, 0, 0, 0, 1},
        {1, 1, 1, 1, 1, 0, 1},
        {1, 1, 1, 1, 1, 1, 1},
    };
    Vector2Int start = new Vector2Int(1, 1);
    Vector2Int goal = new Vector2Int(5, 5);
    bool[,] visited;
    Vector2Int?[,] parent;
    Vector2Int[] dirs =
    {
        new Vector2Int( 1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int( 0, 1),
        new Vector2Int( 0,-1),
    };



    // Start is called before the first frame update
    void Start()
    {
        List<Vector2Int> path = FindPathBFS();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    List<Vector2Int> FindPathBFS()
    {
        int w = map.GetLength(0);
        int h = map.GetLength(1);
        visited = new bool[w, h];
        parent = new Vector2Int?[w, h];
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        visited[start.x, start.y] = true;
        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();

            //∏Ò«• µµ¬¯
            if (cur == goal)
            {
                Debug.Log("BFS: Goal µµ¬¯!");
                return ReconstructPath();
            }

            // ≥◊ πÊ«‚ ¿ÃøÙ ≈Ωªˆ
            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;

                if (!InBounds(nx, ny)) continue;
                if (map[nx, ny] == 1) continue;
                if (visited[nx, ny]) continue;

                visited[nx, ny] = true;
                parent[nx, ny] = cur;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }

        Debug.Log("BFS: ∞Ê∑Œ æ¯¿Ω");
        return null;

    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 &&
               x < map.GetLength(0) &&
               y < map.GetLength(1);
    }

    List<Vector2Int> ReconstructPath()
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = goal;

        while (cur.HasValue)
        {
            path.Add(cur.Value);
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse();
        Debug.Log($"∞Ê∑Œ ±Ê¿Ã: {path.Count}");
        foreach(var p in path)
        {
            Debug.Log(p);
        }
        return path;
    
    }
}
