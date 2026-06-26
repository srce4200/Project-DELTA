using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ControlsSettings : MonoBehaviour
{
    [SerializeField] bool initialized;
    [SerializeField] Slider sensitivitySlider;

    [Header("Controls Keybind")]
    public KeyBindingManager keyBindingManager;

    // UI elements
    public Button interactButton; 
    // Add more UI elements as needed

    private KeyBindingManager.Action currentAction;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity");
        }
        initialized = true;
         
        // Load key bindings
        LoadKeyBindings();
    }
    public void SetMouseSensitivity(float sensitivity)
    {
        if (!initialized) return;
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
    }

    #region keys manager

    public void OnInteractButtonClicked(TextMeshProUGUI buttonText)
    {
        currentAction = KeyBindingManager.Action.interact;
        StartCoroutine(WaitForKeyPress(buttonText)); 
    } 

    private IEnumerator WaitForKeyPress(TextMeshProUGUI keyText)
    {
        while (!Input.anyKeyDown)
            yield return null;

        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                keyBindingManager.SetKeyBinding(currentAction, keyCode);
                keyText.SetText(keyCode.ToString());
                break;
            }
        }
    }

    public void SaveKeyBindings()
    {
        foreach (KeyBindingManager.Action action in System.Enum.GetValues(typeof(KeyBindingManager.Action)))
        {
            PlayerPrefs.SetInt(action.ToString(), (int)keyBindingManager.GetKeyBinding(action));
        }
        PlayerPrefs.Save();
    }

    public void LoadKeyBindings()
    {
        foreach (KeyBindingManager.Action action in System.Enum.GetValues(typeof(KeyBindingManager.Action)))
        {
            KeyCode keyCode = (KeyCode)PlayerPrefs.GetInt(action.ToString(), (int)keyBindingManager.GetKeyBinding(action));
            keyBindingManager.SetKeyBinding(action, keyCode);
        }
    }

    #endregion
}
