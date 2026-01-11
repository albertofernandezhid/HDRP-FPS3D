using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DynamicDayManager : MonoBehaviour
{
    public Light sun;
    [Range(15f, 35f)] public float minSunElevation = 18f;
    [Range(45f, 85f)] public float maxSunElevation = 65f;
    public float dayCycleDuration = 600f;
    public float latitude = 35f;
    [Range(0f, 16f)] public float godRaysIntensity = 1f; // HDRP permite hasta 16 en el dimmer

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
        // Usamos Sin para que el sol suba y baje suavemente sin tocar el horizonte
        float sunWave = Mathf.Sin(_time01 * Mathf.PI);
        float elevation = Mathf.Lerp(minSunElevation, maxSunElevation, sunWave);

        // Movimiento horizontal oscilante para que nunca sea noche
        float azimuth = Mathf.Lerp(-100f, 100f, Mathf.PingPong(_time01 * 2f, 1f));

        sun.transform.rotation = Quaternion.Euler(elevation, azimuth + latitude, 0f);

        float elevation01 = Mathf.InverseLerp(minSunElevation, maxSunElevation, elevation);

        sun.colorTemperature = Mathf.Lerp(3800f, 6500f, elevation01);
        sun.intensity = Mathf.Lerp(20000f, 120000f, elevation01);

        if (_sunData != null)
        {
            // Los rayos son más visibles cuando el sol está algo bajo y el cielo despejado
            float rayFactor = sunWave * (1f - (cloudCoverage * 0.5f));
            _sunData.volumetricDimmer = godRaysIntensity * rayFactor;
        }
    }

    void UpdateAtmosphere()
    {
        float elevation = Mathf.Lerp(minSunElevation, maxSunElevation, Mathf.Sin(_time01 * Mathf.PI));

        if (_clouds != null)
        {
            _clouds.densityMultiplier.value = cloudCoverage;
            _clouds.sunLightDimmer.value = Mathf.Lerp(1f, 0.55f, cloudCoverage);
            _clouds.ambientLightProbeDimmer.value = Mathf.Lerp(1f, 0.7f, cloudCoverage);

            // IMPORTANTE: Activar sombras para God Rays reales
            _clouds.shadows.value = true;

            _cloudOffset += new Vector3(windDirection.x, 0f, windDirection.y) * windSpeed * Time.deltaTime;
            _clouds.shapeOffset.value = _cloudOffset;
        }

        if (_fog != null)
        {
            // La niebla es más densa "por la mañana" (inicio del ciclo)
            float fogValue = fogHumidity * Mathf.Lerp(1.1f, 0.6f, Mathf.Sin(_time01 * Mathf.PI));
            _fog.enableVolumetricFog.value = true;
            _fog.meanFreePath.value = Mathf.Lerp(10000f, 400f, fogValue);
        }

        if (_sky != null)
        {
            // Ajuste dinámico del tinte del cielo según la elevación
            float sunFactor = Mathf.InverseLerp(minSunElevation, maxSunElevation, elevation);
            _sky.horizonTint.value = Color.Lerp(new Color(1f, 0.8f, 0.6f), Color.white, sunFactor);
            _sky.zenithTint.value = Color.Lerp(new Color(0.6f, 0.8f, 1f), Color.white, sunFactor);
        }
    }
}