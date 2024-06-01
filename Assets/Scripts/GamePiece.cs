using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MatchValue{ Red, Green, Blue, Yellow, Orange, Turtle, Wild, None };
public class GamePiece : MonoBehaviour
{
    public int xIndex, yIndex;
    bool m_isMoving;
    Board m_board;
    public InterpType interpType;
    public enum InterpType{ Linear, EaseOut, EaseIn, SmoothStep, SmootherStep };

    public MatchValue matchValue;
    public int scoreValue = 20;
    public AudioClip clearSound;

    // Start is called before the first frame update
    void Start()
    {
        m_isMoving = false;
    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.LeftArrow)){
        //     Move((int)transform.position.x - 2, (int)transform.position.y, 0.5f);
        // }
        // if(Input.GetKeyDown(KeyCode.RightArrow)){
        //     Move((int)transform.position.x + 2, (int)transform.position.y, 0.5f);
        // }
    }

    public void Init(int x, int y, Board board){
        xIndex = x; yIndex = y;
        m_board = board;
    }
    
    public void SetCoord(int x, int y){
        xIndex = x; yIndex = y;
    }

    public void Move(int destX, int destY, float totalTime){
        if(!m_isMoving){
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), totalTime));
        }
    }

    IEnumerator MoveRoutine(Vector3 destination, float totalTime){
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;
        
        m_isMoving = true;

        while(true){
            if(Vector3.Distance(transform.position, destination) < 0.01f){
                m_board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                break;
            }
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp(elapsedTime / totalTime, 0f, 1f);
            switch (interpType){
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0/5f);
                    break;
                case InterpType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0/5f);
                    break;
                case InterpType.SmoothStep:
                    t = t*t*(3 - 2*t);
                    break;
                case InterpType.SmootherStep:
                    t = t*t*t*(t*(t*6 - 15) + 10);
                    break;
            }

            transform.position = Vector3.Lerp(startPosition, destination, t);

            yield return null;
        }

        m_isMoving = false;
    }

    public void ChangeColor(GamePiece pieceToMatch){
        SpriteRenderer renderToChange = GetComponent<SpriteRenderer>();
        // Color colorToMatch = Color.clear;
        if(pieceToMatch != null){
            SpriteRenderer rendererToMatch = pieceToMatch.GetComponent<SpriteRenderer>();
            // if(rendererToMatch != null && renderToChange != null){
            //     renderToChange.color = rendererToMatch.color;
            // }
            matchValue = pieceToMatch.matchValue;
        }
    }


}
