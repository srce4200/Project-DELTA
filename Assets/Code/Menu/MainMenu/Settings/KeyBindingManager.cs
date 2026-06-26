using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBindingManager : MonoBehaviour
{ 
    public enum Action
    {
        interact  
    } 
    private Dictionary<Action, KeyCode> keyBindings = new Dictionary<Action, KeyCode>();

    public void SetKeyBinding(Action action, KeyCode keyCode)
    {
        keyBindings[action] = keyCode;
    }

    public KeyCode GetKeyBinding(Action action)
    {
        if (keyBindings.ContainsKey(action))
            return keyBindings[action];
        else
            return KeyCode.None; // Or any default key code you prefer
    }
}
