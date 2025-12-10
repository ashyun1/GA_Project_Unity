using System.Collections.Generic;
using UnityEngine;

public class AStarEnemyExample : MonoBehaviour
{
    public int width = 21;
    public int height = 21;

    public GameObject wallPrefab;
    public GameObject groundPrefab;
    public GameObject forestPrefab;
    public GameObject mudPrefab;
    public GameObject pathPrefab;
    public GameObject enemyPrefab;

    public Transform playerTransform;

    [Range(0f, 1f)] public float wallChance = 0.3f;
    [Range(0f, 1f)] public float groundChance = 0.6f;
    [Range(0f, 1f)] public float forestChance = 0.25f;
    [Range(0f, 1f)] public float mudChance = 0.15f;

    public int enemyCount = 5;
    public int enemySafeDistance = 4;
    public int enemyPenaltyWeight = 4;

    int[,] map;
    Vector2Int start;
    Vector2Int goal;
    List<Vector2Int> currentPath;

    List<GameObject> spawnedMapObjects = new List<GameObject>();
    List<GameObject> spawnedPathObjects = new List<GameObject>();
    List<GameObject> spawnedEnemyObjects = new List<GameObject>();
    List<Vector2Int> enemyPositions = new List<Vector2Int>();

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

        if (playerTransform != null)
        {
            playerTransform.position = new Vector3(start.x, playerTransform.position.y, start.y);
        }
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateAndValidateMap();
            ClearPathVisualization();

            if (playerTransform != null)
            {
                playerTransform.position = new Vector3(start.x, playerTransform.position.y, start.y);
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
                Debug.Log(" 최단거리 탐색 완료! 총 코스트 = " + totalCost);
            }
            else
            {
                Debug.LogError(" 경로 탐색 실패");
            }
        }


        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (currentPath == null) return;
            if (playerTransform == null) return;

            Vector2Int end = currentPath[currentPath.Count - 1];
            playerTransform.position = new Vector3(end.x, playerTransform.position.y, end.y);
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

    void ClearEnemies()
    {
        foreach (var go in spawnedEnemyObjects) Destroy(go);
        spawnedEnemyObjects.Clear();
        enemyPositions.Clear();
    }

    void GenerateAndValidateMap()
    {
        ClearMapVisualization();
        

        int attempts = 0;

        do
        {
            attempts++;
            GenerateRandomMap();
            SpawnEnemies();     

            if (attempts > 100)
            {
                Debug.LogError(" 100회 초과: 탈출 가능한 미로 생성 실패");
                return;
            }

        } while (!IsPathPossible_DFS(start, goal));

        Debug.Log(" 미로 생성 성공! 생성 시도 횟수 = " + attempts);

        VisualizeMap();
    }

    void GenerateRandomMap()
    {
        map = new int[width, height];

        float sum = groundChance + forestChance + mudChance;
        float gRate = groundChance / sum;
        float fRate = forestChance / sum;
        float mRate = mudChance / sum;

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
                    if (Random.value < wallChance)
                    {
                        map[x, y] = 0;
                    }
                    else
                    {
                        float r = Random.value;

                        if (r < gRate)
                            map[x, y] = 1;
                        else if (r < gRate + fRate)
                            map[x, y] = 2;
                        else
                            map[x, y] = 3;
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
                GameObject p = null;

                if (map[x, y] == 0) p = wallPrefab;
                else if (map[x, y] == 1) p = groundPrefab;
                else if (map[x, y] == 2) p = forestPrefab;
                else if (map[x, y] == 3) p = mudPrefab;

                if (p != null)
                {
                    GameObject go = Instantiate(p, new Vector3(x, 0, y), Quaternion.identity, transform);
                    spawnedMapObjects.Add(go);
                }
            }
        }
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null) return;
        ClearEnemies();

        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (map[x, y] != 0)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (pos != start && pos != goal)
                        candidates.Add(pos);
                }
            }
        }

        int spawnCount = 0;

        for (int i = 0; i < enemyCount && candidates.Count > 0; i++)
        {
            int idx = Random.Range(0, candidates.Count);
            Vector2Int cell = candidates[idx];
            candidates.RemoveAt(idx);

            GameObject e = Instantiate(enemyPrefab,
                new Vector3(cell.x, 0.5f, cell.y),
                Quaternion.identity,
                transform);

            spawnedEnemyObjects.Add(e);
            enemyPositions.Add(cell);
            spawnCount++;
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
               
                    if (enemyPositions.Contains(next)) continue;

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

            if (cur == goal)
            {
                return ReconstructPath(parent, start, goal);
            }

            closed.Add(cur);

            foreach (var d in dirs)
            {
                Vector2Int next = cur + d;

                if (!InBounds(next.x, next.y)) continue;
                if (map[next.x, next.y] == 0) continue;
      
                if (enemyPositions.Contains(next)) continue;

                if (closed.Contains(next)) continue;

                float tentative = g[cur] + TileCostWithEnemy(next);

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
        int enemyPenalty = EnemyPenalty(from);
        return manhattan + enemyPenalty;
    }


    int TileCostWithEnemy(Vector2Int p)
    {
        int baseCost = TileCost(map[p.x, p.y]);
        int penalty = EnemyPenalty(p);
        return baseCost + penalty;
    }


    int EnemyPenalty(Vector2Int p)
    {
        if (enemyPositions == null || enemyPositions.Count == 0) return 0;

        int minDist = int.MaxValue;

        for (int i = 0; i < enemyPositions.Count; i++)
        {
            Vector2Int e = enemyPositions[i];
            int d = Mathf.Abs(p.x - e.x) + Mathf.Abs(p.y - e.y);
            if (d < minDist) minDist = d;
        }

        if (minDist > enemySafeDistance) return 0;

        int level = enemySafeDistance - minDist + 1;
        return level * enemyPenaltyWeight;
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
}