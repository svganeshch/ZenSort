using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShelfGrid : MonoBehaviour
{
    BoxCollider shelfCollider;

    public float shelfPaddingX = 0.1f;
    private float shelfPaddingZ = 0.15f;
    private float propOverlapBoxDepth = 0.25f;

    float shelfLeft;
    float shelfRight;
    float shelfY;
    float shelfZ;
    float currentShelfPosX;
    float currentShelfPosZ;

    int currentLayer = 0;

    private List<Prop> propsMovedInPreviousPick = new List<Prop>();
    public List<List<Prop>> shelfPropList = new List<List<Prop>>();

    private void Awake()
    {
        shelfCollider = GetComponent<BoxCollider>();

        shelfLeft = shelfCollider.bounds.min.x;
        shelfRight = shelfCollider.bounds.max.x;

        shelfY = shelfCollider.bounds.max.y;
        shelfZ = shelfCollider.bounds.size.z;

        currentShelfPosX = shelfLeft;
        currentShelfPosZ = shelfZ;
    }

    public List<Prop> SetProps(List<Prop> allProps)
    {
        var currentPropList = new List<Prop>(allProps);
        var currentPlacedProps = new List<Prop>();

        currentShelfPosX = shelfLeft;
        currentShelfPosZ = shelfZ;

        foreach (Prop prop in currentPropList)
        {
            float propWidth = prop.propSize.x;
            float propY = shelfY + (prop.propCollider.size.y / 2) - prop.propCollider.center.y;
            float propDepth = shelfZ;

            if (currentShelfPosX + propWidth <= shelfRight)
            {
                currentShelfPosZ = shelfZ - (currentLayer * shelfPaddingZ);

                Vector3 propPos = new Vector3(currentShelfPosX + (propWidth / 2), propY, currentShelfPosZ);

                currentPlacedProps.Add(prop);
                allProps.Remove(prop);

                SetProp(prop, propPos);

                currentShelfPosX += propWidth + shelfPaddingX;
            }
        }

        shelfPropList.Add(currentPlacedProps);

        currentPropList = allProps;
        currentPropList = TryFitInGaps(currentPropList);

        currentLayer++;

        return currentPropList;
    }

    public List<Prop> TryFitInGaps(List<Prop> propsToFit)
    {
        var currentLayerProps = shelfPropList[currentLayer];
        var remainingProps = new List<Prop>(propsToFit);

        for (int i = 0; i < currentLayerProps.Count - 1; i++)
        {
            float gapStart = currentLayerProps[i].transform.position.x + (currentLayerProps[i].propSize.x / 2);
            float gapEnd = currentLayerProps[i + 1].transform.position.x - (currentLayerProps[i + 1].propSize.x / 2);
            float availableSpace = gapEnd - gapStart;

            for (int j = 0; j < remainingProps.Count; j++)
            {
                Prop prop = remainingProps[j];

                if (prop.propSize.x <= availableSpace)
                {
                    float propY = shelfY + (prop.propSize.y / 2) - prop.propCollider.center.y;
                    float propZ = currentShelfPosZ;
                    float propX = gapStart + (prop.propSize.x / 2);

                    Vector3 propPos = new Vector3(propX, propY, propZ);

                    shelfPropList[currentLayer].Add(prop);
                    propsToFit.Remove(prop);
                    remainingProps.RemoveAt(j);

                    SetProp(prop, propPos);

                    break;
                }
            }
        }

        return propsToFit;
    }

    private void SetProp(Prop prop, Vector3 propPos)
    {
        prop.shelfGrid = this;
        prop.propLayer = currentLayer;

        Tween setPosTween = prop.SetPositionTween(propPos);
        setPosTween.OnComplete(() => UpdatePropsState());

        //Quaternion targetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
        //prop.transform.rotation = targetRotation;
    }

    public void UpdatePropsState(Prop pickedProp = null)
    {
        propsMovedInPreviousPick.Clear();

        int startLayer = pickedProp != null ? pickedProp.propLayer + 1 : 0;

        List<Prop> propsToMove = new List<Prop>();

        for (int i = startLayer; i < shelfPropList.Count; i++)
        {
            var propList = new List<Prop>(shelfPropList[i]);

            foreach (var prop in propList)
            {
                if (prop == null) continue;

                //Vector3 propSize = prop.propCollider.bounds.size;
                Vector3 propCenter = prop.propCollider.bounds.center;

                Vector3 overlapBoxSize = new Vector3(prop.propSize.x, prop.propSize.y, propOverlapBoxDepth);
                Vector3 frontOffset = prop.transform.forward * (prop.propSize.z * 0.5f + overlapBoxSize.z * 0.5f);
                Vector3 overlapBoxPos = propCenter + frontOffset;

                Collider[] colliders = Physics.OverlapBox(overlapBoxPos, overlapBoxSize / 2, prop.transform.rotation, WorldLayerMaskManager.instance.propLayerMask);
                bool isBlocked = colliders.Any(c => c != prop.propCollider);

                if (isBlocked)
                {
                    prop.SetPropState(false);
                }
                else if (pickedProp != null)
                {
                    shelfPropList[i].Remove(prop);
                    shelfPropList[i - 1].Add(prop);
                    prop.SetPropState(true);
                    propsToMove.Add(prop);
                }
                else
                {
                    prop.SetPropState(true);
                }
            }
        }

        // Move all affected props forward
        MovePropsForward(propsToMove);
    }

    private void MovePropsForward(List<Prop> movedProps)
    {
        List<Prop> nextPropsToMove = new List<Prop>();

        foreach (var movedProp in movedProps)
        {
            Vector3 shiftPos = movedProp.transform.position;
            shiftPos.z += shelfPaddingZ;

            propsMovedInPreviousPick.Add(movedProp);

            movedProp.transform.DOMove(shiftPos, 0.25f).SetEase(Ease.InQuad).OnComplete(() =>
            {
                // Check if there's another layer behind that should also move
                int nextLayer = movedProp.propLayer + 1;
                if (nextLayer < shelfPropList.Count)
                {
                    nextPropsToMove.AddRange(shelfPropList[nextLayer]);
                }
            });
        }

        // Recursively move next layers
        if (nextPropsToMove.Count > 0)
        {
            MovePropsForward(nextPropsToMove);
        }
    }


    private void ShiftPropTween(Prop prop, Vector3 shiftPos)
    {
        propsMovedInPreviousPick.Add(prop);

        shiftPos = new Vector3(prop.transform.position.x, prop.transform.position.y, prop.transform.position.z + shelfPaddingZ);

        prop.transform.DOMove(shiftPos, 0.25f).SetEase(Ease.InQuad);
    }

    public void OnPropUndo(Prop prop)
    {
        shelfPropList[prop.propLayer].Add(prop);

        foreach (var previousProp in propsMovedInPreviousPick)
        {
           Tween moveTween = previousProp.SetPositionTween(previousProp.origPropPos);
            moveTween.OnComplete(() => UpdatePropsState());
        }

        propsMovedInPreviousPick.Clear();
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        for (int i = 0; i < shelfPropList.Count; i++)
        {
            var currentLayerPropList = shelfPropList[i];

            foreach (var prop in currentLayerPropList)
            {
                Gizmos.color = Color.blue;

                if (prop == null) continue;

                //Vector3 propSize = prop.propCollider.size;
                Vector3 propCenter = prop.propCollider.bounds.center;

                Vector3 overlapBoxSize = new Vector3(prop.propSize.x, prop.propSize.y, propOverlapBoxDepth);

                Vector3 frontOffset = prop.transform.forward * (prop.propSize.z * 0.5f + overlapBoxSize.z * 0.5f);
                Vector3 overlapBoxPos = propCenter + frontOffset;

                Gizmos.DrawWireCube(overlapBoxPos, overlapBoxSize);
            }
        }
    }
}