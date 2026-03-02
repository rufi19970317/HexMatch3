using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateJackGoal : MonoBehaviour
{
    [SerializeField]
    Transform createPos;
    [SerializeField]
    GameObject jackGoal;

    void OnDisable()
    {
        if (!gameObject.scene.isLoaded)
            return;

        JackGoalAnimation go = Instantiate(jackGoal).GetComponentInChildren<JackGoalAnimation>();
        go.PlayAnimation(createPos.position);
    }
}
