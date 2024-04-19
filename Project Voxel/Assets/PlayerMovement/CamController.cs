using System.Collections;
using UnityEngine;

public class CamController : MonoBehaviour
{
    public float moveSpeed;
    [Range(0,1)]
    public float smoothTime;

    public Transform playerTransform;

    [HideInInspector]
    public int worldSize;

    private float orthoSize;
    public void Spawn(Vector3 pos) 
    {
        GetComponent<Transform>().position = pos;
        orthoSize = GetComponent<Camera>().orthographicSize;
    }

    public void FixedUpdate()
    {
        Vector3 pos = GetComponent<Transform>().position;

        //Control how fast camera moves with player on x and y axis
        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, playerTransform.position.y, smoothTime);

        //OrthoSize determines how close to edge of the world the camera will clamp. 
        //This prevents camera from going off the playable map 
        pos.x = Mathf.Clamp(pos.x, 0 + (orthoSize*2.5f), worldSize - (orthoSize*2.5f));
        GetComponent<Transform>().position = pos;
    }
}
