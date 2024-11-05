using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

public class WFC : MonoBehaviour
{
    [SerializeField] int width = 16;
    [SerializeField] int height = 16;
    [SerializeField] CandidateSelection candidateSelection;
    [SerializeField] TileSelection tileSelection;
    [SerializeField] float timePerStep = 0.01f;

    [SerializeField] WFCTile[] tiles;

    float timer;
    bool play;

    internal WFCRunner runner;

    /// <summary>
    /// Parameters: x, y, id
    /// </summary>
    public event Action<RectInt>? OnRectChanged;
    public int Width => width;
    public int Height => height;
    public WFCTile[]? Tiles { get { return tiles; } set { tiles = value; } }

    /// <summary>
    /// Null means it's not collapsed yet and doesn't have a tile.
    /// </summary>
    public WFCTile[,] CollapsedCells
    {
        get
        {
            WFCTile[,] cells = new WFCTile[width, height];

            for (int y = 0; y < height; y++)
            {
                var row = runner.cells[y];
                for (int x = 0; x < width; x++)
                {
                    if (runner.IsCollapsed(x, y))
                        cells[x, y] = tiles[row[x].First()];
                }
            }

            return cells;
        }
    }

    //public void CollapseWithStartMapGrid(int[,] startMapGrid, WFCTile[] startMapTiles)
    //{
    //    //NullCheck();
    //    Tiles = startMapTiles;

    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            int tileId = startMapGrid[x, y];

    //            WFCTile tile = GetTileById(tileId);

    //            //if (tile != null && !IsCollapsed(x, y))
    //            {
    //                SetCell(x, y, tile);
    //            }
    //        }
    //    }
    //}

    private WFCTile GetTileById(int id)
    {
        foreach (WFCTile tile in Tiles)
        {
            if (tile.id == id)
                return tile;
        }
        return null;
    }

    [MakeButton]
    void AlignCamera()
    {
        Camera camera = Camera.main;
        camera.transform.position = new Vector3(width / 2, height / 2, -1);
        camera.orthographicSize = Math.Max(width, height) / 2;
    }

    [MakeButton]
    public void ClearGrid()
    {
        runner = new WFCRunner(width, height, tiles.Select(t => new WfcTileData()
        {
            id = t.id,
            weight = t.weight,
            left = t.left,
            right = t.right,
            top = t.top,
            bottom = t.bottom,
        }).ToList());

        runner.CandidateSelection = candidateSelection;
        runner.TileSelection = tileSelection;

        runner.Reset();

        RefreshAllTiles();
    }

    public void RefreshAllTiles()
    {
        OnRectChanged?.Invoke(new RectInt(0, 0, width, height));
    }

    [MakeButton(false)]
    void Play()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("WFC - Can only play in play mode");
            return;
        }

        play = true;
    }

    [MakeButton]
    void Pause()
    {
        play = false;
    }

    void TryPlay()
    {
        if (!play)
            return;

        timer += Time.deltaTime;

        // this is bad if one step actually takes longer to do than timePerStep, can we check that using some system time?
        while (play && timer >= timePerStep)
        {
            timer -= timePerStep;
            if (!StepCore())
            {
                break;
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!play)
            {
                AlignCamera();
                ClearGrid();
            }
            play = !play;
        }
        TryPlay();
    }

    [MakeButton]
    void Step()
    {
        if (!Application.isPlaying)
        {
            Debug.Log("WFC - Can only step in play mode");
            return;
        }

        StepCore();
    }

    bool StepCore()
    {
        var result = runner.Step(out Point point);
        switch (result)
        {
            case WFCRunner.StepResult.OneStep:
                OnRectChanged?.Invoke(new RectInt(point.x, point.y, 1, 1));
                return true;

            case WFCRunner.StepResult.Complete:
                Debug.Log("WFC - Done");
                play = false;
                return false;

            case WFCRunner.StepResult.NoSolution:
                Debug.LogWarning($"WFC - Tried to collapse but had no solutions: {point}");
                play = false;
                return false;

            default:
                Debug.LogError($"Unknown result: {point}");
                return false;
        }
    }
}

public enum CandidateSelection
{
    Random,
    Ordered,
    FurthestFromCenter
}

public enum TileSelection
{
    Random,
    Ordered,
    Weighted
}

public struct WfcTileData
{
    // used to identify the tile
    public int id;

    // used to change the frequency of the tile
    public float weight;

