using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionSettings : MonoBehaviour
{
    [SerializeField] Slider timeSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Apply()
    {
        DayNightCycle.Instance.SetTime(timeSlider.value);
    }
}
