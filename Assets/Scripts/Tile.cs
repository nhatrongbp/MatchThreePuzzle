using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType{
    Normal, 
    Obstacle,   //tunnel
    Breakable   //jelly
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    //tile is just a wrapper of a gamePiece
    //tile has static position
    //tile is waiting for user input
    //tile has a sprite renderer used for highlighting the piece's color while debugging
    public int x, y;
    Board m_board;
    public TileType tileType = TileType.Normal;
    SpriteRenderer m_spriteRenderer;
    public int breakableValue = 0;
    public Sprite[] breakableSprites;
    public Color normalColor;

    void Awake(){
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y, Board board){
        this.x = x; this.y = y;
        m_board = board;
        if(tileType == TileType.Breakable && breakableSprites[breakableValue] != null){
            m_spriteRenderer.sprite = breakableSprites[breakableValue];
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown(){
        if(m_board != null){
            m_board.ClickTile(this);
        }
    }

    void OnMouseEnter(){
        if(m_board != null){
            m_board.DragToTile(this);
        }
    }

    // void OnMouseUp(){
    //     if(m_board != null){
    //         m_board.ReleaseTiles();
    //     }
    // }

    public void BreakTile(){
        if(tileType != TileType.Breakable) return;
        StartCoroutine(BreakTileRoutine());
    }

    IEnumerator BreakTileRoutine(){
        breakableValue--;
        yield return new WaitForSeconds(m_board.collapseTimePerTile);
        if(breakableSprites[breakableValue] != null){
            m_spriteRenderer.sprite = breakableSprites[breakableValue];
        }
        if(breakableValue == 0) {
            Debug.Log("to normal");
            tileType = TileType.Normal;
            m_spriteRenderer.color = normalColor;
        }
    }
}