    // connectors of the tile, used to see which pieces can be around it
    public List<ConnectorDef> left;
    public List<ConnectorDef> right;
    public List<ConnectorDef> top;
    public List<ConnectorDef> bottom;
}

public class WFCRunner
{
#nullable enable

    int width;
    int height;
    CandidateSelection candidateSelection;
    TileSelection tileSelection;
    List<WfcTileData> tiles;

    internal HashSet<int>[][] cells;
    BitArray[] collapsed;

    Stack<Point> pStack = new();

    List<Point> candidateList = new();
    List<Point> furthestList = new();
    HashSet<int> neighborConnectors = new();
    HashSet<int> foundConnectors = new();

    public CandidateSelection CandidateSelection
    {
        get => candidateSelection;
        set => candidateSelection = value;
    }

    public TileSelection TileSelection
    {
        get => tileSelection;
        set => tileSelection = value;
    }

    public WFCRunner(int width, int height, List<WfcTileData> tiles)
    {
        this.width = width;
        this.height = height;
        this.tiles = tiles;

        this.cells = new HashSet<int>[height][];
        this.collapsed = new BitArray[height];

        for (int y = 0; y < height; y++)
        {
            collapsed[y] = new BitArray(width);

            var cellRow = new HashSet<int>[width];
            cells[y] = cellRow;

            for (int x = 0; x < width; x++)
            {
                cellRow[x] = new HashSet<int>();
            }
        }
    }

    public void SetCell(int x, int y, int tile)
    {
        if (IsCollapsed(x, y))
            UnCollapse(x, y);

        cells[y][x].Clear();
        cells[y][x].Add(tile);
        Collapse(x, y);
        pStack.Push(new Point(x, y));
    }

    public void ClearCell(int x, int y)
    {
        UnCollapse(x, y);
    }

