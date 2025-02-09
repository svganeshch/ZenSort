using System;
using UnityEngine;

[Serializable]
public class Prop : MonoBehaviour
{
    public BoxCollider propCollider;

    public Vector3 propSize;

    private void Awake()
    {
        propCollider = GetComponent<BoxCollider>();

        if (propCollider == null)
        {
            propCollider = GetComponentInChildren<BoxCollider>();
        }

        CalculatePropSize();
    }

    public void OnPicked()
    {
        Debug.Log(gameObject.name + " is picked!!");
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }

    private void CalculatePropSize()
    {
        propSize = propCollider.size;
    }
}
