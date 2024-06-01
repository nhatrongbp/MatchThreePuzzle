using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardShuffler : MonoBehaviour
{
    // returns list of non-bomb and non-collectible pieces
    public List<GamePiece> RemoveNormalPieces(GamePiece[,] allPieces)
    {
        List<GamePiece> normalPieces = new List<GamePiece>();
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // ... if it's not a null object (i.e. a hole caused by Obstacle)
                if (allPieces[i, j] != null)
                {
                    Bomb bomb = allPieces[i, j].GetComponent<Bomb>();
                    Collectible collectible = allPieces[i, j].GetComponent<Collectible>();
                    if (bomb == null && collectible == null)
                    {
                        normalPieces.Add(allPieces[i, j]);
                        // and clear position from original array
                        allPieces[i, j] = null;
                    }
                }
            }
        }
        return normalPieces;
    }

    public void ShuffleList(List<GamePiece> piecesToShuffle)
    {
        int maxCount = piecesToShuffle.Count;
        for (int i = 0; i < maxCount - 1; i++)
        {
            int r = Random.Range(i, maxCount);
            if (r == i) continue;
            //CHANGE THE X Y ATTRIBUTES OF THE PIECE
            GamePiece temp = piecesToShuffle[r];
            piecesToShuffle[r] = piecesToShuffle[i];
            piecesToShuffle[i] = temp;
        }
    }

    public void MovePieces(GamePiece[,] allPieces, float swapTime = 0.5f)
    {
        int width = allPieces.GetLength(0);
        int height = allPieces.GetLength(1);

        // the x y attributes od the piece was changed, and now we change its transform position
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allPieces[i, j] != null)
                {
                    //CHANGE THE TRANSFORM POSITION OF THE PIECE
                    allPieces[i, j].Move(i, j, swapTime);
                    //we will change the piece's index in m_allGamePieces later in Board.cs 
                }
            }
        }

    }




}