    bool OnCanvas(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public void Reset()
    {
        pStack.Clear();

        int[] seed = tiles.Select(t => t.id).ToArray();

        for (int y = 0; y < height; y++)
        {
            collapsed[y].SetAll(false);

            HashSet<int>[] cellRow = cells[y];
            for (int x = 0; x < width; x++)
            {
                HashSet<int> cell = cellRow[x];
                cell.Clear();

                foreach (int id in seed)
                {
                    cell.Add(id);
                }
            }
        }
    }

    public enum StepResult
    {
        OneStep,
        Complete,
        NoSolution,
    }

    public StepResult Step(out Point point)
    {
        point = GetLowestEntropy();
        if (point == new Point(-1, -1))
        {
            return StepResult.Complete;
        }

        if (Collapse(point))
        {
            Propagate(point);
            return StepResult.OneStep;
        }
        return StepResult.NoSolution;
    }

    Point GetLowestEntropy()
    {
        candidateList.Clear();
        int lowestEntropy = int.MaxValue;

        for (int y = 0; y < height; y++)
        {
            var cellRow = cells[y];
            var collapsedRow = collapsed[y];

            for (int x = 0; x < collapsedRow.Length; x++)
            {
                if (collapsedRow.Get(x))
                    continue;

                int count = cellRow[x].Count;
                if (count < lowestEntropy)
                {
                    candidateList.Clear();
                    lowestEntropy = count;
                }

                if (count == lowestEntropy)
                    candidateList.Add(new Point(x, y));
            }
        }

        if (candidateList.Count == 0)
            return new Point(-1, -1);

        if (candidateList.Count == 1)
            return candidateList[0];

        return candidateSelection switch
        {
            CandidateSelection.Random => candidateList[Random.Range(0, candidateList.Count)],
            CandidateSelection.Ordered => candidateList[0],
            CandidateSelection.FurthestFromCenter => FurthestFromCenter(candidateList),
            _ => new Point(-1, -1),
        };
    }

    Point FurthestFromCenter(List<Point> points)
    {
        furthestList.Clear();
        int furthestDist = 0;

        Point center = new(width / 2, height / 2);
        foreach (Point p in points)
        {
            Point diff = p - center;
            int sm = diff.x * diff.x + diff.y * diff.y;

            if (sm > furthestDist)
            {
                furthestList.Clear();
                furthestDist = sm;
            }

            if (furthestDist == sm)
                furthestList.Add(p);
        }

        return furthestList[Random.Range(0, furthestList.Count)];
    }

    float[] min = new float[64];
    float[] max = new float[64];

    int Weighted(HashSet<int> tiles)
    {
        if (min.Length < tiles.Count)
            min = new float[(tiles.Count + 1023) / 1024 * 1024];

        if (max.Length < tiles.Count)
            max = new float[(tiles.Count + 1023) / 1024 * 1024];

        float sum = 0;
        int i = 0;
        foreach (int tile in tiles)
        {
            min[i] = sum;
            sum += this.tiles[tile].weight;
            max[i] = sum;
            i++;
        }

        float randomNum = Random.Range(0, sum);

        i = 0;
        foreach (int tile in tiles)
        {
            if (min[i] <= randomNum && max[i] >= randomNum)
            {
                return tile;
            }
            i++;
        }

        // returns normal random if weights didn't work
        Debug.Log("WFC - Weighted random didn't work. Returning normal random.");
        return tiles.ElementAt(Random.Range(0, tiles.Count));
    }

    bool Collapse(Point p) => Collapse(p.x, p.y);

    bool Collapse(int x, int y)
    {
        //Debug.Log("Collapse(" + p.x + ", " + p.y + ")");
        HashSet<int> superPositions = cells[y][x];
        if (superPositions.Count == 0)
        {
            return false;
        }

        int tile = tileSelection switch
        {
            TileSelection.Random => superPositions.ElementAt(Random.Range(0, superPositions.Count)),
            TileSelection.Ordered => superPositions.First(),
            TileSelection.Weighted => Weighted(superPositions),
            _ => 0,
        };

        superPositions.Clear();
        superPositions.Add(tile);
        collapsed[y][x] = true;
        return true;
    }

    void UnCollapse(int x, int y)
    {
        cells[y][x] = tiles.Select(t => t.id).ToHashSet();
        collapsed[y][x] = false;

        foreach (Point dir in ValidDirs(x, y))
        {
            Point p = new Point(x, y) + dir;

            if (collapsed[p.y][p.x])
                pStack.Push(p);
        }
    }

    void Propagate(Point p)
    {
        pStack.Push(p);

        while (pStack.TryPop(out p))
        {
            foreach (Point dir in ValidDirs(p))
            {
                Point pNeighbor = p + dir;

                var nPossibilities = GetCell(pNeighbor);
                int previousCount = nPossibilities.Count;

                IntersectNeighbors(p, dir, nPossibilities);

                if (nPossibilities.Count != previousCount)
                {
                    pStack.Push(pNeighbor);
                }
            }
        }
    }

    ValidDirEnumerator ValidDirs(Point p) => ValidDirs(p.x, p.y);

    ValidDirEnumerator ValidDirs(int x, int y)
    {
        return new ValidDirEnumerator(x, y, width, height);
    }

    internal HashSet<int>[] GetCellRow(int y)
    {
        return cells[y];
    }

    internal BitArray GetCollapsedRow(int y)
    {
        return collapsed[y];
    }

    /// <summary>
    /// List possible outcomes for cell at point <paramref name="p"/>
    /// </summary>
    internal HashSet<int> GetCell(Point p)
    {
        return cells[p.y][p.x];
    }

    /// <summary>
    /// List of valid neighbors in <paramref name="dir"/> for cell at point <paramref name="p"/>
    /// </summary>
    void IntersectNeighbors(Point p, Point dir, HashSet<int> neighborPossibilities)
    {
        neighborConnectors.Clear();
        foundConnectors.Clear();

        foreach (int tileId in GetCell(p))
        {
            WfcTileData tile = tiles[tileId];
            List<ConnectorDef>? connectors = (dir.x, dir.y) switch
            {
                (1, 0) => tile.right,
                (-1, 0) => tile.left,
                (0, 1) => tile.top,
                (0, -1) => tile.bottom,
                _ => null,
            };
            if (connectors == null)
            {
                continue;
            }

            foreach (ConnectorDef connector in connectors)
            {
                neighborConnectors.Add(connector.TileId);
            }
        }

        foreach (int tileId in neighborPossibilities)
        {
            WfcTileData tile = tiles[tileId];
            List<ConnectorDef>? connectors = (dir.x, dir.y) switch
            {
                (1, 0) => tile.left,
                (-1, 0) => tile.right,
                (0, 1) => tile.bottom,
                (0, -1) => tile.top,
                _ => null
            };
            if (connectors == null)
            {
                continue;
            }

            foreach (ConnectorDef connector in connectors)
            {
                if (neighborConnectors.Contains(connector.TileId))
                {
                    foundConnectors.Add(tileId);
                }
            }
        }

        neighborPossibilities.IntersectWith(foundConnectors);
    }

    public bool IsCollapsed(int x, int y) => collapsed[y].Get(x);
}
