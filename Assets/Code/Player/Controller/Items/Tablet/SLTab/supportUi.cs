using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class supportUi : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;
    public RawImage iconDis;
    Tablet tab;
    public void Setup(string name, int price, Texture icon, Tablet tablet)
    {
        itemName.text = name;
        itemPrice.text = price + "CP";
        iconDis.texture = icon;
        tab = tablet;
    }
    public void Click()
    {
        tab.RequestMenu_Support(transform.GetSiblingIndex());
    }
}
