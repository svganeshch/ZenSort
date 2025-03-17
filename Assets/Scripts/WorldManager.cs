using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager instance;

    [Header("UI Camera")]
    public Camera UICamera;

    [Header("VFX")]
    public ParticleSystem matchVFX;
    public ParticleSystem shuffleVFX;

    [Header("Prefabs")]
    public GameObject starPrefab;
    public GameObject compartmentCameraPrefab;

    private void Awake()
    {
        instance = this;
    }

    public void PlayShuffleVFX(List<Prop> existingPropsList)
    {
        foreach (var prop in existingPropsList)
        {
            var svfx = Instantiate(shuffleVFX, prop.transform.position, Quaternion.identity);
            
            Destroy(svfx.gameObject, 1);
        }
    }
}
