using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutsideController : MonoBehaviour
{
    [SerializeField] private List<GameObject> lights = new List<GameObject>();
    [SerializeField] private List<Renderer> emissions = new List<Renderer>();

    public void OnStreetLights()
    {
        Debug.Log("OnStreetLights");
        int count = Mathf.Min(lights.Count, emissions.Count); // защита от разного размера списков
        for (int i = 0; i < count; i++)
        {
            if (lights[i] != null && emissions[i] != null)
            {
                lights[i].SetActive(true);
                emissions[i].material.EnableKeyword("_EMISSION");
            }
        }
    }

    public void OffStreetLights()
    {
        Debug.Log("OffStreetLights");
        int count = Mathf.Min(lights.Count, emissions.Count); // защита от разного размера списков
        for (int i = 0; i < count; i++)
        {
            if (lights[i] != null && emissions[i] != null)
            {
                lights[i].SetActive(false);
                emissions[i].material.DisableKeyword("_EMISSION");
            }
        }
    }
}