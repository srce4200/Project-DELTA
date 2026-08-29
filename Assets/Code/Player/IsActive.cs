using UnityEngine;

public class IsActive : MonoBehaviour
{
    private void Start()
    {
        MapInfo.Instance.AddAliveUnit(transform, UnitSide.blufor);
        //MapInfo.Instance.AddAlivePlayer(transform);
    }
    private void OnDestroy()
    {

        MapInfo.Instance.RemoveAliveUnit(transform, UnitSide.blufor);
        //MapInfo.Instance.RemoveAlivePlayer(transform);
    }
}
