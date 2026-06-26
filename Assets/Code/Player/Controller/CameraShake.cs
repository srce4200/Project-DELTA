using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{ 
    bool shaking;
    public static CameraShake Instance;

    #region positional shake

    void Awake()
    {
        if (!transform.root.GetComponent<PhotonView>().IsMine)
        {
            Destroy(this);
        }
        Instance = this;
    }

    public void Shake(float duration, float magnitude)
    {
        if (shaking == false)
        {
            StartCoroutine(Execute(duration, magnitude));
        }
    }
    IEnumerator Execute(float duration, float magnitude)
    {
        shaking = true;

        Vector3 orignalPosition = transform.transform.localPosition;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            gameObject.transform.localPosition = new Vector3(orignalPosition.x + x, orignalPosition.y + y, orignalPosition.z);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        gameObject.transform.localPosition = orignalPosition;

        shaking = false;
    }
    #endregion
}
