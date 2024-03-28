using UnityEngine;

public class BottleManager : MonoBehaviour
{
    public bool isPouring = false;

    [SerializeField] Bottle[] bottles;
    [SerializeField] int maxLiquidLevel = 4;

    private Bottle chosenBottle = null;
    private Bottle targetBottle = null;

    private void Start()
    {
        SubscribeToEvents();
    }

    private void SubscribeToEvents()
    {
        EventManager.Instance.SubscribeToEvent(EventType.BottlePickRequested,
            PickBottle);
        EventManager.Instance.SubscribeToEvent(EventType.PouringCompleted,
            OnPouringCompleted);
        EventManager.Instance.SubscribeToEvent(EventType.Reset, ResetObj);
    }

    public void PickBottle(object bottleTransform)
    {
        if (isPouring) return;
        Bottle bottle = GetBottleByTransform(bottleTransform as Transform);
        if (chosenBottle == null)
        {
            chosenBottle = bottle;
            chosenBottle.PickBottle();
        }
        else if (chosenBottle == bottle) 
        {
            UnpickBottle();
            return;
        }
        else targetBottle = bottle;
        
        StartPouring();
    }

    private void StartPouring()
    {
        if(chosenBottle != null && targetBottle != null)
        {
            PourInfo pourInfo = CalculatePourInfo();
            if (pourInfo != null && pourInfo.level != 0) 
            {
                isPouring = true;
                chosenBottle.Pour(pourInfo); 
            }
        }
    }

    private PourInfo CalculatePourInfo()
    {
        if(targetBottle.currentLiquids.Count >= 4 || 
            chosenBottle.currentLiquids.Count <= 0)
        {
            return null;
        }
        int level = CalculateLiquidLevel();
        Direction direction = CalculateDirection();
        return new PourInfo(chosenBottle, targetBottle, level, chosenBottle.currentLiquids[chosenBottle.currentLiquids.Count - 1].type, direction);
    }

    private Direction CalculateDirection()
    {
        Direction direction;
        if (chosenBottle.transform.position.x >=
            targetBottle.transform.position.x)
        {
            direction = Direction.Right;
        }
        else
        {
            direction = Direction.Left;
        }
        return direction;
    }

    private int CalculateLiquidLevel()
    {
        int level = 0;
        int chosenBottleLiquidLevelCount = chosenBottle.currentLiquids.Count;
        int targetBottleLiquidLevelCount = targetBottle.currentLiquids.Count;
        if (targetBottleLiquidLevelCount == 0)
        {
            LiquidType lastType = chosenBottle.currentLiquids[chosenBottleLiquidLevelCount - 1].type;
            for (int i = chosenBottleLiquidLevelCount - 1;
                i >= 0; i--)
            {
                if (lastType == chosenBottle.currentLiquids[i].type)
                {
                    level++;
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            if (targetBottle.currentLiquids[targetBottleLiquidLevelCount - 1].type
            == chosenBottle.currentLiquids[chosenBottleLiquidLevelCount - 1].type)
            {
                int freeSpace = maxLiquidLevel - targetBottleLiquidLevelCount;
                for (int i = chosenBottleLiquidLevelCount - 1; i >= 0; i--)
                {
                    if (freeSpace <= 0)
                    {
                        break;
                    }
                    if (chosenBottle.currentLiquids[i].type ==
                        targetBottle.currentLiquids[targetBottleLiquidLevelCount - 1].type)
                    {
                        level++;
                        freeSpace--;
                    }
                    else { break; }
                }
            }
        }
        return level;
    }

    public void OnPouringCompleted()
    {
        isPouring = false;
        ResetBottle();
    }

    private void ResetBottle()
    {
        if (isPouring) return;
        chosenBottle?.ResetBottlePosition();
        ResetBottleInfos();
    }

    private void UnpickBottle()
    {
        if (isPouring) return;
        chosenBottle?.UnpickBottle();
        ResetBottleInfos();
    }

    private void ResetBottleInfos()
    {
        chosenBottle = null;
        targetBottle = null;
    }

    private Bottle GetBottleByTransform(Transform bottleTransform)
    {
        Bottle bottle = null;
        for(int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i].transform == bottleTransform) 
            { 
                bottle = bottles[i];
            }
        }
        return bottle;
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void UnsubscribeFromEvents()
    {
        EventManager.Instance.UnsubscribeFromEvent(EventType.BottlePickRequested,
            PickBottle);
        EventManager.Instance.UnsubscribeFromEvent(EventType.PouringCompleted,
            OnPouringCompleted);
        EventManager.Instance.UnsubscribeFromEvent(EventType.Reset,
            ResetObj);
    }

    private void ResetObj()
    {
        ResetBottleInfos();
    }

}