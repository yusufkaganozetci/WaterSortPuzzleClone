using DG.Tweening;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    [SerializeField] BottleManager bottleManager;

    public void ResetGame()
    {
        StartCoroutine(ResetGameCoroutine());
    }

    private IEnumerator ResetGameCoroutine()
    {
        yield return new WaitWhile(() => bottleManager.isPouring);
        ResetTweens();
        EventManager.Instance.TriggerActionEvent(EventType.Reset);
    }

    private void ResetTweens()
    {
        DOTween.KillAll();
    }

}