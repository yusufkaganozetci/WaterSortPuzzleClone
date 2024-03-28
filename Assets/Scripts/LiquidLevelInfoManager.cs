using System;
using UnityEngine;

[Serializable]
public class LiquidLevelInfo
{
    public int index;//0 means bottom
    public float startRotationForPouring;
    public float endRotationForPouring;
    public float fillAmount;
}

public class LiquidLevelInfoManager : MonoBehaviour
{
    public LiquidLevelInfo[] liquidLevelInfos;

    private void Start()
    {
        EventManager.Instance.SubscribeToEvent(EventType.LiquidLevelInfoRequested,
            GetLiquidLevelInfoByIndex);
    }

    public object GetLiquidLevelInfoByIndex(object indexAsObj)
    {
        LiquidLevelInfo liquidLevelInfo = null;
        int index = (int) indexAsObj;
        for(int i = 0; i < liquidLevelInfos.Length; i++)
        {
            if (liquidLevelInfos[i].index == index)
            {
                liquidLevelInfo = liquidLevelInfos[i];
            }
        }
        return liquidLevelInfo;
    }

    private void OnDestroy()
    {
        EventManager.Instance.UnsubscribeFromEvent(EventType.LiquidLevelInfoRequested,
            GetLiquidLevelInfoByIndex);
    }

}