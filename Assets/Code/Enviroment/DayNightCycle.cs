using UnityEngine;
using Photon.Pun;

public class DayNightCycle : MonoBehaviour
{
    public float timeSpeed = 1.0f;
    [SerializeField] float dayDuration = 120f; // Duration of one full day in seconds
    [SerializeField] Light sun; // Reference to the Directional Light
    public AnimationCurve intensityCurve; // Animation curve for controlling intensity
    public float currentTime = 0f;
    int currentDay;

    PhotonView _pv;

    private void Start()
    {
        _pv = GetComponent<PhotonView>();

        if (sun == null)
        {
            sun = GetComponent<Light>();
        }
    }

    private void Update()
    {
        UpdateSunPositionAndIntensity();
    }

    private void UpdateSunPositionAndIntensity()
    {
        // Calculate time progression
        currentTime += Time.deltaTime * timeSpeed;

        // Reset the current time when the day is over
        if (currentTime >= dayDuration)
        {
            currentDay += 1;
            currentTime = 0f;
        }

        float normalizedTime = currentTime / dayDuration;

        // Set the sun rotation (Make sure it rotates smoothly around the X axis)
        float sunAngle = (normalizedTime * 360f) - 90f; // Offset so 0 is sunrise
        sun.transform.rotation = Quaternion.Euler(new Vector3(sunAngle, 45f, 0f));

        // Evaluate and APPLY the intensity from the animation curve
        if (intensityCurve != null)
        {
            sun.intensity = intensityCurve.Evaluate(normalizedTime);
        }
    }
}