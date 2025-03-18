using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class Prop : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    [HideInInspector] public Vector3 origPropScale;
    public float propPickedScale = 1;

    [HideInInspector] public Vector3 previousPos;

    [HideInInspector] public BoxCollider propCollider;
    [HideInInspector] public Material[] materials;
    [HideInInspector] public Vector3 propSize;

    public int propLayer = 0;
    [HideInInspector] public ShelfGrid shelfGrid;
    
    [HideInInspector] public Tween scaleTween;

    public bool propState = true;
    public bool isPicked = false;

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        propCollider = GetComponent<BoxCollider>();

        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        if (propCollider == null)
        {
            propCollider = GetComponentInChildren<BoxCollider>();
        }

        materials = meshRenderer.materials;
        propSize = Vector3.Scale(propCollider.size, transform.lossyScale);

        origPropScale = transform.localScale;
    }

    public void SetPropState(bool state)
    {
        Color targetColor = state ? Color.white : ColorUtility.TryParseHtmlString("#5B5B5B", out Color disabledColor) ? disabledColor : Color.gray;

        foreach (var material in materials)
        {
            material.DOColor(targetColor, 0.25f);
        }

        propState = state;
    }

    public void ResetProp()
    {
        transform.position = GameManager.instance.propManager.propsSpawnPoint.position;
        transform.localScale = origPropScale;

        propLayer = 0;
        SetPropState(true);
    }

    public void OnPicked()
    {
        if (!propState || isPicked) return;

        previousPos = transform.position;

        isPicked = true;
        BoosterManager.previousPickedProp = this;
        
        //ScalePropToSlot();

        if (propPickedScale < 1)
        {
            scaleTween = transform.DOScale(Vector3.one * propPickedScale, 0.25f).SetEase(Ease.OutQuad);
        }
        else
        {
            scaleTween = transform.DOScale(transform.localScale / 1.75f, 0.25f).SetEase(Ease.OutQuad);
        }
        
        GameManager.instance.slotManager.EnqueueProp(this, OnPropQueueComplete);


        SFXManager.instance.PlayPropPickedSound();
        
        GameManager.instance.OnPropPicked.Invoke();

        //Debug.Log(gameObject.name + " is picked!!");
    }

    private void ScalePropToSlot()
    {
        float slotWidth = 0.15f;
        float propWidth = propSize.x;
        
        float scaleFactor = propWidth / slotWidth;
        
        Vector3 targetScale = Vector3.one * scaleFactor;
        
        scaleTween = transform.DOScale(targetScale, 0.25f).SetEase(Ease.OutQuad);
    }

    private void OnPropQueueComplete()
    {
        shelfGrid.shelfPropList[propLayer].Remove(this);
        StartCoroutine(shelfGrid.UpdateShelf(this));
    }

    public void PropUndo()
    {
        Sequence undoSeq = DOTween.Sequence();

        isPicked = false;
        transform.parent = null;

        //transform.SetPositionAndRotation(origPropPos, Quaternion.identity);
        Tween moveTween = SetPositionTween(previousPos);
        transform.rotation = Quaternion.identity;

        Tween scaleTween = transform.DOScale(origPropScale, 0.15f).SetEase(Ease.InQuad);

        undoSeq.Append(moveTween);
        undoSeq.Join(scaleTween);

        GameManager.instance.slotManager.ResetSlot(this);

        undoSeq.OnComplete(() => StartCoroutine(shelfGrid.OnPropUndo(this)));
    }

    public void SetPosition(Vector3 pos)
    {
        previousPos = pos;
        transform.position = pos;
    }

    public Tween SetPositionTween(Vector3 pos)
    {
        previousPos = pos;
        //transform.position = pos;

        Tween moveTween = transform.DOMove(pos, 0.25f).SetEase(Ease.InQuad);

        return moveTween;
    }

    public Tween PlaySpawnTween()
    {
        float targetScale = 1.2f;
        Vector3 scaleDifference = transform.localScale * targetScale - transform.localScale;
        
        Tween spawnPunchTween = transform.DOPunchScale(scaleDifference, 0.1f, 0, 0).SetEase(Ease.InQuad);

        return spawnPunchTween;
    }
}
