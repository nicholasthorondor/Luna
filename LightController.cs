using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightController : MonoBehaviour
{

    Light directionalLight;
    float initialLightIntensity;

    // Start is called before the first frame update
    void Start ()
    {
        directionalLight = GetComponent<Light> ();
        GameManager.instance.DirectionalLight = directionalLight;
        initialLightIntensity = directionalLight.intensity;
        GameManager.instance.InitialLightIntensity = initialLightIntensity;
    }
}
