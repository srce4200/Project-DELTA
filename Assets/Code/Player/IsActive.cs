using UnityEngine;

public class IsActive : MonoBehaviour
{
    private void Start()
    {
        MapInfo.Instance.AddAlivePlayer(transform);
    }
    private void OnDestroy()
    {
        MapInfo.Instance.RemoveAlivePlayer(transform);
    }
}
