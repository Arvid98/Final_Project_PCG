using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StartMap : MonoBehaviour
{
    [Header("WFCTiles")]
    public WFCTile playerOneTile;
    public WFCTile playerTwoTile;
    public WFCTile baseTile;
    public WFCTile goldTile;
    public WFCTile stoneTile;
    public WFCTile forestTile;

    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;

    [Header("Distance Constraints")]
    public float minDistanceBetweenStarts = 3.0f;
    public int minEdgeDistance = 2;
    public int maxEdgeDistance = 7;

    [Header("Gold Tile Settings")]
    public float goldOffsetDistance = 3.0f;
    public float extraGoldDistanceFromPlayer = 4.0f;
    public int numAdjacentGoldTiles = 3;

    [Header("Stone Tile Settings")]
    public float stoneDistanceFromPlayer = 4.0f;
    public int numAdjacentStoneTiles = 3;

    [Header("Forest Tile Settings")]
    public float forestDistanceFromPlayer = 7.0f;
    public int numAdjacentForestTiles = 7;
    public float maxForestSpreadDistance = 3.0f;
    public int numberOfForest = 3;

    private int[,] grid;
    private List<Vector2> usedPositions = new List<Vector2>();
    private Vector2 playerOnePosition;
    private Vector2 playerTwoPosition;
    WFC wFC;
    void Start()
    {
        wFC = FindAnyObjectByType<WFC>();
        Setup();
    }

    //public void Setup()
    //{
    //    Clear();
    //    grid = new int[gridWidth, gridHeight];
    //    SetRandomPlayerStartPositions();
    //    SetGoldTiles();
    //    SetStoneTiles();
    //    SetExtraGoldTiles();
    //    SetForestTiles();
    //    wFC.Tiles = GetTiles();
    //}
    [MakeButton]
    public void Setup()
    {
        wFC.Clear();
        Clear();
        
        SetRandomPlayerStartPositions();
        SetGoldTiles();
        SetStoneTiles();
        SetExtraGoldTiles();
        SetForestTiles();

        //wFC.CollapseWithStartMapGrid(grid, GetTiles());
        //wFC.Tiles = GetTileTyp();
        //AddTilesToWFCTiles();
        UpdateGridWithTiles();


    }
    void AddTilesToWFCTiles()
    {
        List<WFCTile> currentTiles = new List<WFCTile>(wFC.Tiles);
        List<WFCTile> tilesToAdd = GetTileTyp().ToList(); 

        foreach (var tile in tilesToAdd)
        {
            if (!currentTiles.Contains(tile))
            {
                currentTiles.Add(tile);
            }
        }

        wFC.Tiles = currentTiles.ToArray();
    }
    void UpdateGridWithTiles()
    {
        if (wFC == null)
            return;

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int tileId = grid[x, y];
                WFCTile tile = GetTileById(tileId);


                if (tile != null && tile != baseTile)
                {
                    wFC.SetCell(x, y, tile);
                }
            }
        }
    }
    public void Clear()
    {
        usedPositions.Clear();
        grid = new int[gridWidth, gridHeight];
    }
    public WFCTile[] GetTileTyp()
    {
        List<WFCTile> uniqueTiles = new List<WFCTile> { playerOneTile, playerTwoTile, baseTile, goldTile, stoneTile, forestTile };


        return uniqueTiles.ToArray();

       
    }
    public WFCTile[] GetTiles()
    {
        List<WFCTile> uniqueTiles = new List<WFCTile>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int tileId = grid[x, y];
                WFCTile tile = GetTileById(tileId);
                if (tile != null && !uniqueTiles.Contains(tile) )
                {
                    uniqueTiles.Add(tile);
                }
            }
        }

        return uniqueTiles.ToArray();
    }

    private WFCTile GetTileById(int id)
    {
        if (playerOneTile != null && playerOneTile.id == id) return playerOneTile;
        if (playerTwoTile != null && playerTwoTile.id == id) return playerTwoTile;
        if (baseTile != null && baseTile.id == id) return baseTile;
        if (goldTile != null && goldTile.id == id) return goldTile;
        if (stoneTile != null && stoneTile.id == id) return stoneTile;
        if (forestTile != null && forestTile.id == id) return forestTile;
        return null;
    }

    void SetRandomPlayerStartPositions()
    {
        playerOnePosition = GetValidRandomPosition();
        usedPositions.Add(playerOnePosition);
        grid[(int)playerOnePosition.x, (int)playerOnePosition.y] = playerOneTile.id;

        //wFC.SetCell((int)playerOnePosition.x, (int)playerOnePosition.y, playerOneTile);

        playerTwoPosition = GetValidRandomPosition();
        usedPositions.Add(playerTwoPosition);
        grid[(int)playerTwoPosition.x, (int)playerTwoPosition.y] = playerTwoTile.id;

        //wFC.SetCell((int)playerTwoPosition.x, (int)playerTwoPosition.y, playerTwoTile);
    }

    void SetGoldTiles()
    {
        Vector2 goldPositionOne = GetPositionInDirection(playerOnePosition, playerTwoPosition, goldOffsetDistance);
        usedPositions.Add(goldPositionOne);
        grid[(int)goldPositionOne.x, (int)goldPositionOne.y] = goldTile.id;
        PlaceAdjacentTiles(goldPositionOne, goldTile, numAdjacentGoldTiles);

        Vector2 goldPositionTwo = GetPositionInDirection(playerTwoPosition, playerOnePosition, goldOffsetDistance);
        usedPositions.Add(goldPositionTwo);
        grid[(int)goldPositionTwo.x, (int)goldPositionTwo.y] = goldTile.id;
        PlaceAdjacentTiles(goldPositionTwo, goldTile, numAdjacentGoldTiles);
    }

    void SetStoneTiles()
    {
        Vector2 stonePositionOne = GetPositionAtDistance(playerOnePosition, stoneDistanceFromPlayer);
        usedPositions.Add(stonePositionOne);
        grid[(int)stonePositionOne.x, (int)stonePositionOne.y] = stoneTile.id;
        PlaceAdjacentTiles(stonePositionOne, stoneTile, numAdjacentStoneTiles);

        Vector2 stonePositionTwo = GetPositionAtDistance(playerTwoPosition, stoneDistanceFromPlayer);
        usedPositions.Add(stonePositionTwo);
        grid[(int)stonePositionTwo.x, (int)stonePositionTwo.y] = stoneTile.id;
        PlaceAdjacentTiles(stonePositionTwo, stoneTile, numAdjacentStoneTiles);
    }

    void SetExtraGoldTiles()
    {
        Vector2 extraGoldPositionOne = GetPositionAtDistance(playerOnePosition, extraGoldDistanceFromPlayer);
        usedPositions.Add(extraGoldPositionOne);
        grid[(int)extraGoldPositionOne.x, (int)extraGoldPositionOne.y] = goldTile.id;
        PlaceAdjacentTiles(extraGoldPositionOne, goldTile, numAdjacentGoldTiles);

        Vector2 extraGoldPositionTwo = GetPositionAtDistance(playerTwoPosition, extraGoldDistanceFromPlayer);
        usedPositions.Add(extraGoldPositionTwo);
        grid[(int)extraGoldPositionTwo.x, (int)extraGoldPositionTwo.y] = goldTile.id;
        PlaceAdjacentTiles(extraGoldPositionTwo, goldTile, numAdjacentGoldTiles);
    }

    void SetForestTiles()
    {
        for (int i = 0; i < numberOfForest; i++)
        {
            PlaceForestWithSpread(playerOnePosition);
            PlaceForestWithSpread(playerTwoPosition);
        }
    }

    void PlaceForestWithSpread(Vector2 origin)
    {
        Vector2 forestCenterPosition = GetPositionAtDistance(origin, forestDistanceFromPlayer);
        usedPositions.Add(forestCenterPosition);
        grid[(int)forestCenterPosition.x, (int)forestCenterPosition.y] = forestTile.id;

        int placedTiles = 0;
        while (placedTiles < numAdjacentForestTiles)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-maxForestSpreadDistance, maxForestSpreadDistance),
                Random.Range(-maxForestSpreadDistance, maxForestSpreadDistance)
            );

            Vector2 forestPosition = forestCenterPosition + randomOffset;
            forestPosition = new Vector2(Mathf.Round(forestPosition.x), Mathf.Round(forestPosition.y));
            forestPosition.x = Mathf.Clamp(forestPosition.x, 0, gridWidth - 1);
            forestPosition.y = Mathf.Clamp(forestPosition.y, 0, gridHeight - 1);

            if (!usedPositions.Contains(forestPosition))
            {
                usedPositions.Add(forestPosition);
                grid[(int)forestPosition.x, (int)forestPosition.y] = forestTile.id;
                placedTiles++;
            }
        }
    }

    void PlaceAdjacentTiles(Vector2 centerPosition, WFCTile tile, int numTiles)
    {
        int placedTiles = 0;
        float spreadDistance = 1.5f;

        while (placedTiles < numTiles)
        {
            Vector2 randomOffset = new Vector2(
                Random.Range(-spreadDistance, spreadDistance),
                Random.Range(-spreadDistance, spreadDistance)
            );

            Vector2 tilePosition = centerPosition + randomOffset;
            tilePosition = new Vector2(Mathf.Round(tilePosition.x), Mathf.Round(tilePosition.y));
            tilePosition.x = Mathf.Clamp(tilePosition.x, 0, gridWidth - 1);
            tilePosition.y = Mathf.Clamp(tilePosition.y, 0, gridHeight - 1);

            if (!usedPositions.Contains(tilePosition))
            {
                usedPositions.Add(tilePosition);
                grid[(int)tilePosition.x, (int)tilePosition.y] = tile.id;
                placedTiles++;
            }
        }
    }

    Vector2 GetValidRandomPosition()
    {
        Vector2 randomPosition;
        bool positionIsValid;

        do
        {
            int x = Random.Range(minEdgeDistance, gridWidth - minEdgeDistance);
            int y = Random.Range(minEdgeDistance, gridHeight - minEdgeDistance);
            randomPosition = new Vector2(x, y);

            bool withinMaxEdgeDistance =
                x <= maxEdgeDistance || y <= maxEdgeDistance ||
                x >= gridWidth - maxEdgeDistance || y >= gridHeight - maxEdgeDistance;

            positionIsValid = withinMaxEdgeDistance;
            if (positionIsValid)
            {
                foreach (var usedPosition in usedPositions)
                {
                    if (Vector2.Distance(randomPosition, usedPosition) < minDistanceBetweenStarts)
                    {
                        positionIsValid = false;
                        break;
                    }
                }
            }

        } while (!positionIsValid);

        return randomPosition;
    }

    Vector2 GetPositionInDirection(Vector2 start, Vector2 target, float distance)
    {
        Vector2 direction = (target - start).normalized;
        Vector2 newPosition = start + direction * distance;
        newPosition = new Vector2(Mathf.Round(newPosition.x), Mathf.Round(newPosition.y));
        newPosition.x = Mathf.Clamp(newPosition.x, 0, gridWidth - 1);
        newPosition.y = Mathf.Clamp(newPosition.y, 0, gridHeight - 1);
        return newPosition;
    }

    Vector2 GetPositionAtDistance(Vector2 origin, float distance)
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                                 Vector2.up + Vector2.right, Vector2.up + Vector2.left,
                                 Vector2.down + Vector2.right, Vector2.down + Vector2.left };

        ShuffleArray(directions);

        foreach (var dir in directions)
        {
            Vector2 position = origin + dir.normalized * distance;
            position = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
            position.x = Mathf.Clamp(position.x, 0, gridWidth - 1);
            position.y = Mathf.Clamp(position.y, 0, gridHeight - 1);

            if (!usedPositions.Contains(position))
            {
                return position;
            }
        }

        return origin + Vector2.up * distance;
    }

    void ShuffleArray(Vector2[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2 temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    public int[,] GetGrid()
    {
        return grid;
    }
}
