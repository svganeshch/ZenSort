using UnityEngine;

public class WorldLayerMaskManager : MonoBehaviour
{
    public static WorldLayerMaskManager instance;

    public LayerMask propLayerMask;

    [Header("VFX")]
    public ParticleSystem matchVFX;

    private void Awake()
    {
        instance = this;
    }
}
