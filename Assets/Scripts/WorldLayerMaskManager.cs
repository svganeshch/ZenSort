using UnityEngine;

public class WorldLayerMaskManager : MonoBehaviour
{
    public static WorldLayerMaskManager instance;

    public LayerMask propLayerMask;

    private void Awake()
    {
        instance = this;
    }
}
