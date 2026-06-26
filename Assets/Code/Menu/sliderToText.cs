using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class sliderToText : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        text.text = (slider.value).ToString();
    }
}
