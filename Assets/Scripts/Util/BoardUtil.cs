using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class BoardUtil
{
    // 큐브 육각형 방향 : 하나는 1, 나머지 둘 중 하나가 -1
    public static readonly Vector3Int[] cubeDirs = new Vector3Int[]
    {
        new Vector3Int( 1,-1, 0), // 우상단 방향
        new Vector3Int( 1, 0,-1), // 우하단 방향
        new Vector3Int( 0, 1,-1), // 하단 방향
        new Vector3Int(-1, 1, 0), // 좌하단 방향
        new Vector3Int(-1, 0, 1), // 좌상단 방향
        new Vector3Int( 0,-1, 1)  // 상단 방향
    };


    public static readonly int[][] axisPairs = new int[][]
    {
        new int[]{0, 3}, // 우상단-좌하단
        new int[]{1, 4}, // 우하단-좌상단
        new int[]{2, 5}, // 하단-상단
    };

    // 이웃 판별 : 두 좌표의 차가 육각형 방향과 동일
    public static bool AreNeighbors(HexCell a, HexCell b)
    {
        Vector3Int diff = a.cube - b.cube;

        for (int i = 0; i < cubeDirs.Length; i++)
        {
            if (cubeDirs[i] == diff)
                return true;
        }

        return false;
    }

    // offset > 큐브 좌표 변환
    public static Vector3Int EvenQToCube(int q, int r)
    {
        /*
         x + y + z = 0
         x = 상단 = q (가로는 동일)
         y = 우하단 = r - (q + (q % 2)) / 2
              >> 높이 보정, (좌상단 > 우하단) 대각선을 향해 0을 만드는 과정
         z = 좌하단 = -(x + y)
              >> z와 똑같이 할 수 있지만, 쉽게 x + y + z = 0 임을 이용
        */

        int x = q;
        int y = r - (q + (q % 2)) / 2; // 정확도를 위해
        int z = -(x + y);
        return new Vector3Int(x, y, z);
    }

    // 큐브 > offset 좌표 변환
    public static Vector2Int CubeToEvenQ(Vector3Int cube)
    {
        // x, y 값 구하기 역으로
        int q = cube.x;
        int r = cube.y + (cube.x + (cube.x % 2)) / 2; // 현재 cube.x는 무조건 양수이기에, 절대값 처리 x
        return new Vector2Int(q, r);
    }

    public static bool InBounds(int width, int height, int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height);
    }
}
