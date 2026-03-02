using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField]
    GameObject piecePrefab;

    Queue<Piece> piecePool = new Queue<Piece>();

    public void InitPool(LevelData levelData)
    {
        int piecePoolCount = levelData.width * levelData.height * 2;

        for(int i = 0; i < piecePoolCount; i++)
        {
            Piece piece = Instantiate(piecePrefab, transform).GetComponent<Piece>();
            piece.gameObject.SetActive(false);
            piecePool.Enqueue(piece);
        }
    }

    public Piece GetPiece()
    {
        if(piecePool.Count > 0)
        {
            Piece piece = piecePool.Dequeue();
            piece.gameObject.SetActive(true);
            return piece;
        }
        else
        {
            Piece piece = Instantiate(piecePrefab, transform).GetComponent<Piece>();
            piece.gameObject.SetActive(true);
            return piece;
        }
    }

    public void RetrunPiece(Piece piece)
    {
        piece.gameObject.SetActive(false);
        piece.transform.rotation = Quaternion.identity;
        piecePool.Enqueue(piece);
    }
}
