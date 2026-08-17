using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionSettings : MonoBehaviour
{
    public static MissionSettings Instance;
    [SerializeField] Slider timeSlider;
    [SerializeField] TMP_InputField ticketInp;
    private void Awake()
    {
        Instance = this;
    }
    public void Apply()
    {
        DayNightCycle.Instance.SetTime(timeSlider.value);
    }
    public int GetTickets()
    {
        return int.Parse(ticketInp.text);
    }
}
