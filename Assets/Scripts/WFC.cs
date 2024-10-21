using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class WFC : MonoBehaviour
{
    [SerializeField]
    int width;
    [SerializeField]
    int height;
    [SerializeField]
    WFCTile[] tiles;
    [SerializeField]
    Material defaultMaterial;
    [SerializeField]
    float timePerStep;
    [SerializeField]
    int placeTile;
    [SerializeField]
    CandidateSelection candidateSelection;
    [SerializeField]
    TileSelection tileSelection;

    GameObject[,] gameObjects;
    List<WFCTile>[,] cells;
    bool[,] collapsed;

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

    [MakeButton("Clear grid")]
    void Clear()
    {
        DestroyCells();
        CreateCells();
        Display();
    }

    void DestroyCells()
    {
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    void CreateCells()
    {
        gameObjects = new GameObject[width, height];
        cells = new List<WFCTile>[width, height];
        collapsed = new bool[width, height];
        pStack = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
                go.transform.position = WorldPosition(new Point(x, y));
                go.transform.SetParent(transform);

                if (go.TryGetComponent(out Renderer r))
                    r.material = defaultMaterial;

                gameObjects[x, y] = go;
                cells[x, y] = tiles.ToList();
            }
        }
    }

    Vector3 WorldPosition(Point cell)
    {
        return new Vector3(cell.x - (width - 1f) / 2f, cell.y - (height - 1f) / 2f, 0);
    }

    Point Cell(Vector3 worldPosition)
    {
        return new Point((int)(worldPosition.x + (width - 1f) / 2f), (int)(worldPosition.y + (height - 1f) / 2f));
    }

    void Display()
    {
        NullCheck();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Point p = new Point(x, y);

                if (!gameObjects[x, y].TryGetComponent(out Renderer r))
                    continue;

                if (IsCollapsed(p))
                {
                    WFCTile tile = Tile(p);

                    if (tile == null)
                        continue;

                    r.material = tile.material;
                    continue;
                }

                r.material = defaultMaterial;
            }
        }
    }

    void NullCheck()
    {
        if (cells == null || gameObjects == null || collapsed == null)
            Clear();
    }

    [MakeButton("Get rules from grid")]
    void GetRules()
    {
        if (!EditorApplication.isPlaying)
            return;

        NullCheck();
        ResetRules();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (!collapsed[x, y])
                    continue;

                SetRules(new Point(x, y));
            }
        }

    }

    void ResetRules()
    {
        foreach (var tile in tiles)
        {
            tile.left = new();
            tile.right = new();
            tile.top = new();
            tile.bottom = new();
        }
    }

    void SetRules(Point p)
    {
        foreach (var dir in ValidDirs(p))
        {
            Point nP = p + dir;
            if (!collapsed[nP.x, nP.y])
                continue;

            List<Material> neighborList = new();
            if (dir == new Point(1, 0))
                neighborList = cells[p.x, p.y][0].right;
            if (dir == new Point(-1, 0))
                neighborList = cells[p.x, p.y][0].left;
            if (dir == new Point(0, 1))
                neighborList = cells[p.x, p.y][0].top;
            if (dir == new Point(0, -1))
                neighborList = cells[p.x, p.y][0].bottom;

            if (!neighborList.Contains(cells[nP.x, nP.y][0].material))
                neighborList.Add(cells[nP.x, nP.y][0].material);
        }
    }

    [MakeButton]
    void Play()
    {
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

        while (play && timer >= timePerStep)
        {
            timer -= timePerStep;

            Step();
        }
    }

    void TryPlaceTile()
    {
        NullCheck();

        if (Input.mouseScrollDelta.y < 0)
            placeTile--;
        if (Input.mouseScrollDelta.y > 0)
            placeTile++;

        if (placeTile > tiles.Length - 1)
            placeTile = 0;
        if (placeTile < 0)
            placeTile = tiles.Length - 1;

        bool mouse0 = Input.GetKey(KeyCode.Mouse0);
        bool mouse1 = Input.GetKey(KeyCode.Mouse1);

        if (!mouse0 && !mouse1)
            return;

        Point cell = MouseToCell();
        if (!IsValid(cell))
            return;

        if (mouse0)
        {
            if (IsCollapsed(cell))
                Uncollapse(cell);

            cells[cell.x, cell.y].Clear();
            cells[cell.x, cell.y].Add(tiles[placeTile]);
            Collapse(cell);
            pStack.Push(cell);
        }
        if (mouse1 && IsCollapsed(cell))
        {
            Uncollapse(cell);
        }

        Display();
    }

    void Uncollapse(Point cell)
    {
        cells[cell.x, cell.y] = tiles.ToList();
        collapsed[cell.x, cell.y] = false;

        foreach (var dir in ValidDirs(cell))
        {
            Point p = cell + dir;
            if (collapsed[p.x, p.y])
                pStack.Push(p);
        }
    }

    Point MouseToCell()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out hit, 100.0f))
            return new Point(int.MaxValue, int.MaxValue);

        return Cell(hit.transform.position);
    }

    bool IsValid(Point p)
    {
        return p.x < width && p.x > 0 && p.y < height && p.y > 0;
    }

    float timer;
    bool play;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            play = !play;

        TryPlay();
        TryPlaceTile();
    }

    [MakeButton]
    void Step()
    {
        NullCheck();

        Point coords = GetLowestEntropy();

        if (coords == new Point(-1, -1))
        {
            play = false;
            Debug.Log("Done");
            return;
        }

        Collapse(coords);
        Propagate(coords);
        Display();
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
        Debug.Log("Weighed random didn't work. Returning normal random.");
        return tiles[Random.Range(0, tiles.Length)];
    }

    void Collapse(Point p)
    {
        //Debug.Log("Collapse(" + p.x + ", " + p.y + ")");
        List<WFCTile> superPositions = cells[p.x, p.y];

        if (superPositions.Count == 0)
        {
            Debug.Log("Tried to collapse but had no solutions.");
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
        collapsed[p.x, p.y] = true;
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

    Point[] ValidDirs(Point p)
    {
        List<Point> list = new();

        if (p.x > 0)
            list.Add(new Point(-1, 0));
        if (p.y > 0)
            list.Add(new Point(0, -1));
        if (p.x < width - 1)
            list.Add(new Point(1, 0));
        if (p.y < height - 1)
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
        List<Material> list = new();

        foreach (var possibility in Possibilities(p))
        {
            List<Material> tiles = new();

            if (dir == new Point(1, 0))
                tiles = possibility.right;
            if (dir == new Point(-1, 0))
                tiles = possibility.left;
            if (dir == new Point(0, 1))
                tiles = possibility.top;
            if (dir == new Point(0, -1))
                tiles = possibility.bottom;

            foreach (var tile in tiles)
                if (!list.Contains(tile))
                    list.Add(tile);
        }

        List<WFCTile> tileList = new();
        foreach (var tile in tiles)
            if (list.Contains(tile.material))
                if (!tileList.Contains(tile))
                    tileList.Add(tile);

        return tileList.ToArray();
    }

    bool IsCollapsed(Point p) => collapsed[p.x, p.y];
    WFCTile Tile(Point p)
    {
        if (cells == null)
            return null;

        if (cells[p.x, p.y] == null)
            return null;

        if (!IsCollapsed(p))
            return null;

        if (cells[p.x, p.y].Count < 1)
            return null;

        if (cells[p.x, p.y][0] == null)
            return null;

        return cells[p.x, p.y][0];
    }
}
