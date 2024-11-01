using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class WFC : MonoBehaviour
{
    [SerializeField] WFCTile[] tiles;
    [SerializeField] int width = 16;
    [SerializeField] int height = 16;
    [SerializeField] CandidateSelection candidateSelection;
    [SerializeField] TileSelection tileSelection;
    [SerializeField] float timePerStep = 0.01f;

    List<WFCTile>[,] cells;
    bool[,] collapsed;

    float timer;
    bool play;

    /// <summary>
    /// Parameters: x, y, id
    /// </summary>
    public Action<int, int, int> OnCellChanged;
    public int Width => width;
    public int Height => height;
    public WFCTile[] Tiles { get { return tiles; } set { tiles = value; } }

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
                        cells[x, y] = this.cells[x, y][0];
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

    [MakeButton("Clear grid")]
    void Clear()
    {
        cells = new List<WFCTile>[width, height];
        collapsed = new bool[width, height];
        pStack = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = tiles.ToList();
                OnCellChanged?.Invoke(x, y, -1);
            }
        }
    }

    void NullCheck()
    {
        if (cells == null || collapsed == null)
            Clear();
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
        List<Point> candidates = new List<Point>();
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
                candidate = FurthestFromCenter(candidates.ToArray());
                break;

        }

        return candidate;
    }

    Point FurthestFromCenter(Point[] points)
    {
        List<Point> furthest = new();
        float furthestDist = 0;

        foreach (var p in points)
        {
            float sm = (p - new Point((int)(width / 2f), (int)(height / 2f))).SquareMagnitude;
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

    WFCTile Weighted(WFCTile[] tiles)
    {
        float[] min = new float[tiles.Length];
        float[] max = new float[tiles.Length];

        float sum = 0;
        for (int i = 0; i < tiles.Length; i++)
        {
            min[i] = sum;
            sum += tiles[i].weight;
            max[i] = sum;
        }

        float randomNum = Random.Range(0, sum);

        for (int i = 0; i < tiles.Length; i++)
        {
            if (min[i] <= randomNum && max[i] >= randomNum)
                return tiles[i];
        }

        // returns normal random if weights didn't work
        Debug.Log("WFC - Weighted random didn't work. Returning normal random.");
        return tiles[Random.Range(0, tiles.Length)];
    }

    void Collapse(Point p) => Collapse(p.x, p.y);

    void Collapse(int x, int y)
    {
        //Debug.Log("Collapse(" + p.x + ", " + p.y + ")");
        List<WFCTile> superPositions = cells[x, y];

        if (superPositions.Count == 0)
        {
            Debug.Log("WFC - Tried to collapse but had no solutions.");
            play = false;
            return;
        }

        WFCTile tile = new();
        switch (tileSelection)
        {
            case TileSelection.Random:
                tile = superPositions[Random.Range(0, superPositions.Count)];
                break;
            case TileSelection.Ordered:
                tile = superPositions[0];
                break;
            case TileSelection.Weighted:
                tile = Weighted(superPositions.ToArray());
                break;
        }

        superPositions.Clear();
        superPositions.Add(tile);
        collapsed[x, y] = true;
        OnCellChanged?.Invoke(x, y, tile.id);
    }

    void UnCollapse(int x, int y)
    {
        cells[x, y] = tiles.ToList();
        collapsed[x, y] = false;

        foreach (var dir in ValidDirs(x, y))
        {
            Point p = new Point(x, y) + dir;

            if (collapsed[p.x, p.y])
                pStack.Push(p);
        }
    }

    Stack<Point> pStack;

    void Propagate(Point p)
    {
        pStack.Push(p);

        while (pStack.Count > 0)
        {
            p = pStack.Pop();

            foreach (var dir in ValidDirs(p))
            {
                Point pNeighbor = p + dir;
                WFCTile[] nPossibilities = Possibilities(pNeighbor);

                WFCTile[] possibleNeighbours = PossibleNeighbors(p, dir);

                foreach (var nPossibility in nPossibilities)
                {
                    if (!possibleNeighbours.Contains(nPossibility))
                    {
                        cells[pNeighbor.x, pNeighbor.y].Remove(nPossibility);

                        if (!pStack.Contains(pNeighbor))
                            pStack.Push(pNeighbor);
                    }
                }
            }
        }
    }

    Point[] ValidDirs(Point p) => ValidDirs(p.x, p.y);
    Point[] ValidDirs(int x, int y)
    {
        List<Point> list = new();

        if (x > 0)
            list.Add(new Point(-1, 0));
        if (y > 0)
            list.Add(new Point(0, -1));
        if (x < width - 1)
            list.Add(new Point(1, 0));
        if (y < height - 1)
            list.Add(new Point(0, 1));

        return list.ToArray();
    }

    /// <summary>
    /// List possible outcomes for cell at point <paramref name="p"/>
    /// </summary>
    /// <param name="p"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    WFCTile[] Possibilities(Point p)
    {
        return cells[p.x, p.y].ToArray();
    }

    /// <summary>
    /// List of valid neighbors in <paramref name="dir"/> for cell at point <paramref name="p"/>
    /// </summary>
    /// <param name="p"></param>
    /// <param name="dir"></param>
    /// <returns></returns>
    WFCTile[] PossibleNeighbors(Point p, Point dir)
    {
        List<int> connectors = new();

        foreach (var possibility in Possibilities(p))
        {
            int connector = -1;

            if (dir == new Point(1, 0))
                connector = possibility.right;
            if (dir == new Point(-1, 0))
                connector = possibility.left;
            if (dir == new Point(0, 1))
                connector = possibility.top;
            if (dir == new Point(0, -1))
                connector = possibility.bottom;

            if (!connectors.Contains(connector))
                connectors.Add(connector);
        }

        List<WFCTile> tileList = new();
        foreach (var tile in tiles)
        {
            int connector = -1;

            if (dir == new Point(1, 0))
                connector = tile.left;
            if (dir == new Point(-1, 0))
                connector = tile.right;
            if (dir == new Point(0, 1))
                connector = tile.bottom;
            if (dir == new Point(0, -1))
                connector = tile.top;

            if (connectors.Contains(connector))
                if (!tileList.Contains(tile))
                    tileList.Add(tile);
        }

        return tileList.ToArray();
    }

    bool IsCollapsed(Point p) => IsCollapsed(p.x, p.y);
    bool IsCollapsed(int x, int y) => collapsed[x, y];
}
