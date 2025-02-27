using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    [Header("UI Camera")]
    public Camera UICamera;

    [Header("VFX")]
    public ParticleSystem matchVFX;

    [Header("Prefabs")]
    public GameObject starPrefab;
    public GameObject bonusStarPrefab;

    private void Awake()
    {
        instance = this;
    }
}
