using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField]
    PieceCrushAnimation crush;
    [SerializeField]
    GameObject jack;

    SpriteRenderer sr;
    public HexCell cell { get; private set; }
    public int colorIndex { get; private set; }

    public int[] axisPair { get; private set; }
    public int specialIndex { get; private set; } = -1;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void SetCell(HexCell c)
    {
        cell = c;
    }

    public void SetSpecial0(int color, Sprite spr, int[] axisPair)
    {
        crush.SetMaterial(color);
        colorIndex = color;
        sr.sprite = spr;
        specialIndex = 0;
        this.axisPair = axisPair;

        switch (axisPair[0])
        {
            case 0: //우상단 - 좌하단
                transform.rotation = Quaternion.Euler(0, 0, -60);
                break;
            case 1: //우하단 - 좌상단
                transform.rotation = Quaternion.Euler(0, 0, -120);
                break;
            case 2: //그대로
                break;
        }
    }

    public void SetColor(int color, Sprite spr)
    {
        specialIndex = -1;
        crush.SetMaterial(color);
        colorIndex = color;
        sr.sprite = spr;
    }

    // 잭 인 더 박스 애니메이션
    public void SetActiveJack()
    {
        cell.ActivePoint(300);
        jack.SetActive(true);
    }

    // 파괴 애니메이션
    public void SetActiveCrush()
    {
        crush.transform.position = transform.position;
        crush.gameObject.SetActive(true);
    }

    // 이동 - 큐 방식
    Queue<Vector3> moveQueue = new Queue<Vector3>();

    public void EnqueueMove(Vector3 target)
    {
        moveQueue.Enqueue(target);
    }

    bool isMove = false;

    public void MoveStart()
    {
        if (!isMove)
            MoveToQueue();
    }

    public void MoveToQueue()
    {
        if(moveQueue.Count > 0)
        {
            isMove = true;
            transform.DOMove(moveQueue.Dequeue(), 0.1f)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    if (moveQueue.Count > 0)
                        MoveToQueue();
                    else
                    {
                        transform.DOShakePosition(0.2f, 0.02f);
                        isMove = false;
                    }
                });
        }    
    }

    // 이동 애니메이션
    public void MoveTo(Vector3 target, float duration = 0.3f)
    {
        transform.DOMove(target, duration)
            .OnComplete(() =>
            {
                // 도착 후 흔들기
                transform.DOShakePosition(0.2f, 0.02f);
            });
    }

    public void MoveToSwap(Vector3 target)
    {
        transform.DOMove(target, 0.3f);
    }

    public void SetHintAnimation(HexCell target)
    {
        Vector2 dir = (target.transform.position - transform.position).normalized;

        transform.DOMove((Vector2)transform.position + (dir * 0.1f), 0.3f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);

    }

    public void EndHintAnimation()
    {
        DOTween.Kill(transform);
        transform.position = cell.transform.position;
    }
}
