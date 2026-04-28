using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public Transform target;
    public void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
    }
}
