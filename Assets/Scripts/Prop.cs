using DG.Tweening;
using System;
using UnityEngine;

[Serializable]
public class Prop : MonoBehaviour
{
    private MeshRenderer meshRenderer;

    public Vector3 origPropPos;
    public Vector3 origPropScale;

    public BoxCollider propCollider;
    public Material material;
    public Vector3 propSize;

    public int propLayer = 0;
    public ShelfGrid shelfGrid;

    private bool propState = false;

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
            ColorUtility.TryParseHtmlString("#4B4B4B", out Color disabledColor);

            material.color = disabledColor;

            Debug.Log("Setting disabled color for : " + gameObject.name);
        }

        propState = state;
    }

    public void OnPicked()
    {
        if (!propState) return;

        BoosterManager.previousPickedProp = this;

        GameManager.instance.slotManager.EnqueueProp(this, OnPropQueueComplete);
        transform.localScale = transform.localScale / 2;

        Debug.Log(gameObject.name + " is picked!!");
    }

    private void OnPropQueueComplete()
    {
        shelfGrid.UpdatePropsState(this);
    }

    public void PropUndo()
    {
        transform.parent = null;
        //transform.SetPositionAndRotation(origPropPos, Quaternion.identity);
        Tween moveTween = SetPositionTween(origPropPos);
        transform.rotation = Quaternion.identity;
        transform.localScale = origPropScale;

        GameManager.instance.slotManager.ResetSlot(this);
        
        moveTween.OnComplete(() => shelfGrid.OnPropUndo(this));
    }

    public Tween SetPositionTween(Vector3 pos)
    {
        origPropPos = pos;
        //transform.position = pos;

        Tween moveTween = transform.DOMove(pos, 0.5f).SetEase(Ease.InQuad);

        return moveTween;
    }
}
