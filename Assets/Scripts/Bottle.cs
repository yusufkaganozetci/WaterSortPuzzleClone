using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[Serializable]
public class Liquid
{
    public LiquidType type;

    public Liquid(LiquidType type)
    {
        this.type = type;
    }
}

public class Bottle : MonoBehaviour
{
    public List<Liquid> initialLiquids;
    public List<Liquid> currentLiquids = new List<Liquid>();

    [Header("References")]
    [SerializeField] Renderer liquidRenderer;
    [SerializeField] LineRenderer fillEffect;
    [SerializeField] Transform leftBottleMouth;
    [SerializeField] Transform rightBottleMouth;

    [Header("Bottle Properties")]
    [SerializeField] float chosenBottleYDifference;
    [SerializeField] float pouringXPositionDiff;
    [SerializeField] float pouringYPositionDiff;
    [SerializeField] float pickBottleMovementDuration;

    [Header("Bottle Pouring Properties")]
    [SerializeField] float pouringMovementDuration;
    [SerializeField] float pouringRotationDuration;
    [SerializeField] float pouringDurationPerLevel;

    [Header("Shader Properties")]
    [SerializeField] string[] materialColorStrings;
    [SerializeField] string mainColorString;
    [SerializeField] string topColorString;
    [SerializeField] string fillAmountString;
    [SerializeField] string scaleAndRotationString;

    private Material liquidMaterial;
    private Material fillEffectMaterial;

    private Transform bottleMouth;
    private PourInfo pourInfo;
    private Vector3 startPosition;
    private float currentZRot = 0;

    private bool isPouring = false;
    private bool isFilling = false;

    void Start()
    {
        SubscribeToEvents();
        Initialize();
    }

    private void SubscribeToEvents()
    {
        EventManager.Instance.SubscribeToEvent(EventType.FillBottle, Fill);
        EventManager.Instance.SubscribeToEvent(EventType.Reset, ResetObj);
    }

    private void Initialize()
    {
        startPosition = transform.position;
        liquidMaterial = liquidRenderer.material;
        fillEffectMaterial = fillEffect.material;
        currentLiquids = initialLiquids.ToList();
        liquidMaterial.SetFloat(fillAmountString, initialLiquids.Count * 0.2f);
        UpdateLiquid();
    }

    private void UpdateLiquid()
    {
        for (int i = 0; i < currentLiquids.Count; i++)
        {
            liquidMaterial.SetColor(materialColorStrings[i],
                (Color) EventManager.Instance.TriggerFuncEvent(EventType.MainColorByTypeRequested, currentLiquids[i].type));                /*ColorInfoManager.Instance.GetMainColorByType(currentLiquids[i].type));*/
            if (i == currentLiquids.Count - 1) liquidMaterial.SetColor(topColorString,
                (Color) EventManager.Instance.TriggerFuncEvent(EventType.TopColorByTypeRequested, currentLiquids[i].type));
        }
    }

    public void PickBottle()
    {
        transform.DOMoveY(chosenBottleYDifference, pickBottleMovementDuration);
    }

    public void UnpickBottle()
    {
        transform.DOMove(startPosition, pickBottleMovementDuration);
    }

    public void ResetBottlePosition()
    {
        transform.DOMove(startPosition, pouringMovementDuration);
    }

    void Update()
    {
        if (isPouring)
        {
            UpdateScaleValue();
        }
        if (isFilling)
        {
            UpdateFillEffectPositions();
        }
    }

    private void UpdateScaleValue()
    {
        currentZRot = transform.eulerAngles.z;
        float zRotationRad = currentZRot * (float)Math.PI / 180;
        float multiplier = Math.Max(0.5f, (float)Math.Cos(zRotationRad));
        liquidMaterial.SetFloat(scaleAndRotationString, multiplier);
    }

    public void Pour(PourInfo pourInfo)
    {
        int level = pourInfo.level;
        if (level > currentLiquids.Count) return;

        this.pourInfo = pourInfo;
        
        Bottle targetBottle = pourInfo.targetBottle;
        LiquidType type = pourInfo.liquidType;
        Direction direction = pourInfo.direction;
        Vector3 targetRotation = transform.eulerAngles;
        Vector3 moveVector = targetBottle.transform.position + new Vector3(pouringXPositionDiff, pouringYPositionDiff, 0);
        float startZRotation = (EventManager.Instance.TriggerFuncEvent(EventType.LiquidLevelInfoRequested, currentLiquids.Count - 1) as LiquidLevelInfo).startRotationForPouring;
        float endZRotation = (EventManager.Instance.TriggerFuncEvent(EventType.LiquidLevelInfoRequested, currentLiquids.Count - level) as LiquidLevelInfo).endRotationForPouring;
        
        UpdatePouringValuesByDirection(direction, ref moveVector,
            ref startZRotation, ref endZRotation);

        targetRotation.z = startZRotation;
        Vector3 endRotation = transform.eulerAngles + new Vector3(0, 0, endZRotation);
        
        Sequence sq = DOTween.Sequence();
        sq
            .Append(transform.DOMove(moveVector, pouringMovementDuration))
            .AppendCallback(() => isPouring = true)
            .Append(transform.DORotate(targetRotation, pouringRotationDuration))
            .OnStart(() => EventManager.Instance.TriggerActionEvent(EventType.FillBottle,
            this.pourInfo, level, type, pouringMovementDuration + pouringRotationDuration))
            .Append(transform.DORotate(endRotation, pouringDurationPerLevel * level))
            .Join(liquidMaterial.DOFloat(1f, scaleAndRotationString, pouringDurationPerLevel * level))
            .Join(liquidMaterial.DOFloat((currentLiquids.Count - level) * 0.2f, fillAmountString, pouringDurationPerLevel * level))
            .OnComplete(() => OnPouringCompleted(level, Vector3.zero));
    }

