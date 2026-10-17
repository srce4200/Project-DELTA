using UnityEngine;
using Photon.Pun;
public class MapCamController : MonoBehaviour
{
    public static MapCamController Instance;
    Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    public void MoveCam(Vector3 offset)
    {
        transform.position += offset * Time.deltaTime * 15 * cam.orthographicSize;
    }
    public void ZoomIn(float upDown)
    {
        cam.orthographicSize -= upDown * 200;
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, 30, 2000);
    }
}
