using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public Transform propsSpawnPoint;
    public GameObject[] propPrefabs;

    [SerializeField]
    public List<Prop> generatedProps = new List<Prop>();

    public List<Prop> GenerateProps(int numberOfProps)
    {
        generatedProps.Clear();

        int propPairs = numberOfProps / 3;

        for (int i = 0; i < propPairs; i++)
        {
            GameObject propPrefab = propPrefabs[Random.Range(0, propPrefabs.Length)];
            
            for (int j = 0; j < 3; j++)
            {
                GameObject propObj = Instantiate(propPrefab, propsSpawnPoint.position, Quaternion.identity);
                propObj.transform.parent = transform;

                Prop prop = propObj.GetComponent<Prop>();

                generatedProps.Add(prop);
            }
        }

        return generatedProps;
    }
}
