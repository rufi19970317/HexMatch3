using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintFinder
{
    public Piece hintPiece { get; private set; }
    public HexCell hintTarget { get; private set; }
    public List<HexCell> hintMarks { get; private set; } = new List<HexCell>();

    public void FindHint(HexCell[,] cells)
    {
        int width = cells.GetLength(0);
        int height = cells.GetLength(1);

        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                HexCell cell = cells[q, r];
                if (cell == null | !cell.isActive || cell.piece == null || cell.piece.colorIndex == 6) continue;

                // 6방향 검사
                foreach (Vector3Int dir in BoardUtil.cubeDirs)
                {
                    Vector3Int targetCube = cell.cube + dir;
                    Vector2Int targetOffset = BoardUtil.CubeToEvenQ(targetCube);

                    if (!BoardUtil.InBounds(width, height, targetOffset.x, targetOffset.y)) continue;

                    HexCell targetCell = cells[targetOffset.x, targetOffset.y];
                    if (targetCell == null || !targetCell.isActive || targetCell.piece == null || targetCell.piece.colorIndex == 6) continue;

                    // 스왑
                    HintSwap(cell, targetCell);

                    List<Piece> matches = MatchFinder.FindMatches(cells);

                    // 매치 확인
                    if (matches.Count > 0)
                    {
                        // 힌트 추가
                        foreach (Piece piece in matches)
                        {
                            hintMarks.Add(piece.cell);
                        }
                    }

                    // 다시 스왑
                    HintSwap(cell, targetCell);

                    if (matches.Count > 0)
                    {
                        // 매치에 포함 되어있는 피스가 힌트 피스
                        if (matches.Contains(cell.piece))
                        {
                            hintPiece = cell.piece;
                            hintTarget = targetCell;
                        }
                        else
                        {
                            hintPiece = targetCell.piece;
                            hintTarget = cell;
                        }
                        return;
                    }
                }
            }
        }
        return;

    }

    // 힌트용 스왑
    void HintSwap(HexCell a, HexCell b)
    {
        Piece temp = a.piece;
        a.SetPiece(b.piece);
        b.SetPiece(temp);

        a.piece.SetCell(a);
        b.piece.SetCell(b);
    }
}
