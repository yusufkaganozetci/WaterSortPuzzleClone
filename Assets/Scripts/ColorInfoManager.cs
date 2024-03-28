using System;
using UnityEngine;

public enum LiquidType
{
    Red,
    Yellow,
    Blue,
    Pink,
}

[Serializable]
public class ColorInfo
{
    public LiquidType type;

    [ColorUsage(true, true)]
    public Color mainColor;

    [ColorUsage(true, true)]
    public Color topColor;
}

public class ColorInfoManager : MonoBehaviour
{
    public ColorInfo[] colorInfo;

    private void Start()
    {
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        EventManager.Instance.SubscribeToEvent(EventType.MainColorByTypeRequested,
            GetMainColorByType);
        EventManager.Instance.SubscribeToEvent(EventType.TopColorByTypeRequested,
            GetTopColorByType);
    }

    public object GetMainColorByType(object type)
    {
        Color color = Color.white;
        for (int i = 0; i < colorInfo.Length; i++)
        {
            if (colorInfo[i].type == (LiquidType) type)
            {
                color = colorInfo[i].mainColor;
            }
        }
        return color;
    }

    public object GetTopColorByType(object type)
    {
        Color color = Color.white;
        for (int i = 0; i < colorInfo.Length; i++)
        {
            if (colorInfo[i].type == (LiquidType) type)
            {
                color = colorInfo[i].topColor;
            }
        }
        return color;
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        EventManager.Instance.UnsubscribeFromEvent(EventType.MainColorByTypeRequested,
            GetMainColorByType);
        EventManager.Instance.UnsubscribeFromEvent(EventType.TopColorByTypeRequested,
            GetTopColorByType);
    }

}