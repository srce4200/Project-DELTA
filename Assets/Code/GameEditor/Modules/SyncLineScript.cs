using System.Collections;
using UnityEngine;

public class SyncLineScript : MonoBehaviour
{
    public void CheckLine(Transform pos1, Transform pos2)
    {
        StartCoroutine(UpdateDisplayLine(pos1, pos2));
    }
    IEnumerator UpdateDisplayLine(Transform pos1, Transform pos2)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            if (pos1 == null || pos2 == null)
            {
                Destroy(gameObject);
            }
            else
            {
                GetComponent<LineRenderer>().SetPosition(0, pos1.position);
                GetComponent<LineRenderer>().SetPosition(1, pos2.position);
            }
        }
    }
}
