using Photon.Pun.Demo.Cockpit;
using System.Linq;
using TMPro;
using UnityEngine;

public class ParametersItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI arrtributeName;
    [SerializeField] TMP_InputField input;
    [SerializeField] TMP_Dropdown dropdown;
    parameter pr;
    public void Setup(parameter p)
    {
        pr = p;
        arrtributeName.text = p.paramName;
        ActivateParam((int)p.parameterType);

        switch (p.parameterType)
        {
            case ParameterType.input:
                input.text = p.defaultValues[0];
                break;
            case ParameterType.dropdown:
                foreach(string option in p.defaultValues)
                    dropdown.options.Add(new TMP_Dropdown.OptionData(option));
                break;
            case ParameterType.checkbox:

                break;
        }
    }
    void ActivateParam(int indx)
    {
        if(indx == 0)
        {
            input.gameObject.SetActive(true);
            dropdown.gameObject.SetActive(false);
        }else if(indx == 1)
        {
            input.gameObject.SetActive(false);
            dropdown.gameObject.SetActive(true);
        }
    }
    public parameter ReturnValue()
    {
        if (!dropdown.IsActive())
        {
            pr.defaultValues[0] = input.text;
            return pr;
        }

        pr.selectedValue = dropdown.value;
        return pr;
    }
}
