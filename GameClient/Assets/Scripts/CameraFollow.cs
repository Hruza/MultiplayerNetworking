using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public float followSpeed = 0.8f;
    public int averageCount=5;
    /// <summary>
    /// cíl, který bude následován
    /// </summary>
    public Transform target;

    // Use this for initialization
    void Start()
    {
        positions = new Queue<Vector3>();
    }

    private Vector3 velocity = Vector3.zero;
    public void FixedUpdate()
    {
        if (target == null) return;
        transform.position = Vector3.Lerp(transform.position, target.position,followSpeed);
        //  transform.position = Vector3.SmoothDamp(transform.position, TargetPos(), ref velocity, followSpeed);
    }


    Queue<Vector3> positions;

    private Vector3 TargetPos() {
        Vector3 output=Vector3.zero;
        positions.Enqueue(target.position);

        if (positions.Count > averageCount) {
            positions.Dequeue();
        }
        foreach (Vector3 vector in positions)
        {
            output += vector;
        }
        Debug.Log(positions.Count);
        return output /positions.Count;
    
    }
}
