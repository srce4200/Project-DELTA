using UnityEngine;
using Photon.Pun;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float timeSpeed = 1.0f;
    float dayDuration = 86400;
    public float currentTime = 86400/2f;
    int currentDay;

    [Header("References")]
    [SerializeField] Light sun;
    [SerializeField] Material skyboxMaterial;
    public AnimationCurve intensityCurve;

    [Header("Day / Night Visual Tints")]
    [SerializeField] Color dayHorizonColor = new Color(0.8f, 0.9f, 1f);
    [SerializeField] Color nightHorizonColor = new Color(0.02f, 0.02f, 0.05f);

    PhotonView _pv;
    public static DayNightCycle Instance;
    private void Awake()
    {
        print("Who faster");
        Instance = this;        
    }
    private void Start()
    {
        _pv = GetComponent<PhotonView>();

        if (sun == null) sun = GetComponent<Light>();
        if (skyboxMaterial == null && RenderSettings.skybox != null)
        {
            skyboxMaterial = RenderSettings.skybox;
        }
    }
    
    public void SetTime(float timeInSeconds)
    {
        currentTime = timeInSeconds;
        UpdateDayNightCycle();
    }
    public float GetTime()
    {
        return currentTime;
    }

    private void Update()
    {
        if (_pv == null || _pv.IsMine || !PhotonNetwork.IsConnected)
        {
            UpdateDayNightCycle();
        }
    }

    private void UpdateDayNightCycle()
    {
        currentTime += Time.deltaTime * timeSpeed;

        if (currentTime >= dayDuration)
        {
            currentDay += 1;
            currentTime = 0f;
        }

        float normalizedTime = currentTime / dayDuration;

        // 1. Sun Rotation & Intensity
        float sunAngle = (normalizedTime * 360f) + 90f;
        sun.transform.rotation = Quaternion.Euler(new Vector3(-sunAngle, 45f, 0f));

        if (intensityCurve != null)
        {
            sun.intensity = intensityCurve.Evaluate(normalizedTime);
        }

        // 2. Smooth Transition Blending based on Sun Height
        UpdateSkyboxBlending();
    }

    private void UpdateSkyboxBlending()
    {
        if (skyboxMaterial == null) return;

        // sunHeight ranges from -1 (midnight) to +1 (noon)
        float sunHeight = -sun.transform.forward.y;

        // Create a 0-to-1 blend factor (0 = full night, 1 = full day)
        // We use Smoothstep for a soft, cinematic transition curve
        float blendFactor = Mathf.InverseLerp(-0.3f, 0.3f, sunHeight);

        // Transfer/Blend Sky Exposure (Bright day vs Pitch black night)
        float targetExposure = Mathf.Lerp(0f, 1f, blendFactor);
        skyboxMaterial.SetFloat("_SkyExposure", targetExposure);

        // Blend Horizon Colors smoothly between day and night values
        Color currentHorizon = Color.Lerp(nightHorizonColor, dayHorizonColor, blendFactor);
        skyboxMaterial.SetColor("_HorizonColor", currentHorizon);

        // Toggle Moon and Stars based on transition threshold
        if (blendFactor < 0.2f)
        {
           // skyboxMaterial.SetFloat("_EnableMoon", 1f);
            skyboxMaterial.SetFloat("_EnableStars", 1f);
        }
        else
        {
           // skyboxMaterial.SetFloat("_EnableMoon", 0f);
            skyboxMaterial.SetFloat("_EnableStars", 0f);
        }

        // Sync ambient lighting so the world goes dark with the sky
        RenderSettings.ambientIntensity = Mathf.Clamp01(blendFactor + 0.05f);
    }
}