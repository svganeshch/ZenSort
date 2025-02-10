using System;
using UnityEngine;

[Serializable]
public class Prop : MonoBehaviour
{
    private MeshRenderer meshRenderer;
    public BoxCollider propCollider;

    [HideInInspector] public Material material;
    public Vector3 propSize;

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
        propSize = propCollider.size;
    }

    public void SetPropState(bool state)
    {
        if (state)
        {
            material.color = Color.white;
        }
        else
        {
            material.color = Color.gray;

            Debug.Log("Setting disabled color for : " + gameObject.name);
        }
    }

    public void OnPicked()
    {
        GameManager.instance.slotManager.EnqueueProp(this);
        Debug.Log(gameObject.name + " is picked!!");
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos;
    }
}
