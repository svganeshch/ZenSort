using System;
using System.Collections.Generic;
using UnityEngine;

public class ShelfGrid : MonoBehaviour
{
    BoxCollider shelfCollider;

    public float shelfPaddingX = 0.1f;
    public float shelfPaddingZ = -0.35f;

    float shelfLeft;
    float shelfRight;
    float shelfY;
    float shelfZ;
    float currentShelfPosX;
    float currentShelfPosZ;

    int currentLayer = 0;

    public List<Prop> shelfPropList = new List<Prop>();

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

                shelfPropList.Add(prop);
                allProps.Remove(prop);
            }
        }

        currentPropList = allProps;
        currentPropList = TryFitInGaps(currentPropList);

        currentLayer++;

        return currentPropList;
    }

    public List<Prop> TryFitInGaps(List<Prop> propsToFit)
    {
        for (int i = 0; i < shelfPropList.Count - 1; i++)
        {
            float gapStart = shelfPropList[i].transform.position.x + (shelfPropList[i].propSize.x / 2);
            float gapEnd = shelfPropList[i + 1].transform.position.x - (shelfPropList[i + 1].propSize.x / 2);
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

                    shelfPropList.Add(prop);
                    propsToFit.RemoveAt(j);

                    break;
                }
            }
        }

        return propsToFit;
    }

    private void SetProp(Prop prop)
    {

    }

    private void RemovePalcedProps(ref List<Prop> currentPropList)
    {
        foreach (Prop prop in shelfPropList)
        {
            currentPropList.Remove(prop);
        }
    }
}