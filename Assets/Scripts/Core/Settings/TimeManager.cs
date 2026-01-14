using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;

public class DynamicDayManager : MonoBehaviour
{
    public Light sun;
    [Range(0f, 35f)] public float minSunElevation = 5f;
    [Range(45f, 90f)] public float maxSunElevation = 75f;
    public float dayCycleDuration = 600f;
    public float latitude = 35f;
    [Range(0f, 16f)] public float godRaysIntensity = 1.2f;

    [Header("Artificial Lights Settings")]
    public float streetLightActivationAngle = 15f;

    private List<Light> _streetLights = new List<Light>();
    private List<Light> _indoorLights = new List<Light>();
    private bool _streetLightsActive;

    [Header("Atmosphere")]
    public Volume globalVolume;
    [Range(0f, 1f)] public float cloudCoverage = 0.4f;
    [Range(0f, 1f)] public float fogHumidity = 0.15f;
    public Vector2 windDirection = new Vector2(1f, 0.4f);
    public float windSpeed = 15f;

    float _time01;
    Vector3 _cloudOffset;

    Fog _fog;
    VolumetricClouds _clouds;
    PhysicallyBasedSky _sky;
    HDAdditionalLightData _sunData;

    void Start()
    {
        globalVolume.profile.TryGet(out _fog);
        globalVolume.profile.TryGet(out _clouds);
        globalVolume.profile.TryGet(out _sky);

        if (sun != null)
        {
            _sunData = sun.GetComponent<HDAdditionalLightData>();
            sun.useColorTemperature = true;
            sun.lightUnit = LightUnit.Lux;
        }

        FindAndConfigureLights();

        // Forzar estado inicial apagado para evitar errores de lógica al arranque
        _streetLightsActive = false;
        UpdateLightState(false);
    }

    void FindAndConfigureLights()
    {
        _streetLights.Clear();
        _indoorLights.Clear();

        GameObject[] streetL = GameObject.FindGameObjectsWithTag("StreetLight");
        foreach (GameObject go in streetL)
        {
            Light l = go.GetComponent<Light>();
            if (l != null) _streetLights.Add(l);
        }

        GameObject[] indoorL = GameObject.FindGameObjectsWithTag("IndoorLight");
        foreach (GameObject go in indoorL)
        {
            Light l = go.GetComponent<Light>();
            if (l != null)
            {
                l.enabled = true;
                _indoorLights.Add(l);
            }
        }
    }

    void Update()
    {
        UpdateTime();
        UpdateSun();
        UpdateAtmosphere();
    }

    void UpdateTime()
    {
        _time01 += Time.deltaTime / dayCycleDuration;
        if (_time01 > 1f) _time01 = 0f;
    }

    void UpdateSun()
    {
        float sunWave = Mathf.Sin(_time01 * Mathf.PI);
        float elevation = Mathf.Lerp(minSunElevation, maxSunElevation, sunWave);
        float azimuth = Mathf.Lerp(-100f, 100f, Mathf.PingPong(_time01 * 2f, 1f));

        sun.transform.rotation = Quaternion.Euler(elevation, azimuth + latitude, 0f);

        float elevation01 = Mathf.InverseLerp(minSunElevation, maxSunElevation, elevation);

        sun.colorTemperature = Mathf.Lerp(3000f, 6500f, elevation01);
        sun.intensity = Mathf.Lerp(1000f, 60000f, elevation01);

        // Control de luces de calle usando la variable local elevation
        bool shouldBeActive = elevation < streetLightActivationAngle;
        if (shouldBeActive != _streetLightsActive)
        {
            UpdateLightState(shouldBeActive);
        }

        if (_sunData != null)
        {
            float rayFactor = sunWave * (1f - (cloudCoverage * 0.5f));
            _sunData.volumetricDimmer = godRaysIntensity * rayFactor;
        }
    }

    void UpdateLightState(bool active)
    {
        _streetLightsActive = active;
        foreach (Light l in _streetLights)
        {
            if (l != null) l.enabled = active;
        }
    }

    void UpdateAtmosphere()
    {
        float sunWave = Mathf.Sin(_time01 * Mathf.PI);
        float elevation = Mathf.Lerp(minSunElevation, maxSunElevation, sunWave);

        if (_clouds != null)
        {
            _clouds.densityMultiplier.value = cloudCoverage;
            _clouds.sunLightDimmer.value = Mathf.Lerp(1f, 0.55f, cloudCoverage);
            _clouds.ambientLightProbeDimmer.value = Mathf.Lerp(1f, 0.7f, cloudCoverage);
            _clouds.shadows.value = true;

            _cloudOffset += new Vector3(windDirection.x, 0f, windDirection.y) * windSpeed * Time.deltaTime;
            _clouds.shapeOffset.value = _cloudOffset;
        }

        if (_fog != null)
        {
            float fogValue = fogHumidity * Mathf.Lerp(1.5f, 0.5f, sunWave);
            _fog.enableVolumetricFog.value = true;
            _fog.meanFreePath.value = Mathf.Lerp(10000f, 300f, fogValue);
        }

        if (_sky != null)
        {
            float sunFactor = Mathf.InverseLerp(minSunElevation, maxSunElevation, elevation);
            _sky.horizonTint.value = Color.Lerp(new Color(1f, 0.5f, 0.2f), Color.white, sunFactor);
            _sky.zenithTint.value = Color.Lerp(new Color(0.2f, 0.4f, 1f), Color.white, sunFactor);
        }
    }
}