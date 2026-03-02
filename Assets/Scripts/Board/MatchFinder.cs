using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchFinder
{
    // 매치 찾기
    public static List<Piece> FindMatches(HexCell[,] cells, bool isGameEnd = false)
    {
        int width = cells.GetLength(0);
        int height = cells.GetLength(1);

        List<Piece> results = new List<Piece>();
        HashSet<Vector2Int> matched = new HashSet<Vector2Int>();

        HashSet<Piece> specialPieces = new HashSet<Piece>();

        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                HexCell cell = cells[q, r];
                if (cell == null || !cell.isActive || cell.piece == null || cell.piece.colorIndex == 6) continue;

                // 게임 끝났을 때, 특수 구슬이면 전부 넣고 터뜨리기
                if (isGameEnd && cell.piece.specialIndex >= 0)
                    specialPieces.Add(cell.piece);

                int color = cell.piece.colorIndex;
                Vector3Int start = cell.cube;

                // 축 검사
                foreach (int[] pair in BoardUtil.axisPairs)
                {
                    List<HexCell> line = new List<HexCell>();
                    line.Add(cell);

                    foreach (int dirIndex in pair)
                    {
                        Vector3Int dir = BoardUtil.cubeDirs[dirIndex]; // 방향
                        Vector3Int cur = start; // 현재 위치

                        // 축 순환
                        while (true)
                        {
                            // 다음 좌표가 맵 내에 있는지 확인
                            cur += dir;
                            Vector2Int offset = BoardUtil.CubeToEvenQ(cur);
                            if (!BoardUtil.InBounds(width, height, offset.x, offset.y)) break;

                            // 다음 셀로 이동
                            HexCell next = cells[offset.x, offset.y];

                            // 빈칸이거나 색이 다르면 중지
                            if (next == null || !next.isActive || next.piece == null || next.piece.colorIndex == 6) break;
                            if (next.piece.colorIndex != color) break;

                            line.Add(next);
                        }
                    }

                    // 매치 완성
                    if (line.Count >= 3)
                    {
                        foreach (HexCell c in line)
                        {
                            // 중복 검사
                            if (matched.Add(c.offset))
                            {
                                results.Add(c.piece);

                                // 특수 구슬 검사 : 매치에 특수 구슬이 포함되어 있으면, 터뜨리기
                                if (c.piece.specialIndex >= 0 && !specialPieces.Contains(c.piece))
                                {
                                    specialPieces.Add(c.piece);
                                }
                            }
                        }
                    }
                }
            }
        }

        // 특수 구슬 추가 구현
        if (specialPieces.Count > 0)
        {
            foreach (Piece piece in specialPieces)
            {
                HexCell cell = piece.cell;
                Vector3Int start = cell.cube;

                List<HexCell> line = new List<HexCell>();
                line.Add(cell);

                switch (piece.specialIndex)
                {
                    case 0:
                        // 직선 축
                        int[] pair = piece.axisPair;

                        foreach (int dirIndex in pair)
                        {
                            Vector3Int dir = BoardUtil.cubeDirs[dirIndex]; // 방향
                            Vector3Int cur = start; // 현재 위치

                            // 축 순환
                            while (true)
                            {
                                // 다음 좌표가 맵 내에 있는지 확인
                                cur += dir;
                                Vector2Int offset = BoardUtil.CubeToEvenQ(cur);
                                if (!BoardUtil.InBounds(width, height, offset.x, offset.y)) break;

                                // 다음 셀로 이동
                                HexCell next = cells[offset.x, offset.y];

                                // 없는거 제외 전부 넣기
                                if (next == null || !next.isActive || next.piece == null) break;

                                line.Add(next);
                            }
                        }
                        break;
                }

                // 매치 완성
                foreach (HexCell c in line)
                {
                    // 중복 검사
                    if (matched.Add(c.offset))
                    {
                        results.Add(c.piece);
                    }
                }

                // 특수 구슬 이펙트
                foreach (Piece p in results)
                {
                    p.cell.SetHint(true);
                }
            }
        }
        return results;
    }
}