using System.Collections.Generic;
using UnityEngine;

public class ShelfGrid : MonoBehaviour
{
    BoxCollider shelfCollider;

    public float shelfPaddingX = 0.1f;
    public float shelfPaddingZ = 0.25f;

    float shelfLeft;
    float shelfRight;
    float shelfY;
    float shelfZ;
    float currentShelfPosX;
    float currentShelfPosZ;

    private void Awake()
    {
        shelfCollider = GetComponent<BoxCollider>();

        shelfLeft = shelfCollider.bounds.min.x;
        shelfRight = shelfCollider.bounds.max.x;

        shelfY = shelfCollider.bounds.max.y;
        shelfZ = shelfCollider.bounds.center.z;

        currentShelfPosX = shelfLeft;
    }

    public bool SetProp(Prop prop)
    {
        float propWidth = prop.propSize.x;
        float propY = shelfY + (prop.propSize.y / 2) - prop.propCollider.center.y;
        float propZ = shelfZ + (prop.propSize.z / 2);

        if (currentShelfPosX + (propWidth / 2) > shelfRight)
        {
            Debug.Log(prop.gameObject.name + " cannot fit on : " + gameObject.name);
            return false;
        }

        Vector3 propPos = new Vector3(currentShelfPosX + (propWidth / 2), propY, propZ);
        prop.SetPosition(propPos);

        currentShelfPosX += propWidth + shelfPaddingX;

        return true;
    }
}
