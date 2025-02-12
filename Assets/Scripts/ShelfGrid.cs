using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShelfGrid : MonoBehaviour
{
    BoxCollider shelfCollider;

    public float shelfPaddingX = 0.1f;
    public float shelfPaddingZ = -0.35f;

    public LayerMask propLayerMask;

    float shelfLeft;
    float shelfRight;
    float shelfY;
    float shelfZ;
    float currentShelfPosX;
    float currentShelfPosZ;

    int currentLayer = 0;

    public List<List<Prop>> shelfPropList = new List<List<Prop>>();

    private void Awake()
    {
        shelfCollider = GetComponent<BoxCollider>();

        shelfLeft = shelfCollider.bounds.min.x;
        shelfRight = shelfCollider.bounds.max.x;

        shelfY = shelfCollider.bounds.max.y;
        shelfZ = shelfCollider.bounds.center.z;

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
            float propY = shelfY + (prop.propSize.y / 2) - prop.propCollider.center.y;
            float propDepth = shelfZ + (prop.propSize.z / 2);

            if (currentShelfPosX + propWidth <= shelfRight)
            {
                currentShelfPosZ = propDepth + (shelfPaddingZ - currentLayer) * 0.1f;

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
        prop.SetPosition(propPos);

        if (currentLayer > 0)
        {
            prop.SetPropState(false);
        }
    }

    public void UpdatePropsState(Prop pickedProp)
    {
        int propLayer = pickedProp.propLayer + 1;

        for (int i = propLayer; i < shelfPropList.Count; i++)
        {
            var propList = new List<Prop>(shelfPropList[i]);

            foreach (var prop in propList)
            {
                if (prop == null) continue;

                Vector3 propSize = prop.propCollider.bounds.size;
                Vector3 propCenter = prop.propCollider.bounds.center;

                Vector3 overlapBoxSize = new Vector3(propSize.x, propSize.y, 0.5f);

                Vector3 frontOffset = prop.transform.forward * (propSize.z * 0.5f + overlapBoxSize.z * 0.5f);
                Vector3 overlapBoxPos = propCenter + frontOffset;

                Collider[] colliders = Physics.OverlapBox(overlapBoxPos, overlapBoxSize / 2, prop.transform.rotation, propLayerMask);

                bool isBlocked = colliders.Any(c => c != prop.propCollider);

                if (isBlocked)
                {
                    prop.SetPropState(false);
                }
                else
                {
                    shelfPropList[i].Remove(prop);
                    shelfPropList[pickedProp.propLayer].Add(prop);

                    prop.SetPropState(true);
                    ShiftPropTween(prop, pickedProp.propPos);
                }
            }
        }
    }

    private void ShiftPropTween(Prop prop, Vector3 shiftPos)
    {
        shiftPos = new Vector3(prop.transform.position.x, prop.transform.position.y, shiftPos.z);

        prop.transform.DOMove(shiftPos, 0.5f).SetEase(Ease.InQuad)
            .OnComplete(() => prop.SetPosition(shiftPos));
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        for (int i = 1; i < shelfPropList.Count; i++)
        {
            var currentLayerPropList = shelfPropList[i];

            foreach (var prop in currentLayerPropList)
            {
                Gizmos.color = Color.blue;

                Vector3 propSize = prop.propCollider.size;
                Vector3 propCenter = prop.propCollider.bounds.center;

                Vector3 overlapBoxSize = new Vector3(propSize.x, propSize.y, 0.5f);

                Vector3 frontOffset = prop.transform.forward * (propSize.z * 0.5f + overlapBoxSize.z * 0.5f);
                Vector3 overlapBoxPos = propCenter + frontOffset;

                Gizmos.DrawWireCube(overlapBoxPos, overlapBoxSize);
            }
        }
    }
}