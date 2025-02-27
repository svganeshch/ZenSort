using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    [Header("VFX")]
    public ParticleSystem matchVFX;

    [Header("Prefabs")]
    public GameObject starPrefab;

    private void Awake()
    {
        instance = this;
    }
}
