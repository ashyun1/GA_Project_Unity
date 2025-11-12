using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    public int mazeWidth = 17;
    public int mazeHeight = 11;
    public GameObject wallPrefab;
    public GameObject pathMarkerPrefab;
    public float cellSize = 1f;

    public float wallGenerationProbability = 0.5f;

    private int[,] map;
    private bool[,] visited;
    private Vector2Int goal;
    private List<Vector3> solutionPath = new List<Vector3>();
    private GameObject currentMazeParent;

    private readonly Vector2Int[] dirs = {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0)
    };

    private Vector2Int startPos = new Vector2Int(1, 1);

    void Start()
    {
        if (mazeWidth < 5) mazeWidth = 5;
        if (mazeHeight < 5) mazeHeight = 5;

        goal = new Vector2Int(mazeWidth - 2, mazeHeight - 2);

        GenerateAndSolveMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateAndSolveMap();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            VisualizeSolution();
        }
    }

    private void GenerateAndSolveMap()
    {
        DestroyExistingObjects();

        bool isSolvable = false;
        int maxAttempts = 500;
        int attempts = 0;

        while (!isSolvable && attempts < maxAttempts)
        {
            map = new int[mazeWidth, mazeHeight];
            GenerateRandomMapWithProbability();

            visited = new bool[mazeWidth, mazeHeight];
            solutionPath.Clear();

            isSolvable = SearchMaze(startPos.x, startPos.y);
            attempts++;
        }

        if (isSolvable)
        {
            Debug.Log($"🎉 탈출 가능한 맵 생성! (시도 횟수: {attempts}, 경로 길이: {solutionPath.Count})");
            VisualizeMap();
        }
        else
        {
            Debug.LogError($"⚠️ 시도 횟수({maxAttempts}회) 내에 탈출 가능한 맵을 생성하지 못했습니다. 확률({wallGenerationProbability})을 조정하세요.");
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

    private bool SearchMaze(int x, int y)
    {
        if (x < 0 || x >= mazeWidth || y < 0 || y >= mazeHeight || map[x, y] == 1 || visited[x, y])
            return false;

        visited[x, y] = true;
        solutionPath.Add(GetWorldPosition(x, y));

        if (x == goal.x && y == goal.y)
            return true;

        foreach (var d in dirs)
        {
            if (SearchMaze(x + d.x, y + d.y))
                return true;
        }

        solutionPath.RemoveAt(solutionPath.Count - 1);

        return false;
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
                if (map[x, y] == 1)
                {
                    Vector3 pos = GetWorldPosition(x, y);
                    Instantiate(wallPrefab, pos, Quaternion.identity, currentMazeParent.transform);
                }
            }
        }
    }

    private void VisualizeSolution()
    {
        GameObject solutionMarkers = GameObject.Find("SolutionMarkers");
        if (solutionMarkers != null)
        {
            Destroy(solutionMarkers);
        }

        GameObject markerParent = new GameObject("SolutionMarkers");

        for (int i = 0; i < solutionPath.Count; i++) // 시작/끝점 포함
        {
            Vector3 pos = solutionPath[i];
            pos.y = 0.1f;
            Instantiate(pathMarkerPrefab, pos, Quaternion.identity, markerParent.transform);
        }

        Debug.Log($"탈출 경로를 시각화했습니다. 총 길이: {solutionPath.Count} 타일");
    }

    private Vector3 GetWorldPosition(int x, int y)
    {
        float xOffset = (mazeWidth * cellSize) / 2f - cellSize / 2f;
        float zOffset = (mazeHeight * cellSize) / 2f - cellSize / 2f;

        return new Vector3(x * cellSize - xOffset, 0f, y * cellSize - zOffset);
    }

    private void DestroyExistingObjects()
    {
        // 최상위 부모 오브젝트를 참조 변수로 관리하여 파괴
        if (currentMazeParent != null)
        {
            Destroy(currentMazeParent);
            currentMazeParent = null;
        }

        // 혹시 남아있을 수 있는 SolutionMarkers 제거
        GameObject solutionMarkers = GameObject.Find("SolutionMarkers");
        if (solutionMarkers != null)
        {
            Destroy(solutionMarkers);
        }
    }
}