using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideInEditor : MonoBehaviour
{
    public bool hide = false;
    private void OnDrawGizmos()
    {
        if(hide)
            gameObject.SetActive(false);
    }
}
