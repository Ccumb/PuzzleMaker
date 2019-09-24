﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    public GameState currentState = GameState.move;
    public int width;
    public int height;

    public int offset;

    public GameObject tilePrefab;
    public GameObject breakableTilePrefab;
    public GameObject[] dotPrefabs;

    public GameObject[,] dots;
    
    public Dot currentDot;
    private FindMatches mFindMatches;

    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public TileType[] boardLayout;

    public int basePieceValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;
    public float refillDelay = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        scoreManager = FindObjectOfType<ScoreManager>();
        mFindMatches = FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        dots = new GameObject[width, height];
        breakableTiles = new BackgroundTile[width, height];
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GenerateBlankSpace()
    {
        for(int i = 0; i < boardLayout.Length; i++)
        {
            if(boardLayout[i].tileKind == TileKind.Blank)
            {
                blankSpaces[boardLayout[i].x, boardLayout[i].y] = true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        // Look at all the tiles in the layout
        for(int i = 0; i < boardLayout.Length; i++)
        {
            // if a tile is a Jelly tile
            if(boardLayout[i].tileKind == TileKind.Breakable)
            {
                // Create a Jelly tile at that pos
                Vector2 pos = new Vector2(boardLayout[i].x, boardLayout[i].y);
                GameObject tile = Instantiate(breakableTilePrefab, pos, Quaternion.identity);
                breakableTiles[boardLayout[i].x, boardLayout[i].y] = tile.GetComponent<BackgroundTile>();
            }
        }
    }

    private void SetUp()
    {
        GenerateBlankSpace();
        GenerateBreakableTiles();

        for (int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    Vector2 pos = new Vector2(i, j + offset);
                    Vector2 tilePos = new Vector2(i, j);

                    GameObject tile = Instantiate(tilePrefab, tilePos, Quaternion.identity) as GameObject;
                    tile.transform.parent = this.transform;
                    tile.name = "( " + i + ", " + j + " )";

                    int dotToUse = Random.Range(0, dotPrefabs.Length);
                    int maxIteration = 0;
                    while (MatchesAt(i, j, dotPrefabs[dotToUse]) && maxIteration < 100)
                    {
                        dotToUse = Random.Range(0, dotPrefabs.Length);

                        maxIteration++;
                        Debug.Log(maxIteration);
                    }
                    maxIteration = 0;

                    GameObject dot = Instantiate(dotPrefabs[dotToUse], pos, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().column = i;

                    dot.transform.parent = this.transform;
                    dot.name = "( " + i + ", " + j + " )";

                    dots[i, j] = dot;
                }
            }
        }
    }

    bool MatchesAt(int column, int row, GameObject piece)
    {
        if(column > 1 && row > 1)
        {
            if (dots[column - 1, row] != null && dots[column - 2, row] != null)
            {
                if (dots[column - 1, row].tag == piece.tag && dots[column - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (dots[column, row - 1] != null && dots[column, row - 2] != null)
            {
                if (dots[column, row - 1].tag == piece.tag && dots[column, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        else if(column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (dots[column, row - 1] != null && dots[column, row - 2] != null)
                {
                    if (dots[column, row - 1].tag == piece.tag && dots[column, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (column > 1)
            {
                if (dots[column - 1, row] != null && dots[column - 2, row] != null)
                {
                    if (dots[column - 1, row].tag == piece.tag && dots[column - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    bool ColumnOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;

        Dot firstPiece = mFindMatches.currentMatches[0].GetComponent<Dot>();

        if(firstPiece != null)
        {
            foreach (GameObject currentPiece in mFindMatches.currentMatches)
            {
                Dot dot = currentPiece.GetComponent<Dot>();
                if(dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if(dot.column == firstPiece.column)
                {
                    numberVertical++;
                }
            }
        }
        return (numberHorizontal == 5 || numberVertical == 5);
    }

    void CheckToMakeBombs()
    {
        if(mFindMatches.currentMatches.Count == 4 || mFindMatches.currentMatches.Count == 7)
        {
            mFindMatches.CheckBombs();
        }
        if(mFindMatches.currentMatches.Count == 5 || mFindMatches.currentMatches.Count == 8)
        {
            if (ColumnOrRow())
            {
                // Make a color bomb
                // is the current dot matched?
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.mOtherDot != null)
                        {
                            Dot otherDot = currentDot.mOtherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Make a adjacent bomb
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjacentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.mOtherDot != null)
                        {
                            Dot otherDot = currentDot.mOtherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjacentBomb();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    void DestroyMatchesAt(int column, int row)
    {
        if(dots[column, row].GetComponent<Dot>().isMatched)
        {
            // How many elements are in the matched pieces list from findmatches?
            if(mFindMatches.currentMatches.Count >= 4)
            {
                CheckToMakeBombs();
            }

            // Does a tile need to break?
            if(breakableTiles[column, row] != null)
            {
                // if it does, give a one damage
                breakableTiles[column, row].TakeDamage(1);
                
                if(breakableTiles[column, row].hitPoints <= 0)
                {
                    breakableTiles[column, row] = null;
                }
            }
           
            Destroy(dots[column, row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            dots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(dots[i, j] != null)
                {
                    DestroyMatchesAt(i, j);
                }
            }
        }
        mFindMatches.currentMatches.Clear();
        StartCoroutine("DecreaseRowCo2");
    }

    IEnumerator DecreaseRowCo2()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                // if the current spot isn't blank or empty
                if(!blankSpaces[i, j] && dots[i, j] == null)
                {
                    // loop from the space above to the top of the column
                    for(int k = j + 1; k < height; k++)
                    {
                        // if a dot is found
                        if(dots[i, k] != null)
                        {
                            // move that dot to this empty space
                            dots[i, k].GetComponent<Dot>().row = j;
                            // set that spot to be null
                            dots[i, k] = null;
                            // break out of the loop
                            break;
                        }
                    }
                }
            }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine("FillBoardCo");
    }

    IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(dots[i, j] == null)
                {
                    nullCount++;
                }
                else if(nullCount > 0)
                {
                    dots[i, j].GetComponent<Dot>().row -= nullCount;
                    dots[i, j] = null;
                }
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);

        StartCoroutine("FillBoardCo");
    }

    void RefillBoard()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(dots[i, j] == null && !blankSpaces[i, j])
                {
                    Vector2 tmpPos = new Vector2(i, j + offset);
                    int dotToUse = Random.Range(0, dotPrefabs.Length);
                    int maxIterations = 0;

                    while(MatchesAt(i, j, dotPrefabs[dotToUse]) && maxIterations < 100)
                    {
                        maxIterations++;
                        dotToUse = Random.Range(0, dotPrefabs.Length);
                    }

                    maxIterations = 0;

                    GameObject piece = Instantiate(dotPrefabs[dotToUse], tmpPos, Quaternion.identity);
                    piece.transform.parent = this.transform;
                    dots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().column = i;
                }
            }
        }
    }

    bool MatchesOnBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(dots[i, j] != null)
                {
                    if(dots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);

        while(MatchesOnBoard())
        {
            streakValue ++;
            yield return new WaitForSeconds(2 * refillDelay);
            DestroyMatches();
        }
        mFindMatches.currentMatches.Clear();
        currentDot = null;

        if(IsDeadLocked())
        {
            StartCoroutine(ShuffleBoard());
            Debug.Log("DeadLocked!!");
        }

        yield return new WaitForSeconds(refillDelay);
        currentState = GameState.move;
        streakValue = 1;
    }

    private void SwitchPieces(int column, int row, Vector2 direction)
    {
        // Take the second piece and save it in a holder
        GameObject holder = dots[column + (int)direction.x, row + (int)direction.y] as GameObject;
        // switching the first dot to be the second position
        dots[column + (int)direction.x, row + (int)direction.y] = dots[column, row];
        // Set the first dot to be the second dot
        dots[column, row] = holder;
    }

    private bool CheckForMatches()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(dots[i, j] != null)
                {
                    //Make sure that one and two to the right are in the board
                    if (i < width - 2)
                    {
                        // Check if the dots to the right and two to the right exist
                        if (dots[i + 1, j] != null && dots[i + 2, j] != null)
                        {
                            if (dots[i + 1, j].tag == dots[i, j].tag
                                && dots[i + 2, j].tag == dots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        // Check if the dots above exist
                        if (dots[i, j + 1] != null && dots[i, j + 2] != null)
                        {
                            if (dots[i, j + 1].tag == dots[i, j].tag
                                && dots[i, j + 2].tag == dots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }

        }
        return false;
    }

    public bool SwitchAndCheck(int column, int row, Vector2 direction)
    {
        SwitchPieces(column, row, direction);
        
        if(CheckForMatches())
        {
            SwitchPieces(column, row, direction);
            return true;
        }
        SwitchPieces(column, row, direction);
        return false;
    }

    private bool IsDeadLocked()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(dots[i, j] != null)
                {
                    if(i < width - 1)
                    {
                        if(SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if(j < height - 1)
                    {
                        if(SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(0.5f);
        // Create a list of game object
        List<GameObject> newBoard = new List<GameObject>();
        // Add every piece to this list
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if (dots[i, j] != null)
                {
                    newBoard.Add(dots[i, j]);
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
        // for every spot on the board
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // if this spot shouldn't be blank
                if(!blankSpaces[i, j])
                {
                    // pick a random number 
                    int pieceToUse = Random.Range(0, newBoard.Count);
                    
                    // Assign the column to the piece
                    int maxIteration = 0;

                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIteration < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);

                        maxIteration++;
                        Debug.Log(maxIteration);
                    }
                    maxIteration = 0;

                    // Make a container for piece
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();

                    piece.column = i;
                    // Assign the row to the piece
                    piece.row = j;
                    // Fill in the dots array with this new piece
                    dots[i, j] = newBoard[pieceToUse];
                    // Remove it from the list
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }
        // Check if it's stil deadlocked
        if(IsDeadLocked())
        {
            ShuffleBoard();
        }
    }

}