using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackGoalAnimation : MonoBehaviour
{
    public void PlayAnimation(Vector3 startPos)
    {
        transform.position = startPos;
        Vector3 midPos = transform.position + new Vector3(0, -1f, 0);
        
        // ui 좌표 변환
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, GameManager.Instance.GetGoalPos().position);
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        Vector3 endPos = worldPos;

        Vector3[] path = new Vector3[] { midPos, endPos };

        // 이동 애니메이션
        var moveTween = transform.DOPath(path, 2f, PathType.CatmullRom)
            .SetEase(Ease.InOutSine);
        
        Vector3 targetScale = GameManager.Instance.GetGoalPos().localScale;

        // 크기 애니메이션
        var scaleTween = transform.DOScale(targetScale, 2f);

        DOTween.Sequence()
            .Append(moveTween)
            .Join(scaleTween)
            .OnComplete(() =>
            {
                GameManager.Instance.UpdateGoal();
                Destroy(gameObject);
            });
    }
}
