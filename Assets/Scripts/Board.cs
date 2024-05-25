using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour
{
    public int width, height;
    public float borderSize;
    public GameObject tileNormalPrefab;
    public GameObject tileObstaclePrefab;
    public GameObject[] gamePiecePrefabs;
    public GameObject adjacentPrefab, columnBombPrefab, rowBombPrefab, colorBombPrefab;
    public int maxCollectibles = 3, collectibleCount;
    [Range(0, 1)]
    public float chanceForCollectible = 0.1f;
    public GameObject[] collectiblePrefabs;
    GameObject m_clickedTileBomb, m_targetTileBomb;

    public float swapTime = 0.5f, collapseTimePerTile = 0.1f;
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;
    Tile m_clickedTile, m_targetTile;
    public bool m_playerInputEnabled = true;
    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;

    [System.Serializable]
    public class StartingObject
    {
        public GameObject prefab;
        public int x, y, z;
    }

    ParticleManager m_particleManager;
    int m_scoreMultiplier = 0;

    // Start is called before the first frame update
    void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePieces = new GamePiece[width, height];
        
        m_particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>(); ;
    }

    public void SetupBoard(){
        SetupTiles();
        SetupGamePieces();
        collectibleCount = FindAllCollectibles().Count;
        SetupCamera();
        FillBoard(2 << 5);
        //HighlightMatches();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E pressed");
        }
    }

    void MakeTile(GameObject prefab, int i, int j, int z = 0)
    {
        GameObject tile = Instantiate(prefab, new Vector3(i, j, z), Quaternion.identity) as GameObject;
        tile.name = i + "," + j;
        m_allTiles[i, j] = tile.GetComponent<Tile>();
        tile.transform.parent = transform;
        m_allTiles[i, j].Init(i, j, this);
    }

    GamePiece MakeGamePiece(GameObject prefab, int i, int j, int falseYOffset = 0, float moveTime = 0.1f)
    {
        prefab.GetComponent<GamePiece>().Init(i, j, this);
        PlaceGamePiece(prefab.GetComponent<GamePiece>(), i, j);

        if (falseYOffset != 0)
        {
            prefab.transform.position = new Vector3(i, j + falseYOffset, 0);
            //Debug.Log(i + "," + j + " is placed at " + i + "," + (j + falseYOffset) + " offset:" + falseYOffset);
            // prefab.GetComponent<GamePiece>().Move(i, j, collapseTimePerTile * falseYOffset / 2);
            prefab.GetComponent<GamePiece>().Move(i, j, collapseTimePerTile * falseYOffset);
        }

        prefab.transform.parent = transform;
        return prefab.GetComponent<GamePiece>();
    }

    GameObject MakeBomb(GameObject prefab, int x, int y)
    {
        if (prefab != null && IsWithinBounds(x, y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
            bomb.GetComponent<Bomb>().Init(x, y, this);
            bomb.transform.parent = transform;
            return bomb;
        }
        return null;
    }

    void SetupTiles()
    {
        //setup special TileType(s)
        foreach (StartingObject item in startingTiles)
        {
            if (item != null && m_allTiles[item.x, item.y] == null)
                MakeTile(item.prefab, item.x, item.y, item.z);
        }

        //the rest is TileType.Normal
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allTiles[i, j] == null)
                {
                    MakeTile(tileNormalPrefab, i, j);
                }
            }
        }
    }

    void SetupGamePieces()
    {
        foreach (StartingObject item in startingGamePieces)
        {
            if (item != null && m_allGamePieces[item.x, item.y] == null
                && m_allTiles[item.x, item.y].tileType != TileType.Obstacle)
            {
                GameObject piece = Instantiate(item.prefab, new Vector3(item.x, item.y, 0), Quaternion.identity) as GameObject;
                MakeGamePiece(piece, item.x, item.y, height - item.x);
            }
        }
    }

    void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)width / 2 - .5f, (float)height / 2 - .5f, -10f);
        float aspectRatio = Screen.width / (float)Screen.height;
        float verticalSize = (float)height / 2 + borderSize;
        float horizontalSize = ((float)width / 2 + borderSize) / aspectRatio;
        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    }

    GameObject GetRandomObject(GameObject[] objectArray){
        int randomIdx = UnityEngine.Random.Range(0, objectArray.Length);
        return objectArray[randomIdx];
    }

    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        gamePiece.xIndex = x;
        gamePiece.yIndex = y;
        gamePiece.transform.position = new Vector3(x, y, 0);
        // gamePiece.transform.rotation = Quaternion.identity;
        if (IsWithinBounds(x, y))
            m_allGamePieces[x, y] = gamePiece;
    }

    GamePiece FillRandomGamePieceAt(int i, int j, int falseYOffset = 0)
    {
        GameObject randomPiece = Instantiate(GetRandomObject(gamePiecePrefabs), new Vector3(i, j, 0), Quaternion.identity);
        if (randomPiece != null)
            return MakeGamePiece(randomPiece, i, j, falseYOffset);
        return null;
    }

    GamePiece FillRandomCollectibleAt(int i, int j, int falseYOffset = 0)
    {
        GameObject randomPiece = Instantiate(GetRandomObject(collectiblePrefabs), new Vector3(i, j, 0), Quaternion.identity);
        if (randomPiece != null)
            return MakeGamePiece(randomPiece, i, j, falseYOffset);
        return null;
    }

    void FillBoard(int maxIterations = 0)
    {
        //Debug.Log(maxIterations);
        int falseYOffset = 0;
        if (maxIterations == 0)
        {
            falseYOffset = LowestNullTile();
            // Debug.Log("LowestNullTile="+falseYOffset);
        }
        int iterations;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePieces[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
                {
                    if (maxIterations > 0)
                    {
                        //first time call from Start()
                        if(j == height-1 && CanAddCollectible()){
                            FillRandomCollectibleAt(i, j, height - i);
                            collectibleCount++;
                        }
                        else
                            FillRandomGamePieceAt(i, j, height - i);
                    }
                    else
                    {
                        //later call from RefillRoutine()
                        if(j == height-1 && CanAddCollectible()){
                            FillRandomCollectibleAt(i, j, height - falseYOffset);
                            collectibleCount++;
                        }
                        else
                            FillRandomGamePieceAt(i, j, height - falseYOffset);
                        // FillRandomAt(i, j, height - falseYOffset - 4);
                    }
                    iterations = 0;
                    while (iterations < maxIterations && HasMatchOnFill(i, j))
                    {
                        ClearAPieceAt(i, j);
                        FillRandomGamePieceAt(i, j, height - i);
                        // FillRandomAt(i, j, falseYOffset);
                        iterations++;
                    }
                }
            }
        }
    }

    int LowestNullTile()
    {
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {
                if (m_allTiles[i, j].tileType != TileType.Obstacle && m_allGamePieces[i, j] == null) return j;
            }
        }
        return height - 1;
    }

    bool HasMatchOnFill(int i, int j, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatchesWithDirection(i, j, new Vector2(-1, 0), minLength);
        List<GamePiece> downwardMatches = FindMatchesWithDirection(i, j, new Vector2(0, -1), minLength);
        return leftMatches != null || downwardMatches != null;
    }

    public void ClickTile(Tile Tile)
    {
        if (m_clickedTile == null) m_clickedTile = Tile;
    }

    public void DragToTile(Tile Tile)
    {
        if (m_clickedTile != null)
        {
            if (IsNextTo(m_clickedTile, Tile))
            {
                m_targetTile = Tile;
                ReleaseTiles();
            }
            else
            {
                m_clickedTile = null;
            }
        }
    }

    public void ReleaseTiles()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            StartCoroutine(SwitchTilesRoutine(m_clickedTile, m_targetTile));
        }
        m_clickedTile = null;
        m_targetTile = null;
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInputEnabled)
        {
            m_playerInputEnabled = false;
            //2 tiles are only to get the position of the selected pieces
            //the actual work is to switch 2 pieces, not to switch 2 tiles
            GamePiece clickedPiece = m_allGamePieces[clickedTile.x, clickedTile.y];
            GamePiece targetPiece = m_allGamePieces[targetTile.x, targetTile.y];
            if (clickedPiece != null && targetPiece != null)
            {
                clickedPiece.Move(targetTile.x, targetTile.y, swapTime);
                targetPiece.Move(clickedTile.x, clickedTile.y, swapTime);
                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.x, clickedTile.y, 3);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.x, targetTile.y, 3);
                List<GamePiece> colorMatches = new List<GamePiece>();

                if(IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece)){
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                } 
                else if(!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece)){
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                }
                else if(IsColorBomb(clickedPiece) && IsColorBomb(targetPiece)){
                    colorMatches = m_allGamePieces.Cast<GamePiece>().ToList();
                }

                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.x, clickedTile.y, swapTime);
                    targetPiece.Move(targetTile.x, targetTile.y, swapTime);
                    yield return new WaitForSeconds(swapTime);
                    m_playerInputEnabled = true;
                }
                else
                {
                    GameManager.Instance.UpdateMoves();
                    
                    Vector2 swipeDirection = new Vector2(targetTile.x-clickedTile.x, targetTile.y-clickedTile.y);
                    m_clickedTileBomb = DropBomb(clickedTile.x, clickedTile.y, swipeDirection, clickedPieceMatches);
                    m_targetTileBomb = DropBomb(targetTile.x, targetTile.y, swipeDirection, targetPieceMatches);
                    if(m_clickedTileBomb != null && targetPiece != null){
                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                        if(!IsColorBomb(clickedBombPiece))
                            clickedBombPiece.ChangeColor(targetPiece);
                    }
                    if(m_targetTileBomb != null && clickedPiece != null){
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                        if(!IsColorBomb(targetBombPiece))
                            targetBombPiece.ChangeColor(clickedPiece);
                    }

                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList()
                                                            .Union(colorMatches).ToList());
                }
            }
        }
    }

    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.x - end.x) == 1 && start.y == end.y) return true;
        if (Mathf.Abs(start.y - end.y) == 1 && start.x == end.x) return true;
        return false;
    }

    List<GamePiece> FindMatchesWithDirection(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        if (IsWithinBounds(startX, startY)) startPiece = m_allGamePieces[startX, startY];

        if (startPiece != null) matches.Add(startPiece);
        else return null;

        int nextX, nextY;
        for (int i = 1; i < Mathf.Max(width, height); i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;
            if (!IsWithinBounds(nextX, nextY)) break;
            GamePiece nextPiece = m_allGamePieces[nextX, nextY];
            if (nextPiece == null) break;
            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }
        return (matches.Count >= minLength) ? matches : null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatchesWithDirection(startX, startY, new Vector2(0, 1), 2);
        List<GamePiece> downwardMatches = FindMatchesWithDirection(startX, startY, new Vector2(0, -1), 2);
        if (upwardMatches == null) upwardMatches = new List<GamePiece>();
        if (downwardMatches == null) downwardMatches = new List<GamePiece>();
        var combinedMatches = downwardMatches.Union(upwardMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatchesWithDirection(startX, startY, new Vector2(1, 0), minLength - 1);
        List<GamePiece> downwardMatches = FindMatchesWithDirection(startX, startY, new Vector2(-1, 0), minLength - 1);
        if (upwardMatches == null) upwardMatches = new List<GamePiece>();
        if (downwardMatches == null) downwardMatches = new List<GamePiece>();
        var combinedMatches = downwardMatches.Union(upwardMatches).ToList();
        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    List<GamePiece> FindMatchesAt(int i, int j, int minLength = 3)
    {
        List<GamePiece> horMatches = FindHorizontalMatches(i, j, minLength);
        List<GamePiece> verMatches = FindVerticalMatches(i, j, minLength);
        if (horMatches == null) horMatches = new List<GamePiece>();
        if (verMatches == null) verMatches = new List<GamePiece>();
        return horMatches.Union(verMatches).ToList();
    }

    List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        foreach (GamePiece item in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(item.xIndex, item.yIndex, minLength)).ToList();
        }
        return matches;
    }

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                combinedMatches = combinedMatches.Union(FindMatchesAt(i, j)).ToList();
            }
        }
        return combinedMatches;
    }

    void HighlightMatchesAt(int i, int j)
    {
        HighLightTileOff(i, j, 0f);
        var combinedMatches = FindMatchesAt(i, j, 3);
        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighLightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    IEnumerator HighLightTileOff(int i, int j, float secs)
    {
        yield return new WaitForSeconds(secs);
        SpriteRenderer spriteRenderer = m_allTiles[i, j].GetComponent<SpriteRenderer>();

        spriteRenderer.color = new Color(
            spriteRenderer.color.r,
            spriteRenderer.color.g,
            spriteRenderer.color.b, 0);

        if (m_allTiles[i, j].tileType == TileType.Breakable)
        {
            if (m_allTiles[i, j].breakableValue > 0)
                spriteRenderer.color = Color.white;
        }
    }

    void HighLightTileOn(int i, int j, Color color)
    {
        SpriteRenderer spriteRenderer = m_allTiles[i, j].GetComponent<SpriteRenderer>();
        spriteRenderer.color = color;
        StartCoroutine(HighLightTileOff(i, j, collapseTimePerTile * 2));
    }

    void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x, y];
        if (tileToBreak != null)
        {
            m_particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y);
            tileToBreak.BreakTile();
        }
    }

    void BreakTilesAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece item in gamePieces)
        {
            if (item != null)
                BreakTileAt(item.xIndex, item.yIndex);
        }
    }

    void ClearAPieceAt(int x, int y)
    {
        GamePiece piece = m_allGamePieces[x, y];
        if (piece != null)
        {
            m_allGamePieces[x, y] = null;
            Destroy(piece.gameObject);
        }
        // StartCoroutine(HighLightTileOff(x, y, collapseTimePerTile));
    }

    void ClearPiecesAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece item in gamePieces)
        {
            if (item != null)
            {
                ClearAPieceAt(item.xIndex, item.yIndex);
                item.ScorePoints(m_scoreMultiplier, (gamePieces.Count > 4) ? 20 : 0);
                if(bombedPieces.Contains(item)){
                    m_particleManager.BombFXAt(item.xIndex, item.yIndex);
                } else {
                    m_particleManager.ClearPieceFXAt(item.xIndex, item.yIndex);
                }
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearAPieceAt(i, j);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePieces[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePieces[column, j] != null)
                    {
                        //Debug.Log("move over " + (j - i) + " tile with time: " + collapseTime * (j - i));
                        m_allGamePieces[column, j].Move(column, i, collapseTime * (j - i));
                        m_allGamePieces[column, i] = m_allGamePieces[column, j];
                        m_allGamePieces[column, j] = null;
                        m_allGamePieces[column, i].SetCoord(column, i);
                        if (!movingPieces.Contains(m_allGamePieces[column, i]))
                        {
                            movingPieces.Add(m_allGamePieces[column, i]);
                        }
                        break;
                    }
                }
            }
        }
        return movingPieces;
    }

    List<GamePiece> CollapseColumns(List<GamePiece> gamePieces)
    {
        List<int> ints = new List<int>();
        foreach (GamePiece item in gamePieces)
        {
            if (!ints.Contains(item.xIndex)) ints.Add(item.xIndex);
        }
        // return ints;
        List<GamePiece> movingPieces = new List<GamePiece>();
        foreach (int item in ints)
        {
            movingPieces = movingPieces.Union(CollapseColumn(item, collapseTimePerTile)).ToList();
        }
        //Debug.Log("finish collapse");
        return movingPieces;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePieces)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
    {
        m_playerInputEnabled = false;
        m_scoreMultiplier = 0;
        do
        {
            m_scoreMultiplier++;
            //wait for the ClearAndCollapseRoutine to finish
            yield return StartCoroutine(ClearAndCollapseRoutine(gamePieces));

            //yield return null;

            while (!FinishFalling(m_allGamePieces.Cast<GamePiece>().ToList()))
            {
                // Debug.Log("pieces are falling please wiat");
                yield return null;
            };
            yield return StartCoroutine(RefillRoutine());
            while (!FinishFalling(m_allGamePieces.Cast<GamePiece>().ToList()))
            {
                // Debug.Log("pieces are falling please wiat");
                yield return null;
            };
            gamePieces = FindAllMatches();
            yield return new WaitForSeconds(collapseTimePerTile * 2);

        } while (gamePieces.Count > 0);
        m_playerInputEnabled = true;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard();
        yield return null;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePiecesToClear)
    {
        List<GamePiece> falledPieces = new List<GamePiece>();
        List<GamePiece> newMatches = new List<GamePiece>();
        while (true)
        {
            // find pieces affected by bombs
            List<GamePiece> bombedPieces = GetBombedPieces(gamePiecesToClear);
            gamePiecesToClear = gamePiecesToClear.Union(bombedPieces).ToList();

            //bomb explodes other bombs
            bombedPieces = GetBombedPieces(gamePiecesToClear);
            gamePiecesToClear = gamePiecesToClear.Union(bombedPieces).ToList();

            //collect collectibles if possible
            //collecttible a at bottom row
            List<GamePiece> collectedPieces = FindCollectiblesAtRow(0, bottomOnly:true);
            List<GamePiece> allCollectibles = FindAllCollectibles();
            //blocker is also a collectible
            List<GamePiece> blockers = gamePiecesToClear.Intersect(allCollectibles).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();
            collectibleCount -= collectedPieces.Count;
            gamePiecesToClear = gamePiecesToClear.Union(collectedPieces).ToList();

            yield return new WaitForSeconds(collapseTimePerTile * 2);
            ClearPiecesAt(gamePiecesToClear, bombedPieces);
            BreakTilesAt(gamePiecesToClear);

            // add bombs to Board here (if needed)
            if(m_clickedTileBomb != null) {
                ActivateBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }
            if(m_targetTileBomb != null) {
                ActivateBomb(m_targetTileBomb);
                m_targetTileBomb = null;
            }

            yield return new WaitForSeconds(collapseTimePerTile * 2);
            falledPieces = CollapseColumns(gamePiecesToClear);

            while (!FinishFalling(falledPieces))
            {
                //Debug.Log("pieces are falling please wiat");
                yield return null;
            };

            yield return new WaitForSeconds(collapseTimePerTile);
            newMatches = FindMatchesAt(falledPieces);
            collectedPieces = FindCollectiblesAtRow(0, bottomOnly:true);
            newMatches = newMatches.Union(collectedPieces).ToList();
            if (newMatches.Count == 0)
            {
                //Debug.Log("no more matches");
                break;
            }
            gamePiecesToClear = newMatches;
            m_scoreMultiplier++;
            SoundManager.Instance.PlayRandomBonusSound();
            yield return new WaitForSeconds(collapseTimePerTile);
        }
    }

    bool FinishFalling(List<GamePiece> gamePieces)
    {
        foreach (GamePiece item in gamePieces)
        {
            if (item != null && item.transform.position.y - (float)item.yIndex > 0.001f) return false;
        }
        return true;
    }

    List<GamePiece> GetPiecesAtRow(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            if (m_allGamePieces[i, row] != null)
            {
                gamePieces.Add(m_allGamePieces[i, row]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetPiecesAtColumn(int col)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = 0; i < height; i++)
        {
            if (m_allGamePieces[col, i] != null)
            {
                gamePieces.Add(m_allGamePieces[col, i]);
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetPiecesAdjacent(int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();
        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (IsWithinBounds(i, j) && m_allGamePieces[i, j] != null)
                {
                    gamePieces.Add(m_allGamePieces[i, j]);
                }
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecesToClear = new List<GamePiece>();
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();
                Bomb bomb = piece.GetComponent<Bomb>();
                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetPiecesAtColumn(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetPiecesAtRow(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetPiecesAdjacent(bomb.xIndex, bomb.yIndex);
                            break;
                        case BombType.Color:
                        default:
                            break;
                    }
                }
                allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList();
            }
        }
        allPiecesToClear = RemoveCollectiblesFrom(allPiecesToClear);
        return allPiecesToClear;
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false, horizontal = false;
        int xStart = -1, yStart = -1;
        foreach (GamePiece item in gamePieces)
        {
            if (item != null)
            {
                if (xStart == -1 && yStart == -1)
                {
                    xStart = item.xIndex;
                    yStart = item.yIndex;
                }
                if (item.xIndex != xStart && item.yIndex == yStart) horizontal = true;
                if (item.xIndex == xStart && item.yIndex != yStart) vertical = true;
            }
        }
        return horizontal && vertical;
    }

    GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        GameObject bomb = null;
        if (gamePieces.Count >= 4)
        {
            if (IsCornerMatch(gamePieces))
            {
                bomb = MakeBomb(adjacentPrefab, x, y);
            }
            else
            {
                if(gamePieces.Count >= 5) {
                    bomb = MakeBomb(colorBombPrefab, x, y);
                }
                else {
                    if (swapDirection.x != 0) bomb = MakeBomb(rowBombPrefab, x, y);
                    else bomb = MakeBomb(columnBombPrefab, x, y);
                }
            }
        }
        return bomb;
    }

    void ActivateBomb(GameObject bomb){
        int x = (int) bomb.transform.position.x;
        int y = (int) bomb.transform.position.y;
        if(IsWithinBounds(x, y)) m_allGamePieces[x, y] = bomb.GetComponent<GamePiece>();
    }

    List<GamePiece> FindAllMatchValue(MatchValue mValue){
        List<GamePiece> foundPieces = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(m_allGamePieces[i, j] != null){
                    if(m_allGamePieces[i, j].matchValue == mValue){
                        foundPieces.Add(m_allGamePieces[i, j]);
                    }
                }
            }
        }
        return foundPieces;
    }

    bool IsColorBomb(GamePiece gamePiece){
        Bomb bomb = gamePiece.GetComponent<Bomb>();
        if(bomb != null) return bomb.bombType == BombType.Color;
        return false;
    }

    List<GamePiece> FindCollectiblesAtRow(int row, bool bottomOnly){
        List<GamePiece> foundCollectibles = new List<GamePiece>();
        for(int i = 0; i < width; i++){
            if(m_allGamePieces[i, row] != null){
                Collectible collectible = m_allGamePieces[i, row].GetComponent<Collectible>();
                if(collectible != null) {
                    if(!bottomOnly || (bottomOnly && collectible.clearedAtBottom)){
                        foundCollectibles.Add(m_allGamePieces[i, row]);
                    }
                }
            }
        }
        return foundCollectibles;
    }

    List<GamePiece> FindAllCollectibles(){
        List<GamePiece> foundCollectibles = new List<GamePiece>();
        for(int i = 0; i < height; i++){
            List<GamePiece> collectibleRow = FindCollectiblesAtRow(i, bottomOnly:false);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }
        return foundCollectibles;
    }

    bool CanAddCollectible(){
        return UnityEngine.Random.Range(0f, 1f) <= chanceForCollectible
            && collectiblePrefabs.Length > 0
            && collectibleCount < maxCollectibles;
    }

    List<GamePiece> RemoveCollectiblesFrom(List<GamePiece> gameObjects){
        List<GamePiece> res = new List<GamePiece>();
        foreach (GamePiece item in gameObjects)
        {
            Collectible collectible = item.GetComponent<Collectible>();
            if(collectible == null || collectible.clearedByBomb){
                res.Add(item);
            }
        }
        return res;
    }

}
