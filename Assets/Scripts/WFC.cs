using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;
using Unity.Collections;

public class WFC : MonoBehaviour
{
    [SerializeField] int width = 16;
    [SerializeField] int height = 16;
    [SerializeField] CandidateSelection candidateSelection;
    [SerializeField] TileSelection tileSelection;
    [SerializeField] float timePerStep = 0.01f;

    [SerializeField] WFCTile[] tiles;

    HashSet<WFCTile>[,] cells;
    bool[,] collapsed;

    float timer;
    bool play;

    /// <summary>
    /// Parameters: x, y, id
    /// </summary>
    public Action<int, int, int> OnCellChanged;
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

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (IsCollapsed(x, y))
                        cells[x, y] = this.cells[x, y].First();
                }
            }

            return cells;
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

    public void SetCell(int x, int y, WFCTile tile)
    {
        NullCheck();

        if (!OnCanvas(x, y))
            Debug.LogError("(" + x + ", " + y + ") is not a valid cell. It's outside of the canvas.");

        if (IsCollapsed(x, y))
            UnCollapse(x, y);

        cells[x, y].Clear();
        cells[x, y].Add(tile);
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

    [MakeButton]
    void AlignCamera()
    {
        Camera camera = Camera.main;
        camera.transform.position = new Vector3(width / 2, height / 2, -1);
        camera.orthographicSize = Math.Max(width, height) / 2;
    }

    [MakeButton]
    void ClearGrid()
    {
        cells = new HashSet<WFCTile>[width, height];
        collapsed = new bool[width, height];

        pStack.Clear();
        possibleNeighbours.Clear();

        HashSet<WFCTile> seed = tiles.ToHashSet();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new HashSet<WFCTile>(seed);
                OnCellChanged?.Invoke(x, y, -1);
            }
        }
    }

    void NullCheck()
    {
        if (cells == null || collapsed == null)
            ClearGrid();
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
    private void Pause()
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

            Step();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            play = !play;

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

        NullCheck();

        Point coords = GetLowestEntropy();

        if (coords == new Point(-1, -1))
        {
            play = false;
            Debug.Log("WFC - Done");
            return;
        }

        Collapse(coords);
        Propagate(coords);
    }

    Point GetLowestEntropy()
    {
        List<Point> candidates = new();
        int lowestEntropy = int.MaxValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);
                if (IsCollapsed(p))
                    continue;

                if (cells[x, y].Count < lowestEntropy)
                {
                    candidates.Clear();
                    lowestEntropy = cells[x, y].Count;
                }

                if (cells[x, y].Count == lowestEntropy)
                    candidates.Add(p);
            }
        }

        if (candidates.Count == 0)
            return new Point(-1, -1);

        Point candidate = new();

        switch (candidateSelection)
        {
            case CandidateSelection.Random:
                candidate = candidates[Random.Range(0, candidates.Count)];
                break;

            case CandidateSelection.Ordered:
                candidate = candidates[0];
                break;

            case CandidateSelection.FurthestFromCenter:
                candidate = FurthestFromCenter(candidates);
                break;

        }

        return candidate;
    }

    Point FurthestFromCenter(List<Point> points)
    {
        List<Point> furthest = new();
        int furthestDist = 0;

        foreach (Point p in points)
        {
            Point diff = p - new Point(width / 2, height / 2);
            int sm = diff.x * diff.x + diff.y * diff.y;
            if (sm > furthestDist)
            {
                furthest.Clear();
                furthestDist = sm;
            }
            if (furthestDist == sm)
                furthest.Add(p);
        }

        return furthest[Random.Range(0, furthest.Count)];
    }

    WFCTile Weighted(HashSet<WFCTile> tiles)
    {
        float[] min = new float[tiles.Count];
        float[] max = new float[tiles.Count];

        float sum = 0;
        int i = 0;
        foreach (WFCTile tile in tiles)
        {
            min[i] = sum;
            sum += tile.weight;
            max[i] = sum;
            i++;
        }

        float randomNum = Random.Range(0, sum);

        i = 0;
        foreach (WFCTile tile in tiles)
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

    void Collapse(Point p) => Collapse(p.x, p.y);

    void Collapse(int x, int y)
    {
        //Debug.Log("Collapse(" + p.x + ", " + p.y + ")");
        HashSet<WFCTile> superPositions = cells[x, y];

        if (superPositions.Count == 0)
        {
            Debug.Log("WFC - Tried to collapse but had no solutions.");
            play = false;
            return;
        }

        WFCTile tile = tileSelection switch
        {
            TileSelection.Random => superPositions.ElementAt(Random.Range(0, superPositions.Count)),
            TileSelection.Ordered => superPositions.First(),
            TileSelection.Weighted => Weighted(superPositions),
            _ => throw new Exception("Invalid tile selection."),
        };

        superPositions.Clear();
        superPositions.Add(tile);
        collapsed[x, y] = true;
        OnCellChanged?.Invoke(x, y, tile.id);
    }

    void UnCollapse(int x, int y)
    {
        cells[x, y] = tiles.ToHashSet();
        collapsed[x, y] = false;

        foreach (Point dir in ValidDirs(x, y))
        {
            Point p = new Point(x, y) + dir;

            if (collapsed[p.x, p.y])
                pStack.Push(p);
        }
    }

    Stack<Point> pStack = new();
    HashSet<WFCTile> possibleNeighbours = new();

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

                PossibleNeighbors(p, dir, possibleNeighbours);
                nPossibilities.IntersectWith(possibleNeighbours);
                possibleNeighbours.Clear();

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

    struct ValidDirEnumerator : IEnumerable<Point>, IEnumerator<Point>
    {
        private int _state;
        private readonly int _x;
        private readonly int _y;
        private readonly int _w;
        private readonly int _h;

        public ValidDirEnumerator(int x, int y, int width, int height)
        {
            _x = x;
            _y = y;
            _w = width;
            _h = height;
            _state = 0;
        }

        public readonly Point Current => _state switch
        {
            1 => new Point(-1, 0),
            2 => new Point(0, -1),
            3 => new Point(1, 0),
            4 => new Point(0, 1),
            _ => new Point(),
        };

        readonly object IEnumerator.Current => Current;

        public readonly void Dispose()
        {
        }

        public readonly ValidDirEnumerator GetEnumerator() => this;

        public bool MoveNext()
        {
            while (_state < 4)
            {
                _state++;
                switch (_state)
                {
                    case 1 when _x > 0:
                    case 2 when _y > 0:
                    case 3 when _x < _w - 1:
                    case 4 when _y < _h - 1:
                        return true;
                }
            }
            return false;
        }

        public readonly void Reset()
        {
        }

        readonly IEnumerator<Point> IEnumerable<Point>.GetEnumerator() => GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    /// <summary>
    /// List possible outcomes for cell at point <paramref name="p"/>
    /// </summary>
    HashSet<WFCTile> GetCell(Point p)
    {
        return cells[p.x, p.y];
    }

    /// <summary>
    /// List of valid neighbors in <paramref name="dir"/> for cell at point <paramref name="p"/>
    /// </summary>
    void PossibleNeighbors(Point p, Point dir, HashSet<WFCTile> foundTiles)
    {
        HashSet<int> connectors = new();

        foreach (WFCTile possibility in GetCell(p))
        {
            int connector = (dir.x, dir.y) switch
            {
                (1, 0) => possibility.right[0].TileId,
                (-1, 0) => possibility.left[0].TileId,
                (0, 1) => possibility.top[0].TileId,
                (0, -1) => possibility.bottom[0].TileId,
                _ => -1,
            };
            connectors.Add(connector);
        }

        foreach (WFCTile tile in tiles)
        {
            int connector = (dir.x, dir.y) switch
            {
                (1, 0) => tile.left[0].TileId,
                (-1, 0) => tile.right[0].TileId,
                (0, 1) => tile.bottom[0].TileId,
                (0, -1) => tile.top[0].TileId,
                _ => -1
            };
            if (connectors.Contains(connector))
            {
                foundTiles.Add(tile);
            }
        }
    }

    bool IsCollapsed(Point p) => IsCollapsed(p.x, p.y);
    bool IsCollapsed(int x, int y) => collapsed[x, y];
}
