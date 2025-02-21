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

    public Vector3 origPropScale;

    public Vector3 previousPos;

    public BoxCollider propCollider;
    public Material material;
    public Vector3 propSize;

    public int propLayer = 0;
    public ShelfGrid shelfGrid;

    private bool propState = false;
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
        if (state)
        {
            material.color = Color.white;
        }
        else
        {
            ColorUtility.TryParseHtmlString("#5B5B5B", out Color disabledColor);

            material.color = disabledColor;

           // Debug.Log("Setting disabled color for : " + gameObject.name);
        }

        propState = state;
    }

    public void OnPicked()
    {
        if (!propState || isPicked) return;

        previousPos = transform.position;

        isPicked = true;
        BoosterManager.previousPickedProp = this;

        GameManager.instance.slotManager.EnqueueProp(this, OnPropQueueComplete);
        transform.localScale = transform.localScale / 1.25f;

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
        isPicked = false;
        transform.parent = null;

        //transform.SetPositionAndRotation(origPropPos, Quaternion.identity);
        Tween moveTween = SetPositionTween(previousPos);
        transform.rotation = Quaternion.identity;
        transform.localScale = origPropScale;

        GameManager.instance.slotManager.ResetSlot(this);
        
        moveTween.OnComplete(() => StartCoroutine(shelfGrid.OnPropUndo(this)));
    }

    public Tween SetPositionTween(Vector3 pos)
    {
        previousPos = pos;
        //transform.position = pos;

        Tween moveTween = transform.DOMove(pos, 0.15f).SetEase(Ease.InQuad);

        return moveTween;
    }

    public Tween SetSpawnTween(Vector3 pos)
    {
        previousPos = pos;
        
        Tween spawnPunchTween = transform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 0, 0).SetEase(Ease.InQuad);

        return spawnPunchTween;
    }
}
