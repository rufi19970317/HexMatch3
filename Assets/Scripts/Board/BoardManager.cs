using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;

// 참고자료 : https://www.redblobgames.com/grids/hexagons/
// 육각형 그리드의 좌표계는 위 사이트를 참고하여 offset 및 cube 좌표계로 설정하였습니다.

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    GameObject HexBackGround;
    [SerializeField]
    GameObject piecePrefab;
    [SerializeField]
    List<Sprite> pieceSprites = new List<Sprite>();
    [SerializeField]
    List<Sprite> specialSprites0 = new List<Sprite>();

    HexCell[,] cells;
    int[,] paterns; // 레벨 패턴
    int width, height; // 보드 가로, 세로
    List<HexCell> jackBox = new List<HexCell>(); // 잭인더박스

    float cellSize; // 바깥쪽 원의 반지름

    int[] cellTop;
    int[] pieceIndexList;
    LevelData nowLevel;

    bool isGameActive = false;

    HintFinder hintFinder = new HintFinder();

    public void GameEnd()
    {
        isGameActive = false;
    }

    #region 보드 세팅
    // 레벨 세팅
    public void SetLevel(LevelData levelData)
    {
        nowLevel = levelData;
        InitBoard();
    }

    // 보드 세팅
    void InitBoard()
    {
        CalculateCellSize();

        width = nowLevel.width;
        height = nowLevel.height;

        pieceIndexList = new int[width];

        // 구슬 패턴 등록
        paterns = new int[nowLevel.width, nowLevel.colors.Length / nowLevel.width];
        for (int i = 0; i < nowLevel.colors.Length; i++)
        {
            paterns[i % nowLevel.width, i / nowLevel.width] = nowLevel.colors[i];
        }

        // 셀 생성
        cells = new HexCell[width, height];
        cellTop = new int[width];

        for (int i = 0; i < width; i++) cellTop[i] = height - 1;

        // 구슬 패턴을 맞추기 위해 height를 역순으로
        for (int r = height - 1; r >= 0; r--)
        {
            for (int q = 0; q < width; q++)
            {
                bool active = nowLevel.map[(r * width) + q] == 1;
                CreateHexCell(q, r, active);

                if (active)
                {
                    SpawnNewPiece(cells[q, r], true);

                    if (cellTop[q] > r)
                        cellTop[q] = r;
                }
            }
        }

        isGameActive = true;

        FindHint();
        hintCoroutine = StartCoroutine(MarkingHint());

    }

    // cellSize 구하기
    void CalculateCellSize()
    {
        SpriteRenderer sr = piecePrefab.GetComponent<SpriteRenderer>();
        Vector2 spriteSize = sr.sprite.bounds.size;
        cellSize = spriteSize.x / 2f;
    }

    // 보드판(셀) 생성
    void CreateHexCell(int q, int r, bool active)
    {
        HexCell cell = Instantiate(HexBackGround, transform).GetComponent<HexCell>();
        cells[q, r] = cell;
        cell.SetHexCell(q, r, active);
        cell.transform.position = GetWorldPosition(q, r);
        cell.SetActive(active);
    }

    // flat-top 좌표 변환 (offset : even-q 기준)
    public Vector2 GetWorldPosition(int q, int r)
    {
        // 좌상단을 (0, 0)으로 하는 flat-top + even-q + offset을 사용
        // 좌상단에서 시작하여, 우하단으로 내려가는 구조

        // 셀 하나의 가로, 세로 전체 크기 계산
        float w = 2 * cellSize;
        float h = Mathf.Sqrt(3) * cellSize;


        // 좌표 단순 위치 계산

        // flat-top은 가로 1칸(q값)마다 y값이 변동됨.
        // even - q의 경우, 홀수마다 y값이 상승.
        // q가 짝수 : (0, r)과 동일
        // q가 홀수 : (0, r) + (h / 2f)

        float x = q * (w * (3 / 4f)); // 가로 간격 = 가로 크기의 3/4
        float y = -r * h + ((q % 2 == 0) ? 0 : h / 2); // 세로 간격 = 세로 크기의 1/2


        // 보드 전체 크기 계산

        // boardWidth = ((가로 칸수 - 1) * 가로 간격) + (가로 크기) 
        // boardHeight = (세로 칸수 * 셀 세로 크기) + (셀 세로 크기) * 1/2

        float boardWidth = (width - 1) * (w * (3 / 4f)) + w;
        float boardHeight = height * h + (h / 2);


        // 보드 중심을 월드 좌표 (0,0)에 맞춤
        // 현재 위치는 제외해야 함.
        float diffX = boardWidth / 2 - (w / 2);
        float diffY = boardHeight / 2 - (h / 2);

        // 중심 기준으로 위치 조정
        x -= diffX;
        y += diffY;

        return new Vector2(x, y);
    }
    #endregion

    // 구슬 스폰
    void SpawnNewPiece(HexCell cell, bool isInit = false)
    {
        Piece p = GameManager.Instance.GetPiece();
        cell.SetPiece(p);

        // 다음 구슬 패턴 가져오기
        int x = cell.offset.x;
        int y = pieceIndexList[cell.offset.x]++;

        int color = paterns[x, y];

        p.SetColor(color, pieceSprites[color]);

        if (isInit)
        {
            // 고정 위치에 생성
            p.SetCell(cell);
            p.transform.position = cell.transform.position;

            if (color == 6) jackBox.Add(cell);
        }
        else
        {
            // 새 위치(최상단)에 생성
            p.SetCell(cell);
            p.transform.position = cells[cell.offset.x, cellTop[cell.offset.x]].transform.position + new Vector3(0, Mathf.Sqrt(3) * cellSize);
            p.EnqueueMove(cell.transform.position);
            p.MoveStart();
        }
    }

    bool isSpecialStart = false;
    public void StartSpecialGameClear(int move)
    {
        if (!isSpecialStart)
        {
            isSpecialStart = true;
            StartCoroutine(SpecialGameClear(move));
        }
    }

    // 특수 블럭
    // 남은 move 수만큼 랜덤 선택
    // 구슬 변경
    // FindMatch 수정 : 해당 구슬이 포함되어 있으면, 모든 매치 불 켜기
    // 구슬 파괴

    IEnumerator SpecialGameClear(int move)
    {
        List<HexCell> lastCell = new List<HexCell>();

        for (int q = 0; q < width; q++)
        {
            for (int r = 0; r < height; r++)
            {
                HexCell cell = cells[q, r];
                if (cell == null | !cell.isActive || cell.piece == null || cell.piece.colorIndex == 6) continue;
                lastCell.Add(cell);
            }
        }

        // 간단하게 중복없이 뽑기

        for(int i = 0; i < move; i++)
        {
            HexCell cell = lastCell[Random.Range(0, lastCell.Count)];
            lastCell.Remove(cell);

            // 축 방향으로 터트리는 특수 구슬로 변경
            int color = cell.piece.colorIndex;
            cell.piece.SetSpecial0(color, specialSprites0[color], BoardUtil.axisPairs[Random.Range(0, 3)]);
            cell.SetHint(true);
            cell.ActivePoint(3000);

            GameManager.Instance.MinusMove();

            yield return new WaitForSeconds(1f);
        }

        List<Piece> matches = MatchFinder.FindMatches(cells, true);
        yield return new WaitForSeconds(1f);

        yield return StartCoroutine(ProcessMatches(matches));

        yield return new WaitForSeconds(3f);
        GameManager.Instance.ActivateGameEndUI();
    }

    #region 힌트
    Coroutine hintCoroutine;

    // 힌트 생성
    void FindHint()
    {
        // 초기화
        if (hintFinder.hintMarks.Count > 0)
        {
            hintFinder.hintPiece.EndHintAnimation();
            foreach (HexCell cell in hintFinder.hintMarks)
            {
                cell.SetHint(false);
            }
            hintFinder.hintMarks.Clear();
        }

        hintFinder.FindHint(cells);
    }

    // 힌트 딜레이
    IEnumerator MarkingHint()
    {
        if (hintFinder.hintMarks.Count > 0 && isGameActive)
        {
            yield return new WaitForSeconds(2f);

            foreach (HexCell cell in hintFinder.hintMarks)
            {
                cell.SetHint(true);
            }

            hintFinder.hintPiece.SetHintAnimation(hintFinder.hintTarget);
        }
    }
    #endregion

    #region 구슬 스왑 및 매치 확인
    bool isStartSwap = false;

    // 자리 바꾸기
    public void OnSwap(HexCell a, HexCell b)
    {
        if (!isStartSwap && a.piece.colorIndex < 6 && b.piece.colorIndex < 6 && isGameActive)
            StartCoroutine(StartSwap(a, b));
    }

    IEnumerator StartSwap(HexCell a, HexCell b)
    {
        // 스왑 동안 힌트 끄기
        if(hintCoroutine != null)
            StopCoroutine(hintCoroutine);
        foreach(HexCell cell in hintFinder.hintMarks)
        {
            hintFinder.hintPiece.EndHintAnimation();
            cell.SetHint(false);
        }

        isStartSwap = true;

        // 실제 스왑
        SwapPieces(a, b);

        yield return new WaitForSeconds(0.3f); // 애니메이션 대기

        // 매치 검사
        List<Piece> matches = MatchFinder.FindMatches(cells);
        if (matches.Count > 0)
        {
            GameManager.Instance.MinusMove();
            yield return StartCoroutine(ProcessMatches(matches));

            // 새 힌트 찾기
            FindHint();
        }
        else
        {
            // 매치 없으면 원위치 복귀
            SwapPieces(b, a);
            yield return new WaitForSeconds(0.3f);
        }

        // 힌트 켜기
        hintCoroutine = StartCoroutine(MarkingHint());

        isStartSwap = false;
    }

    void SwapPieces(HexCell a, HexCell b)
    {
        // 애니메이션
        a.piece.MoveToSwap(b.transform.position);
        b.piece.MoveToSwap(a.transform.position);

        // 구슬 교환
        Piece temp = a.piece;
        a.SetPiece(b.piece);
        b.SetPiece(temp);

        a.piece.SetCell(a);
        b.piece.SetCell(b);
    }

    int combo = 0;
    IEnumerator ProcessMatches(List<Piece> matches)
    {
        // 1. 잭 확인
        ActivateJack(matches);

        // 2. 구슬 제거 및 포인트 띄우기
        int points = matches.Count * 20 * (combo == 0 ? 1 : combo);
        bool isActivePoint = false;
        foreach (Piece p in matches)
        {
            if (!isActivePoint)
            {
                isActivePoint = true;
                p.cell.ActivePoint(points);
            }

            p.cell.SetHint(false);
            if (p.colorIndex == 6) continue;
            p.SetActiveCrush();
            p.cell.SetPiece(null);
            GameManager.Instance.ReturnPiece(p);
        }

        yield return new WaitForSeconds(0.3f);

        // 3. 중력 적용
        
        ApplyGravity();

        // 4. 새로 채우기
        // 생성 칸이 비어있으면 생성
        float jackTime = 0.5f;
        bool isSpawn = true;
        while (isSpawn)
        {
            isSpawn = false;

            for (int q = 0; q < width; q++)
            {
                HexCell nowCell = cells[q, cellTop[q]];
                if (nowCell.piece == null)
                {
                    isSpawn = true;
                    SpawnNewPiece(nowCell);
                }
            }

            ApplyGravity();
            jackTime -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        if(jackTime > 0)
            yield return new WaitForSeconds(jackTime);

        // 5. 반복
        List<Piece> newMatches = MatchFinder.FindMatches(cells);
        if (newMatches.Count > 0)
        {
            combo++;
            yield return StartCoroutine(ProcessMatches(newMatches));
        }
        else
            combo = 0;
    }

    void ActivateJack(List<Piece> matches)
    {
        // 잭의 위치에서 이웃한지 확인
        foreach(HexCell cell in jackBox)
        {
            for(int i = 0; i < matches.Count; i++)
            {
                // 있으면 활성화
                if (BoardUtil.AreNeighbors(cell, matches[i].cell))
                {
                    cell.piece.SetActiveJack();
                    GameManager.Instance.AddJackGoal();
                    break;
                }
            }
        }
    }

    /*
    단순 중력 방식 : 위에서 아래로

    void ApplyGravity()
    {
        for (int q = 0; q < width; q++)
        {
            for (int r = height - 1; r >= 0; r--)
            {
                HexCell cell = cells[q, r];
                if (!cell.isActive) continue;

                // 빈 칸이면 위쪽 구슬 내리기

                if (cell.piece == null)
                {
                    for (int upper = r - 1; upper >= 0; upper--)
                    {
                        HexCell upperCell = cells[q, upper];
                        if (upperCell.isActive && upperCell.piece != null)
                        {
                            // 이동
                            cell.SetPiece(upperCell.piece);
                            cell.piece.SetCell(cell);
                            cell.piece.MoveTo(cell.transform.position);
                            upperCell.SetPiece(null);
                            break;
                        }
                    }
                }
            }
        }
    }
    */

    // 중력 적용 : 길찾기 방식
    void ApplyGravity()
    {
        bool isMove = false; // 움직인게 있으면 반복

        for(int q = 0; q < width; q++)
        {
            // 좌하단 부터 시작
            for(int r = height - 1; r > 0; r--)
            {
                HexCell cell = cells[q, r];
                if (!cell.isActive) continue;
                if (cellTop[q] == r) continue; // 구슬 나오는 부분이면 패스

                // 구슬이 비었을 경우, 탐색
                if (cell.piece == null)
                {
                    Vector3Int[] dirs = new Vector3Int[3] { BoardUtil.cubeDirs[5], BoardUtil.cubeDirs[0], BoardUtil.cubeDirs[4] };

                    for(int i = 0; i < 3; i++)
                    {
                        // 상단 > 우상단 > 좌상단 순으로 확인
                        bool isFinish = false;

                        Vector2Int nowOffset = BoardUtil.CubeToEvenQ(cell.cube + dirs[i]);

                        if (!BoardUtil.InBounds(width, height, nowOffset.x, nowOffset.y))
                            continue;

                        // 길찾기
                        Stack<Vector3> moveQueue = new Stack<Vector3>();
                        moveQueue.Push(cell.transform.position);

                        for (int upper = nowOffset.y; upper >= 0; upper--)
                        {
                            HexCell upperCell = cells[nowOffset.x, upper];
                            if (!upperCell.isActive) break;

                            if (upperCell.piece != null)
                            {
                                if (upperCell.piece.colorIndex == 6) break; // 상자면 x

                                // 구슬이 존재하면 이동
                                cell.SetPiece(upperCell.piece);
                                cell.piece.SetCell(cell);

                                while(moveQueue.Count > 0)
                                {
                                    cell.piece.EnqueueMove(moveQueue.Pop());
                                }
                                cell.piece.MoveStart();
                                upperCell.SetPiece(null);

                                isMove = true;
                                isFinish = true;
                                break;
                            }

                            // 구슬이 나오는 부분에 다다랐을 경우, 다음 방향 탐색은 패스
                            if (upper == cellTop[nowOffset.x])
                            {
                                isFinish = true;
                                break;
                            }

                            moveQueue.Push(upperCell.transform.position);
                        }

                        if (isFinish) break;
                    }
                }
            }
        }

        if (isMove)
            ApplyGravity();
    }
    #endregion
}
