using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField]
    BoardManager board;

    Piece selectedPiece;
    bool isDragging = false;

    void Update()
    {
        // 모바일
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch(touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchDown(touch.position);
                    break;
                case TouchPhase.Moved:
                    OnTouchDrag(touch.position);
                    break;
                case TouchPhase.Ended:
                    OnTouchUp(touch.position);
                    break;
            }
        }

        // PC 테스트 용
        if (Input.GetMouseButtonDown(0))
        {
            OnTouchDown(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            OnTouchDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            OnTouchUp(Input.mousePosition);
        }
    }

    void OnTouchDown(Vector3 touchPos)
    {
        // 터치 포지션 변환
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos);

        // Raycast로 구슬 찾기
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Piece piece = hit.collider.GetComponent<Piece>();
            if (piece != null)
            {
                selectedPiece = piece;
                isDragging = true;
            }
        }
    }

    void OnTouchDrag(Vector3 touchPos)
    {
        if (!isDragging || selectedPiece == null)
            return;

        // 2번째 구슬 찾기
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(touchPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
        {
            Piece target = hit.collider.GetComponent<Piece>();
            if (target != null && target != selectedPiece)
            {
                // 두 구슬이 붙어있는지 확인
                if (BoardUtil.AreNeighbors(selectedPiece.cell, target.cell))
                {
                    // 붙어있으면, 자리 바꾸기
                    board.OnSwap(selectedPiece.cell, target.cell);
                    selectedPiece = null;
                }

                isDragging = false; // 이번 터치는 끝
            }
        }
    }

    // 리셋
    void OnTouchUp(Vector3 touchPos)
    {
        isDragging = false;
        selectedPiece = null;
    }
}
