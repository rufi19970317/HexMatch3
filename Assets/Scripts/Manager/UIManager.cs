using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    Transform goalPos;

    [SerializeField]
    TMP_Text goalText;
    [SerializeField]
    TMP_Text moveText;
    [SerializeField]
    TMP_Text pointText;

    [SerializeField]
    GameObject clearMark;
    [SerializeField]
    GameObject clearText;
    [SerializeField]
    GameObject gameEnd;


    public void InitUI(int goal, int move)
    {
        goalText.text = goal.ToString();
        moveText.text = move.ToString();
        pointText.text = "0";
    }

    public void UpdateGoal(int goal)
    {
        goalText.text = goal.ToString();
        if (goal <= 0)
        {
            goalText.gameObject.SetActive(false);
            clearMark.SetActive(true);
            clearText.SetActive(true);
            StartCoroutine(OffClearText());
        }
    }

    IEnumerator OffClearText()
    {
        yield return new WaitForSeconds(2f);
        clearText.SetActive(false);
    }

    public void UpdateMove(int move)
    {
        moveText.text = move.ToString();
    }

    public void UpdatePoint(int point)
    {
        pointText.text = point.ToString();
    }

    public Transform GetGoalPos()
    {
        return goalPos;
    }

    public void ActivateGameEndUI()
    {
        gameEnd.SetActive(true);
    }
}