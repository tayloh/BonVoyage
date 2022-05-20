using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneryManager : MonoBehaviour
{
    public SceneryType SceneryType;


    [SerializeField] private Material SkyboxSunny;
    [SerializeField] private Material SkyboxStorm;
    [SerializeField] private Material SkyboxEvening;
    [SerializeField] private Material SkyboxNight;

    [SerializeField] private Material WaterSunny;
    [SerializeField] private Material WaterStorm;
    [SerializeField] private Material WaterEvening;
    [SerializeField] private Material WaterNight;

    [SerializeField] private GameObject Water;

    [SerializeField] private Color MissParticlesSunny;
    [SerializeField] private Color MissParticlesStorm;
    [SerializeField] private Color MissParticlesEvening;
    [SerializeField] private Color MissParticlesNight;

    [SerializeField] private GameObject HexagonGridGround;

    [SerializeField] private Material HexSunny;
    [SerializeField] private Material HexStorm;
    [SerializeField] private Material HexEvening;
    [SerializeField] private Material HexNight;

    [SerializeField] private GameObject HexPrefab;


    void Awake()
    {
        SetScenery(SceneryType);
    }
    
    private void SetHexagonMaterial(SceneryType type)
    {
        var renderers = HexPrefab.GetComponentsInChildren<Renderer>();

        foreach (var item in renderers)
        {
            switch (type)
            {
                case SceneryType.Sunny:
                    item.material = HexSunny;
                    break;
                case SceneryType.Evening:
                    item.material = HexEvening;
                    break;
                case SceneryType.Storm:
                    item.material = HexStorm;
                    break;
                case SceneryType.Night:
                    item.material = HexNight;
                    break;

            }
        }
    }

    private void SetScenery(SceneryType type)
    {
        SetHexagonMaterial(type);

        switch (type)
        {
            case SceneryType.Sunny:
                RenderSettings.skybox = SkyboxSunny;
                Water.GetComponent<Renderer>().material = WaterSunny;
                break;
            case SceneryType.Storm:
                RenderSettings.skybox = SkyboxStorm;
                Water.GetComponent<Renderer>().material = WaterStorm;
                break;
            case SceneryType.Evening:
                RenderSettings.skybox = SkyboxEvening;
                Water.GetComponent<Renderer>().material = WaterEvening;
                break;
            case SceneryType.Night:
                RenderSettings.skybox = SkyboxNight;
                Water.GetComponent<Renderer>().material = WaterNight;
                break;
        }
    }

   
}

public enum SceneryType
{
    Sunny,
    Storm,
    Evening,
    Night
}