    private void UpdatePouringValuesByDirection(Direction direction, ref Vector3 moveVector, ref float startZRotation, ref float endZRotation)
    {
        if (direction == Direction.Right)
        {
            pourInfo.chosenBottle.bottleMouth = leftBottleMouth;
            startZRotation *= -1;
            endZRotation *= -1;
        }
        else//left direction
        {
            pourInfo.chosenBottle.bottleMouth = rightBottleMouth;
            moveVector.x -= 2 * pouringXPositionDiff;
        }
    }

    private void ShowFillEffect(LiquidType type)
    {
        isFilling = true;
        fillEffectMaterial.SetColor(mainColorString,
            (Color) EventManager.Instance.TriggerFuncEvent(EventType.MainColorByTypeRequested, type));
        UpdateFillEffectPositions();
        fillEffect.gameObject.SetActive(true);
    }

    private void UpdateFillEffectPositions()
    {
        if(pourInfo == null || pourInfo.chosenBottle == null) return;
        Vector3 fillEffect0Pos = fillEffect.GetPosition(0);
        Vector3 fillEffect1Pos = pourInfo.chosenBottle.bottleMouth.position;
        fillEffect0Pos.x = pourInfo.chosenBottle.bottleMouth.position.x;
        fillEffect1Pos.z = 0;
        fillEffect.SetPosition(0, fillEffect0Pos);
        fillEffect.SetPosition(1, fillEffect1Pos);
    }

    private void Fill(object pourInfoAsObj,object level, object type, object delay)
    {
        pourInfo = pourInfoAsObj as PourInfo;
        Bottle bottle = pourInfo.targetBottle;
        if (bottle != this) return;
        StartCoroutine(bottle.FillCoroutine((int)level, (LiquidType)type, (float)delay, pourInfo.chosenBottle.bottleMouth.position));
    }

    public IEnumerator FillCoroutine( int level, LiquidType type, float delay, Vector3 effectPos)
    {
        yield return new WaitForSeconds(delay);
        
        if (level + currentLiquids.Count > 4) 
        {
            yield break;
        }
        for(int i = 0; i < level; i++)
        {
            currentLiquids.Add(new Liquid(type));
        }
        UpdateLiquid();
        ShowFillEffect(type);
        liquidMaterial.DOFloat(currentLiquids.Count * 0.2f,
            fillAmountString, pouringDurationPerLevel * level)
            .OnComplete(()=>isFilling = false);
    }

    private void OnPouringCompleted(int pouredLevelCount, Vector3 pouringCompletedRotation)
    {
        isPouring = false;
        pourInfo.targetBottle.fillEffect.gameObject.SetActive(false);
        currentLiquids.RemoveRange(currentLiquids.Count - pouredLevelCount, pouredLevelCount);
        transform.DORotate(pouringCompletedRotation, pouringRotationDuration);
        
        if (currentLiquids.Count > 0)
            liquidMaterial.SetColor(topColorString,
            (Color) EventManager.Instance.TriggerFuncEvent(EventType.TopColorByTypeRequested,
            currentLiquids[currentLiquids.Count - 1].type));
        else
            liquidMaterial.SetFloat(fillAmountString, -0.1f);
        EventManager.Instance.TriggerActionEvent(EventType.PouringCompleted);
    }

    private void UnsubscribeFromEvents()
    {
        EventManager.Instance.UnsubscribeFromEvent(EventType.FillBottle, Fill);
        EventManager.Instance.UnsubscribeFromEvent(EventType.Reset, ResetObj);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void ResetObj()
    {
        isPouring = false;
        isFilling = false;
        pourInfo = null;
        bottleMouth = null;
        currentLiquids.Clear();
        fillEffect.gameObject.SetActive(false);
        transform.position = startPosition;
        transform.eulerAngles = Vector3.zero;
        Initialize();
    }

}