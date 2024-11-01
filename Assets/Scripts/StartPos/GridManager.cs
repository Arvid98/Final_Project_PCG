using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject tilePrefab;
    public GameObject playerStartPositionPrefab;
    public GameObject playerTwoStartPositionPrefab;
    public GameObject goldTilePrefab;
    public GameObject stoneTilePrefab;
    public GameObject forestTilePrefab;

    [Header("Grid Settings")]
    public int gridWidth = 10;
    public int gridHeight = 10;
    public float tileSize = 1.0f;

    [Header("Distance Constraints")]
    public float minDistanceBetweenStarts = 3.0f;
    public int minEdgeDistance = 2;
    public int maxEdgeDistance = 7;

    [Header("Gold Tile Settings")]
    public float goldOffsetDistance = 3.0f;
    public float extraGoldDistanceFromPlayer = 4.0f;

    [Header("Stone Tile Settings")]
    public float stoneDistanceFromPlayer = 4.0f;

    [Header("Forest Tile Settings")]
    public float forestDistanceFromPlayer = 7.0f;
    public int numAdjacentForestTiles = 7;
    public float maxForestSpreadDistance = 3.0f;

    private List<Vector2> usedPositions = new List<Vector2>();
    private Vector2 playerOnePosition;
    private Vector2 playerTwoPosition;

    void Start()
    {
        //Setup();
    }

    [MakeButton]
    public void Setup()
    {
        Clear();
        SetRandomPlayerStartPositions();
        SetGoldTiles();
        SetStoneTiles();
        SetExtraGoldTiles();
        SetForestTiles();
        GenerateGrid();
    }

 
    public void Clear()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        usedPositions.Clear();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2 position = new Vector2(x, y);
                if (usedPositions.Contains(position)) continue;

                Vector3 tilePosition = new Vector3(x * tileSize, y * tileSize, 0);
                Instantiate(tilePrefab, tilePosition, Quaternion.identity, transform);
            }
        }
    }

    void SetRandomPlayerStartPositions()
    {
        playerOnePosition = GetValidRandomPosition();
        usedPositions.Add(playerOnePosition);
        Instantiate(playerStartPositionPrefab, new Vector3(playerOnePosition.x * tileSize, playerOnePosition.y * tileSize, 0), Quaternion.identity, transform);

        playerTwoPosition = GetValidRandomPosition();
        usedPositions.Add(playerTwoPosition);
        Instantiate(playerTwoStartPositionPrefab, new Vector3(playerTwoPosition.x * tileSize, playerTwoPosition.y * tileSize, 0), Quaternion.identity, transform);
    }

    void SetGoldTiles()
    {
        Vector2 goldPositionOne = GetPositionInDirection(playerOnePosition, playerTwoPosition, goldOffsetDistance);
        usedPositions.Add(goldPositionOne);
        Instantiate(goldTilePrefab, new Vector3(goldPositionOne.x * tileSize, goldPositionOne.y * tileSize, 0), Quaternion.identity, transform);

        Vector2 goldPositionTwo = GetPositionInDirection(playerTwoPosition, playerOnePosition, goldOffsetDistance);
        usedPositions.Add(goldPositionTwo);
        Instantiate(goldTilePrefab, new Vector3(goldPositionTwo.x * tileSize, goldPositionTwo.y * tileSize, 0), Quaternion.identity, transform);
    }

    void SetStoneTiles()
    {
        Vector2 stonePositionOne = GetPositionAtDistance(playerOnePosition, stoneDistanceFromPlayer);
        usedPositions.Add(stonePositionOne);
        Instantiate(stoneTilePrefab, new Vector3(stonePositionOne.x * tileSize, stonePositionOne.y * tileSize, 0), Quaternion.identity, transform);

        Vector2 stonePositionTwo = GetPositionAtDistance(playerTwoPosition, stoneDistanceFromPlayer);
        usedPositions.Add(stonePositionTwo);
        Instantiate(stoneTilePrefab, new Vector3(stonePositionTwo.x * tileSize, stonePositionTwo.y * tileSize, 0), Quaternion.identity, transform);
    }

    void SetExtraGoldTiles()
    {
        Vector2 extraGoldPositionOne = GetPositionAtDistance(playerOnePosition, extraGoldDistanceFromPlayer);
        usedPositions.Add(extraGoldPositionOne);
        Instantiate(goldTilePrefab, new Vector3(extraGoldPositionOne.x * tileSize, extraGoldPositionOne.y * tileSize, 0), Quaternion.identity, transform);

        Vector2 extraGoldPositionTwo = GetPositionAtDistance(playerTwoPosition, extraGoldDistanceFromPlayer);
        usedPositions.Add(extraGoldPositionTwo);
        Instantiate(goldTilePrefab, new Vector3(extraGoldPositionTwo.x * tileSize, extraGoldPositionTwo.y * tileSize, 0), Quaternion.identity, transform);
    }

    void SetForestTiles()
    {
        for (int i = 0; i < 3; i++)
        {
            PlaceForestWithSpread(playerOnePosition);
            PlaceForestWithSpread(playerTwoPosition);
        }
    }

    void PlaceForestWithSpread(Vector2 origin)
    {
        Vector2 forestCenterPosition = GetPositionAtDistance(origin, forestDistanceFromPlayer);
        usedPositions.Add(forestCenterPosition);
        Instantiate(forestTilePrefab, new Vector3(forestCenterPosition.x * tileSize, forestCenterPosition.y * tileSize, 0), Quaternion.identity, transform);

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
                Instantiate(forestTilePrefab, new Vector3(forestPosition.x * tileSize, forestPosition.y * tileSize, 0), Quaternion.identity, transform);
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

    Vector2 GetPositionInDirection(Vector2 origin, Vector2 target, float distance)
    {
        Vector2 direction = (target - origin).normalized;
        Vector2 newPosition = origin + direction * distance;

        newPosition = new Vector2(Mathf.Round(newPosition.x), Mathf.Round(newPosition.y));
        newPosition.x = Mathf.Clamp(newPosition.x, 0, gridWidth - 1);
        newPosition.y = Mathf.Clamp(newPosition.y, 0, gridHeight - 1);

        return newPosition;
    }

    Vector2 GetPositionAtDistance(Vector2 origin, float distance)
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 position;

        foreach (var dir in directions)
        {
            position = origin + dir * distance;
            position = new Vector2(Mathf.Round(position.x), Mathf.Round(position.y));
            position.x = Mathf.Clamp(position.x, 0, gridWidth - 1);
            position.y = Mathf.Clamp(position.y, 0, gridHeight - 1);

            if (!usedPositions.Contains(position))
            {
                return position;
            }
        }

        Vector2[] diagonalDirections = { Vector2.up + Vector2.right, Vector2.up + Vector2.left, Vector2.down + Vector2.right, Vector2.down + Vector2.left };
        foreach (var dir in diagonalDirections)
        {
            position = origin + dir.normalized * distance;
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
}
