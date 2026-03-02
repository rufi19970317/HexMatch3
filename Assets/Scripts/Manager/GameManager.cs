using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField]
    BoardManager boardManager;
    [SerializeField]
    PoolManager poolManager;
    [SerializeField]
    UIManager uiManager;

    LevelData nowLevel;

    bool isEnd = false;

    int goal;
    int move;

    int nowGoal = 0;
    int uiGoal = 0;

    [SerializeField]
    string levelName = "Level1";

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        // 프레임 고정
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        // 모바일 화면 꺼짐 방지
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    void Start()
    {
        InitGame();
    }

    void InitGame()
    {
        nowLevel = LevelLoader.LoadLevel(levelName);

        goal = nowLevel.goal;
        move = nowLevel.move;
        nowGoal = goal;
        uiGoal = goal;
        nowPoint = 0;

        uiManager.InitUI(goal, move);

        poolManager.InitPool(nowLevel);
        boardManager.SetLevel(nowLevel);
    }

    public LevelData GetLevelData()
    {
        return nowLevel;
    }

    public Piece GetPiece()
    {
        return poolManager.GetPiece();
    }

    public void ReturnPiece(Piece piece)
    {
        poolManager.RetrunPiece(piece);
    }

    public void MinusMove()
    {
        if(move > 0)
            move--;

        uiManager.UpdateMove(move);
    }

    void GameOver()
    {
        isEnd = true;
    }

    public void AddJackGoal()
    {
        if (isEnd) return;

        if(nowGoal > 0)
            nowGoal--;

        if (nowGoal <= 0)
        {
            GameClear();
        }
    }

    public void UpdateGoal()
    {
        if (uiGoal > 0)
        {
            uiGoal--;
            uiManager.UpdateGoal(uiGoal);
        }

        if(uiGoal <= 0)
            boardManager.StartSpecialGameClear(move);
    }

    int nowPoint = 0;

    public void AddPoint(int point)
    {
        nowPoint += point;
        uiManager.UpdatePoint(nowPoint);
    }

    void GameClear()
    {
        boardManager.GameEnd();
        isEnd = true;
    }

    public Transform GetGoalPos()
    {
        return uiManager.GetGoalPos();
    }

    public void ActivateGameEndUI()
    {
        uiManager.ActivateGameEndUI();
    }

    public void RestartScene()
    {
        DOTween.KillAll(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadSceneLevel1()
    {
        DOTween.KillAll(false);
        SceneManager.LoadScene("GameScene");
    }

    public void LoadSceneDebug()
    {
        DOTween.KillAll(false);
        SceneManager.LoadScene("GameSceneDebug");
    }
}
