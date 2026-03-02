using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HexCell : MonoBehaviour
{
    // offset : 생성, 이동 등 단순 좌표 계산용
    // 큐브 : 매치 및 이웃 판별 등 복잡한 알고리즘용

    [SerializeField]
    GameObject hintMark;

    [SerializeField]
    TMP_Text pointText;

    public Vector2Int offset { get; private set; } // offset 좌표
    public Vector3Int cube { get; private set; } // 큐브 좌표
    public bool isActive { get; private set; }
    public Piece piece { get; private set; }


    public void SetHexCell(int q, int r, bool active = true)
    {
        offset = new Vector2Int(q, r);
        isActive = active;
        piece = null;
        cube = BoardUtil.EvenQToCube(q, r);
    }

    public void SetPiece(Piece piece)
    {
        this.piece = piece;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
        isActive = active;
    }

    public void SetHint(bool isActive)
    {
        hintMark.SetActive(isActive);
    }

    Coroutine pointCoroutine;

    public void ActivePoint(int point)
    {
        if(pointCoroutine != null) StopCoroutine(pointCoroutine);

        GameManager.Instance.AddPoint(point);
        pointText.text = point.ToString();
        pointText.gameObject.SetActive(true);
        pointCoroutine = StartCoroutine(DisablePoint());
    }

    IEnumerator DisablePoint()
    {
        yield return new WaitForSeconds(0.75f);
        pointText.gameObject.SetActive(false);
    }
}