using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MazeGenerator : MonoBehaviour
{
    public int mazeWidth = 17;
    public int mazeHeight = 11;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject pathMarkerPrefab;
    public float cellSize = 1f;

    public float wallGenerationProbability = 0.5f;

    private int[,] map;
    private Vector2Int goal;

    private List<Vector2Int> bfsSolutionPath = new List<Vector2Int>();
    private List<Vector2Int> dfsSolutionPath = new List<Vector2Int>();

    private int[,] distanceMap;
    private Vector2Int farthestPos = Vector2Int.zero;


    private GameObject currentMazeParent;

    private readonly Vector2Int[] dirs = {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0)
    };

    private Vector2Int startPos = new Vector2Int(1, 1);

    private PlayerMovement playerMovement;

    void Start()
    {
        if (mazeWidth < 5) mazeWidth = 5;
        if (mazeHeight < 5) mazeHeight = 5;

        goal = new Vector2Int(mazeWidth - 2, mazeHeight - 2);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerMovement = player.GetComponent<PlayerMovement>();
            player.transform.position = GetWorldPosition(startPos.x, startPos.y);
        }
        else
        {
            Debug.LogError("씬에서 'Player' 태그를 가진 오브젝트와 PlayerMovement 스크립트를 찾을 수 없습니다. 설정이 필요합니다.");
        }

        GenerateAndSolveMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateNewMaze();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ClearAllPaths();
            VisualizeSolution(dfsSolutionPath, "DFSSolutionMarkers", Color.green);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            VisualizeFarthestPoint();
        }
    }

    public void GenerateNewMaze()
    {
        ClearAllVisualizations();
        DestroyExistingObjects();

        GenerateAndSolveMap();

        if (playerMovement != null)
        {
            playerMovement.transform.position = GetWorldPosition(startPos.x, startPos.y);
            playerMovement.StopMovement();
        }
    }

    public void ShowPathButton()
    {
        ClearAllPaths();
        VisualizeSolution(bfsSolutionPath, "BFSSolutionMarkers", Color.green);
    }

    public void AutoMoveButton()
    {
        if (playerMovement != null && bfsSolutionPath.Count > 0)
        {
            List<Vector3> worldPath = new List<Vector3>();
            foreach (var gridPos in bfsSolutionPath)
            {
                worldPath.Add(GetWorldPosition(gridPos.x, gridPos.y));
            }

            playerMovement.MoveAlongPath(worldPath);
        }
        else
        {
            Debug.LogWarning("PlayerMovement 스크립트가 없거나 최단 경로가 비어 있습니다.");
        }
    }

    private void GenerateAndSolveMap()
    {
        bool isSolvable = false;
        int maxAttempts = 500;
        int attempts = 0;

        while (!isSolvable && attempts < maxAttempts)
        {
            map = new int[mazeWidth, mazeHeight];
            distanceMap = new int[mazeWidth, mazeHeight];
            GenerateRandomMapWithProbability();

            bfsSolutionPath.Clear();
            dfsSolutionPath.Clear();

            List<Vector2Int> bfsPath = FindPathBFS();

            if (bfsPath != null)
            {
                isSolvable = true;
                bfsSolutionPath = bfsPath;

                dfsSolutionPath = FindPathDFS();

                if (dfsSolutionPath == null)
                {
                    dfsSolutionPath = bfsSolutionPath;
                }
            }
            attempts++;
        }

        if (isSolvable)
        {
            Debug.Log($"탈출 가능한 맵 생성! (시도 횟수: {attempts}, 최단 경로 길이(BFS): {bfsSolutionPath.Count})");
            FindFarthestPoint();
            VisualizeMap();
        }
        else
        {
            Debug.LogError($" 시도 횟수({maxAttempts}회) 내에 탈출 가능한 맵을 생성하지 못했습니다. 확률({wallGenerationProbability})을 조정하거나 Space를 눌러 다시 시도하세요.");
            VisualizeMap();
        }
    }

    private void GenerateRandomMapWithProbability()
    {
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (x == 0 || x == mazeWidth - 1 || y == 0 || y == mazeHeight - 1)
                {
                    map[x, y] = 1;
                }
                else if ((x == startPos.x && y == startPos.y) || (x == goal.x && y == goal.y))
                {
                    map[x, y] = 0;
                }
                else
                {
                    if (Random.value < wallGenerationProbability)
                    {
                        map[x, y] = 1;
                    }
                    else
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }
    }

    private List<Vector2Int> FindPathBFS()
    {
        int w = mazeWidth;
        int h = mazeHeight;
        bool[,] visited = new bool[w, h];
        Vector2Int?[,] parent = new Vector2Int?[w, h];
        Queue<Vector2Int> q = new Queue<Vector2Int>();

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                distanceMap[x, y] = -1;
            }
        }

        q.Enqueue(startPos);
        visited[startPos.x, startPos.y] = true;
        distanceMap[startPos.x, startPos.y] = 0;

        List<Vector2Int> path = null;

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();

            if (cur == goal)
            {
                path = ReconstructPath(parent, goal);
            }

            int currentDist = distanceMap[cur.x, cur.y];

            foreach (var d in dirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;
                Vector2Int nextPos = new Vector2Int(nx, ny);

                if (!InBounds(nx, ny)) continue;
                if (map[nx, ny] == 1) continue;
                if (visited[nx, ny]) continue;

                visited[nx, ny] = true;
                parent[nx, ny] = cur;
                distanceMap[nx, ny] = currentDist + 1;
                q.Enqueue(nextPos);
            }
        }

        return path;
    }

    private void FindFarthestPoint()
    {
        int maxDist = -1;
        farthestPos = startPos;

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (distanceMap[x, y] > maxDist)
                {
                    maxDist = distanceMap[x, y];
                    farthestPos = new Vector2Int(x, y);
                }
            }
        }

        if (maxDist > 0)
        {
            Debug.Log($"가장 먼 칸 발견: 좌표 {farthestPos}, 거리: {maxDist}");
        }
        else
        {
            Debug.LogWarning("가장 먼 칸을 찾지 못했습니다. (도달 가능한 길 없음)");
        }
    }

    public void VisualizeFarthestPoint()
    {
        GameObject existingFarthestMarker = GameObject.Find("FarthestMarker");
        if (existingFarthestMarker != null)
        {
            Destroy(existingFarthestMarker);
        }

        if (pathMarkerPrefab == null)
        {
            Debug.LogError("PathMarkerPrefab이 연결되지 않았습니다.");
            return;
        }

        if (distanceMap == null || farthestPos == startPos)
        {
            Debug.LogWarning("가장 먼 칸 정보가 없습니다. (맵을 먼저 생성하세요)");
            return;
        }

        GameObject markerParent = new GameObject("FarthestMarker");

        Vector3 pos = GetWorldPosition(farthestPos.x, farthestPos.y);
        pos.y = 0.1f;

        GameObject marker = Instantiate(pathMarkerPrefab, pos, Quaternion.identity, markerParent.transform);

        MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.yellow;
            renderer.material.SetColor("_EmissionColor", Color.yellow);
        }

        Debug.Log($"시작점으로부터 가장 먼 칸({farthestPos})을 노란색으로 표시했습니다.");
    }

    private List<Vector2Int> FindPathDFS()
    {
        int w = mazeWidth;
        int h = mazeHeight;
        bool[,] visited = new bool[w, h];
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        Dictionary<Vector2Int, Vector2Int> parentMap = new Dictionary<Vector2Int, Vector2Int>();

        stack.Push(startPos);
        visited[startPos.x, startPos.y] = true;

        while (stack.Count > 0)
        {
            Vector2Int cur = stack.Pop();

            if (cur == goal)
            {
                return ReconstructPath(parentMap, goal);
            }

            List<Vector2Int> shuffledDirs = new List<Vector2Int>(dirs);
            for (int i = 0; i < shuffledDirs.Count; i++)
            {
                int r = Random.Range(i, shuffledDirs.Count);
                Vector2Int temp = shuffledDirs[i];
                shuffledDirs[i] = shuffledDirs[r];
                shuffledDirs[r] = temp;
            }

            foreach (var d in shuffledDirs)
            {
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;
                Vector2Int nextPos = new Vector2Int(nx, ny);

                if (!InBounds(nx, ny)) continue;
                if (map[nx, ny] == 1) continue;
                if (visited[nx, ny]) continue;

                visited[nx, ny] = true;
                parentMap[nextPos] = cur;
                stack.Push(nextPos);
            }
        }
        return null;
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < mazeWidth && y < mazeHeight;
    }

    private List<Vector2Int> ReconstructPath(Vector2Int?[,] parent, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int? cur = target;

        while (cur.HasValue)
        {
            path.Add(cur.Value);
            cur = parent[cur.Value.x, cur.Value.y];
        }

        path.Reverse();
        return path;
    }

    private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> parentMap, Vector2Int target)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int cur = target;

        while (parentMap.ContainsKey(cur))
        {
            path.Add(cur);
            cur = parentMap[cur];
        }
        path.Add(startPos);

        path.Reverse();
        return path;
    }


    private void VisualizeMap()
    {
        if (currentMazeParent != null) Destroy(currentMazeParent);

        currentMazeParent = new GameObject("CurrentMaze");
        currentMazeParent.transform.position = Vector3.zero;

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                Vector3 pos = GetWorldPosition(x, y);

                if (map[x, y] == 1)
                {
                    if (wallPrefab != null)
                    {
                        Instantiate(wallPrefab, pos, Quaternion.identity, currentMazeParent.transform);
                    }
                }

                if (map[x, y] == 0)
                {
                    if (floorPrefab != null)
                    {
                        pos.y = -0.01f;
                        Instantiate(floorPrefab, pos, Quaternion.identity, currentMazeParent.transform);
                    }
                }
            }
        }
    }

    private void VisualizeSolution(List<Vector2Int> path, string parentObjectName, Color markerColor)
    {
        GameObject existingMarkersParent = GameObject.Find(parentObjectName);
        if (existingMarkersParent != null)
        {
            Destroy(existingMarkersParent);
        }

        if (pathMarkerPrefab == null)
        {
            Debug.LogError("PathMarkerPrefab이 연결되지 않았습니다.");
            return;
        }

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("경로가 비어 있습니다. 맵이 해결 가능한지 확인하세요.");
            return;
        }

        GameObject markerParent = new GameObject(parentObjectName);

        for (int i = 0; i < path.Count; i++)
        {
            Vector2Int gridPos = path[i];
            Vector3 pos = GetWorldPosition(gridPos.x, gridPos.y);

            pos.y = 0.1f;

            GameObject marker = Instantiate(pathMarkerPrefab, pos, Quaternion.identity, markerParent.transform);
            MeshRenderer renderer = marker.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material.color = markerColor;
                renderer.material.SetColor("_EmissionColor", markerColor);
            }
        }

        Debug.Log($"[{parentObjectName}] 경로를 시각화했습니다. 총 길이: {path.Count} 타일");
    }

    private void ClearAllPaths()
    {
        GameObject bfsMarkers = GameObject.Find("BFSSolutionMarkers");
        if (bfsMarkers != null) Destroy(bfsMarkers);

        GameObject dfsMarkers = GameObject.Find("DFSSolutionMarkers");
        if (dfsMarkers != null) Destroy(dfsMarkers);
    }

    private void ClearAllVisualizations()
    {
        GameObject bfsMarkers = GameObject.Find("BFSSolutionMarkers");
        if (bfsMarkers != null) Destroy(bfsMarkers);

        GameObject dfsMarkers = GameObject.Find("DFSSolutionMarkers");
        if (dfsMarkers != null) Destroy(dfsMarkers);

        GameObject farthestMarker = GameObject.Find("FarthestMarker");
        if (farthestMarker != null) Destroy(farthestMarker);
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        float xOffset = (mazeWidth * cellSize) / 2f - cellSize / 2f;
        float zOffset = (mazeHeight * cellSize) / 2f - cellSize / 2f;

        return new Vector3(x * cellSize - xOffset, 0f, y * cellSize - zOffset);
    }

    private void DestroyExistingObjects()
    {
        if (currentMazeParent != null)
        {
            Destroy(currentMazeParent);
            currentMazeParent = null;
        }
    }
}