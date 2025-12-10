using System.Collections.Generic;
using UnityEngine;

public class AStarExample : MonoBehaviour
{
    public int width = 21;
    public int height = 21;

    public GameObject wallPrefab;
    public GameObject groundPrefab;
    public GameObject forestPrefab;
    public GameObject mudPrefab;
    public GameObject pathPrefab;

    public PlayerMovement playerMovement;

    int[,] map;
    Vector2Int start;
    Vector2Int goal;
    List<Vector2Int> currentPath;

    List<GameObject> spawnedMapObjects = new List<GameObject>();
    List<GameObject> spawnedPathObjects = new List<GameObject>();

    readonly Vector2Int[] dirs =
    {
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0)
    };

    void Awake()
    {
        start = new Vector2Int(1, 1);
        goal = new Vector2Int(width - 2, height - 2);
    }

    void Start()
    {
        GenerateAndValidateMap();

        if (playerMovement != null)
        {
            Vector3 pos = new Vector3(start.x, playerMovement.transform.position.y, start.y);
            playerMovement.transform.position = pos;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateAndValidateMap();
            ClearPathVisualization();

            if (playerMovement != null)
            {
                Vector3 pos = new Vector3(start.x, playerMovement.transform.position.y, start.y);
                playerMovement.transform.position = pos;
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (map == null) return;

            ClearPathVisualization();
            currentPath = AStar(map, start, goal);

            if (currentPath != null)
            {
                VisualizePath(currentPath);

                int totalCost = CalculatePathCost(currentPath);
                Debug.Log("최단거리 탐색 완료! 총 코스트 = " + totalCost);
            }
            else
            {
                Debug.LogError("경로 탐색 실패");
            }
        }


        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentPath == null) return;

            if (playerMovement != null)
            {
                List<Vector3> p3 = ConvertPathToVector3(currentPath);
                playerMovement.MoveAlongPath(p3);
            }
        }
    }

    void ClearMapVisualization()
    {
        foreach (var go in spawnedMapObjects) Destroy(go);
        spawnedMapObjects.Clear();
    }

    void ClearPathVisualization()
    {
        foreach (var go in spawnedPathObjects) Destroy(go);
        spawnedPathObjects.Clear();
    }

    void GenerateAndValidateMap()
    {
        ClearMapVisualization();

        int attempts = 0;

        do
        {
            attempts++;
            GenerateRandomMap();

            if (attempts > 100)
            {
                Debug.LogError(" 100회 초과: 탈출 가능한 미로 생성 실패");
                return;
            }

        } while (!IsPathPossible_DFS(start, goal));

        Debug.Log("미로 생성 성공! 생성 시도 횟수 = " + attempts);

        VisualizeMap();
    }

    int CalculatePathCost(List<Vector2Int> path)
    {
        int total = 0;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int p = path[i];
            total += TileCost(map[p.x, p.y]);
        }

        return total;
    }


    void GenerateRandomMap()
    {
        map = new int[width, height];
        float wallChance = 0.1f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    map[x, y] = 0;
                }
                else
                {
                    float r = Random.value;

                    if (Random.value < wallChance)
                        map[x, y] = 0;
                    else if (r < 0.6f)
                        map[x, y] = 1;
                    else if (r < 0.85f)
                        map[x, y] = 2;
                    else
                        map[x, y] = 3;
                }
            }
        }

        map[start.x, start.y] = 1;
        map[goal.x, goal.y] = 1;
    }

    void VisualizeMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject prefab = null;

                if (map[x, y] == 0) prefab = wallPrefab;
                else if (map[x, y] == 1) prefab = groundPrefab;
                else if (map[x, y] == 2) prefab = forestPrefab;
                else if (map[x, y] == 3) prefab = mudPrefab;

                if (prefab != null)
                {
                    GameObject go = Instantiate(prefab, new Vector3(x, 0, y), Quaternion.identity, transform);
                    spawnedMapObjects.Add(go);
                }
            }
        }
    }

    bool IsPathPossible_DFS(Vector2Int s, Vector2Int e)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        stack.Push(s);
        visited.Add(s);

        while (stack.Count > 0)
        {
            Vector2Int cur = stack.Pop();
            if (cur == e) return true;

            foreach (var d in dirs)
            {
                Vector2Int next = cur + d;

                if (InBounds(next.x, next.y) &&
                    !visited.Contains(next) &&
                    map[next.x, next.y] != 0)
                {
                    visited.Add(next);
                    stack.Push(next);
                }
            }
        }

        return false;
    }

    List<Vector2Int> AStar(int[,] map, Vector2Int start, Vector2Int goal)
    {
        SimplePriorityQueue<Vector2Int> open = new SimplePriorityQueue<Vector2Int>();
        Dictionary<Vector2Int, float> g = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, float> f = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int p = new Vector2Int(x, y);
                g[p] = float.MaxValue;
                f[p] = float.MaxValue;
            }
        }

        g[start] = 0;
        f[start] = Heuristic(start, goal);
        open.Enqueue(start, f[start]);

        while (open.Count > 0)
        {
            Vector2Int cur = open.Dequeue();

            if (cur == goal) return ReconstructPath(parent, start, goal);

            closed.Add(cur);

            foreach (var d in dirs)
            {
                Vector2Int next = cur + d;

                if (!InBounds(next.x, next.y)) continue;
                if (map[next.x, next.y] == 0) continue;
                if (closed.Contains(next)) continue;

                float tentative = g[cur] + TileCost(map[next.x, next.y]);

                if (tentative < g[next])
                {
                    parent[next] = cur;
                    g[next] = tentative;
                    float newF = tentative + Heuristic(next, goal);
                    f[next] = newF;

                    open.UpdatePriority(next, newF);
                }
            }
        }

        return null;
    }

    float Heuristic(Vector2Int from, Vector2Int to)
    {
        int dx = Mathf.Abs(from.x - to.x);
        int dy = Mathf.Abs(from.y - to.y);
        int manhattan = dx + dy;

        int wallPenalty = NearWall(from) ? 2 : 0;

        return manhattan + wallPenalty;
    }

    bool NearWall(Vector2Int p)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int nx = p.x + dx;
                int ny = p.y + dy;

                if (InBounds(nx, ny) && map[nx, ny] == 0)
                    return true;
            }
        }

        return false;
    }

    int TileCost(int tile)
    {
        if (tile == 1) return 1;
        if (tile == 2) return 3;
        if (tile == 3) return 5;
        return int.MaxValue;
    }

    bool InBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int cur = goal;

        while (parent.ContainsKey(cur))
        {
            path.Add(cur);
            cur = parent[cur];

            if (cur == start)
            {
                path.Add(start);
                break;
            }
        }

        path.Reverse();
        return path;
    }

    void VisualizePath(List<Vector2Int> path)
    {
        foreach (var p in path)
        {
            GameObject go = Instantiate(pathPrefab, new Vector3(p.x, 0.1f, p.y), Quaternion.identity, transform);
            spawnedPathObjects.Add(go);
        }
    }

    List<Vector3> ConvertPathToVector3(List<Vector2Int> list)
    {
        List<Vector3> r = new List<Vector3>();
        foreach (var p in list) r.Add(new Vector3(p.x, 0, p.y));
        return r;
    }
}
