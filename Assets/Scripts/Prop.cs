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
    [HideInInspector] public Material material;
    [HideInInspector] public Vector3 propSize;

    public int propLayer = 0;
    [HideInInspector] public ShelfGrid shelfGrid;

    private bool propState = true;
    private bool isPicked = false;

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

        material = meshRenderer.material;
        propSize = Vector3.Scale(propCollider.size, transform.localScale);

        origPropScale = transform.localScale;
    }

    public void SetPropState(bool state)
    {
        Color targetColor = state ? Color.white : ColorUtility.TryParseHtmlString("#5B5B5B", out Color disabledColor) ? disabledColor : Color.gray;

        material.DOColor(targetColor, 0.25f);

        propState = state;
    }

    public void OnPicked()
    {
        if (!propState || isPicked) return;

        previousPos = transform.position;

        isPicked = true;
        BoosterManager.previousPickedProp = this;

        GameManager.instance.slotManager.EnqueueProp(this, OnPropQueueComplete);
        //transform.localScale = transform.localScale / 1.25f;

        if (propPickedScale == 1)
        {
            transform.DOScale(transform.localScale / 2, 0.25f).SetEase(Ease.OutQuad);
        }
        else
        {
            transform.DOScale(Vector3.one * propPickedScale, 0.25f).SetEase(Ease.OutQuad);
        }

        shelfGrid.shelfPropList[propLayer].Remove(this);

        SFXManager.instance.PlayPropPickedSound();

        Debug.Log(gameObject.name + " is picked!!");
    }

    private void OnPropQueueComplete()
    {
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
        
        moveTween.OnComplete(() => StartCoroutine(shelfGrid.OnPropUndo(this)));
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

        Tween moveTween = transform.DOMove(pos, 0.15f).SetEase(Ease.InQuad);

        return moveTween;
    }

    public Tween PlaySpawnTween()
    {
        Tween spawnPunchTween = transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 0, 0).SetEase(Ease.InQuad);

        return spawnPunchTween;
    }
}
