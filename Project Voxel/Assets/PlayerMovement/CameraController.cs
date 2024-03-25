using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed;
    [Range(0,1)]
    public float smoothTime;

    public Transform playerTransform;

    public void FixedUpdate()
    {
        Vector3 pos = GetComponent<Transform>().position;

        //Control how fast camera moves with player on x and y axis
        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, playerTransform.position.y, smoothTime);


        GetComponent<Transform>().position = pos;
    }
}
