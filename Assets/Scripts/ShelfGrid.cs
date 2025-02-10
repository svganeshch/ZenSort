using System;
using System.Collections.Generic;
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
                prop.SetPosition(propPos);

                currentShelfPosX += propWidth + shelfPaddingX;

                currentPlacedProps.Add(prop);
                allProps.Remove(prop);

                if (currentLayer > 0)
                {
                    prop.SetPropState(false);
                }
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

        for (int i = 0; i < currentLayerProps.Count - 1; i++)
        {
            float gapStart = currentLayerProps[i].transform.position.x + (currentLayerProps[i].propSize.x / 2);
            float gapEnd = currentLayerProps[i + 1].transform.position.x - (currentLayerProps[i + 1].propSize.x / 2);
            float availableSpace = gapEnd - gapStart;

            for (int j = 0; j < propsToFit.Count; j++)
            {
                Prop prop = propsToFit[j];

                if (prop.propSize.x <= availableSpace)
                {
                    float propY = shelfY + (prop.propSize.y / 2) - prop.propCollider.center.y;
                    float propZ = currentShelfPosZ;
                    float propX = gapStart + (availableSpace / 2);

                    Vector3 propPos = new Vector3(propX, propY, propZ);
                    prop.SetPosition(propPos);

                    shelfPropList[currentLayer].Add(prop);
                    propsToFit.RemoveAt(j);

                    if (currentLayer > 0)
                    {
                        prop.SetPropState(false);
                    }

                    break;
                }
            }
        }

        return propsToFit;
    }

    public void UpdatePropsState()
    {
        for (int i = shelfPropList.Count - 1; i >= 1; i--)
        {
            var propList = shelfPropList[i];

            foreach (var prop in propList)
            {
                Vector3 propSize = prop.propCollider.bounds.size;
                Vector3 propCenter = prop.propCollider.bounds.center;

                Vector3 propFrontPos = propCenter + prop.transform.forward * (propSize.z / 2);

                Vector3 overlapBoxSize = new Vector3(propSize.x, propSize.y, 0.05f);

                Collider[] colliders = Physics.OverlapBox(propFrontPos, overlapBoxSize / 2, prop.transform.rotation, propLayerMask);

                Collider col = colliders[0];
                if (col != null)
                {
                    col.gameObject.TryGetComponent<Prop>(out Prop propCol);

                    if (propCol != null)
                    {
                        propCol.SetPropState(false);
                        Debug.Log($"Prop {prop.name} overlaps with {col.name}");
                    }
                }
            }
        }
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

                Vector3 propSize = prop.propCollider.bounds.size;
                Vector3 propCenter = prop.propCollider.bounds.center;

                Vector3 overlapBoxPos = propCenter + prop.transform.forward * (propSize.z / 2);

                Vector3 overlapBoxSize = new Vector3(propSize.x, propSize.y, 0.05f);

                Gizmos.DrawWireCube(overlapBoxPos, overlapBoxSize);
            }
        }
    }
}