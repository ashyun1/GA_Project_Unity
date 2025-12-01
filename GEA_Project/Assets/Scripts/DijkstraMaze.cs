using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DijkstraMaze : MonoBehaviour
{
    public int width = 21;
    public int height = 21;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject groundPrefab;
    public GameObject forestPrefab;
    public GameObject mudPrefab;
    public GameObject pathPrefab;

    [Header("Movement")]
    public PlayerMovement playerMovement;

    int[,] map;
    Vector2Int start;
    Vector2Int goal;
    List<Vector2Int> currentPath;

    private List<GameObject> spawnedMapObjects = new List<GameObject>();
    private List<GameObject> spawnedPathObjects = new List<GameObject>();

    readonly Vector2Int[] dirs = { new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(-1, 0) };

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
            Vector3 startPos = new Vector3(start.x, playerMovement.transform.position.y, start.y);
            playerMovement.transform.position = startPos;
        }

        Debug.Log("맵이 생성되었습니다. E 키로 최단 경로를 찾고, Enter 키로 캐릭터를 이동시키세요.");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateAndValidateMap();
            ClearPathVisualization();
            if (playerMovement != null)
            {
                Vector3 startPos = new Vector3(start.x, playerMovement.transform.position.y, start.y);
                playerMovement.transform.position = startPos;
            }
        }

        // E 키: 최단 경로 탐색 및 시각화만 수행
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (map == null)
            {
                Debug.LogWarning("맵이 생성되지 않았습니다. Space Bar를 눌러 맵을 생성해주세요.");
                return;
            }

            ClearPathVisualization();
            currentPath = Dijkstras(map, start, goal);
            if (currentPath != null)
            {
                VisualizePath(currentPath);
                int totalCost = CalculatePathCost(currentPath);
                Debug.Log($"✅ 최단 경로 탐색 완료. 총 비용: {totalCost}. Enter 키를 눌러 캐릭터를 이동시키세요.");
            }
            else
            {
                Debug.LogError("오류: 다익스트라로 경로를 찾을 수 없습니다.");
            }
        }

        // Enter 키: 이전에 찾은 경로를 따라 캐릭터 이동만 수행
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentPath == null)
            {
                Debug.LogWarning("먼저 E 키를 눌러 최단 경로를 찾아야 합니다.");
                return;
            }

            if (playerMovement != null)
            {
                List<Vector3> path3D = ConvertPathToVector3(currentPath);
                playerMovement.MoveAlongPath(path3D);
            }
            else
            {
                Debug.LogError("Player Movement 컴포넌트(PlayerMovement.cs)가 연결되지 않았습니다.");
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
        const int MAX_ATTEMPTS = 100;

        do
        {
            attempts++;
            GenerateRandomMap();

            if (attempts > MAX_ATTEMPTS)
            {
                Debug.LogError($"맵 생성 시도 {MAX_ATTEMPTS}회 초과. 유효한 맵을 만들 수 없습니다.");
                return;
            }
        } while (!IsPathPossible_DFS(start, goal));

        Debug.Log($"✅ 맵 생성 성공! 시도 횟수: {attempts}. 탈출 가능합니다.");
        VisualizeMap();
    }

    void GenerateRandomMap()
    {
        map = new int[width, height];
        float wallChance = 0.3f;

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
                    {
                        map[x, y] = 0;
                    }
                    else
                    {
                        if (r < 0.6f) map[x, y] = 1;
                        else if (r < 0.85f) map[x, y] = 2;
                        else map[x, y] = 3;
                    }
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
                GameObject prefabToSpawn = null;
                switch (map[x, y])
                {
                    case 0: prefabToSpawn = wallPrefab; break;
                    case 1: prefabToSpawn = groundPrefab; break;
                    case 2: prefabToSpawn = forestPrefab; break;
                    case 3: prefabToSpawn = mudPrefab; break;
                }

                if (prefabToSpawn != null)
                {
                    GameObject go = Instantiate(prefabToSpawn, new Vector3(x, 0, y), Quaternion.identity, transform);
                    spawnedMapObjects.Add(go);
                }
            }
        }
    }

    bool IsPathPossible_DFS(Vector2Int current, Vector2Int end)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        stack.Push(current);
        visited.Add(current);

        while (stack.Count > 0)
        {
            Vector2Int cur = stack.Pop();

            if (cur == end) return true;

            foreach (var dir in dirs)
            {
                Vector2Int next = cur + dir;
                if (InBounds(map, next.x, next.y) &&
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

    List<Vector2Int> Dijkstras(int[,] map, Vector2Int start, Vector2Int goal)
    {
        SimplePriorityQueue<Vector2Int> pq = new SimplePriorityQueue<Vector2Int>();
        Dictionary<Vector2Int, float> dist = new Dictionary<Vector2Int, float>();
        Dictionary<Vector2Int, Vector2Int> parent = new Dictionary<Vector2Int, Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                dist[new Vector2Int(x, y)] = float.MaxValue;
            }
        }

        dist[start] = 0;
        pq.Enqueue(start, 0);

        while (pq.Count > 0)
        {
            Vector2Int cur = pq.Dequeue();

            if (cur == goal)
            {
                return ReconstructPath(parent, start, goal);
            }

            foreach (var dir in dirs)
            {
                Vector2Int next = cur + dir;
                int nx = next.x;
                int ny = next.y;

                if (!InBounds(map, nx, ny)) continue;
                if (map[nx, ny] == 0) continue;

                int moveCost = TileCost(map[nx, ny]);
                float newDist = dist[cur] + moveCost;

                if (newDist < dist[next])
                {
                    dist[next] = newDist;
                    parent[next] = cur;

                    pq.UpdatePriority(next, newDist);
                }
            }
        }

        return null;
    }

    int TileCost(int tileType)
    {
        switch (tileType)
        {
            case 1: return 1;
            case 2: return 3;
            case 3: return 5;
            case 0: return int.MaxValue;
            default: return 1;
        }
    }

    bool InBounds(int[,] map, int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> parent, Vector2Int start, Vector2Int goal)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = goal;

        while (parent.ContainsKey(current))
        {
            path.Add(current);
            current = parent[current];
            if (current == start)
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
        if (path == null) return;

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int pos = path[i];
            GameObject go = Instantiate(pathPrefab, new Vector3(pos.x, 0.1f, pos.y), Quaternion.identity, transform);
            spawnedPathObjects.Add(go);
        }
    }

    int CalculatePathCost(List<Vector2Int> path)
    {
        if (path == null || path.Count < 2) return 0;

        int totalCost = 0;
        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int currentPos = path[i];
            totalCost += TileCost(map[currentPos.x, currentPos.y]);
        }
        return totalCost;
    }

    List<Vector3> ConvertPathToVector3(List<Vector2Int> path2D)
    {
        List<Vector3> path3D = new List<Vector3>();
        foreach (var point in path2D)
        {
            path3D.Add(new Vector3(point.x, 0f, point.y));
        }
        return path3D;
    }
}