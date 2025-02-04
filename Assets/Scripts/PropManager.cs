using System.Collections.Generic;
using UnityEngine;

public class PropManager : MonoBehaviour
{
    public GameObject[] propPrefabs;

    private List<Prop> generatedProps = new List<Prop>();

    public List<Prop> GenerateProps(int numberOfProps)
    {
        int propPairs = numberOfProps / 3;

        for (int i = 0; i < propPairs; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                GameObject propObj = Instantiate(propPrefabs[Random.Range(0, propPrefabs.Length)]);
                Prop prop = propObj.GetComponent<Prop>();

                generatedProps.Add(prop);
            }
        }

        return generatedProps;
    }
}
