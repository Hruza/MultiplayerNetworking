using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;

    private void Start()
    {
        Camera.main.transform.parent.GetComponent<CameraFollow>().target = this.transform;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ClientSend.PlayerShoot(MouseWorldPos()-transform.position);
        }
    }

    static public Vector3 MouseWorldPos()
    {
        Plane plane = new Plane(Vector3.forward, Vector3.zero);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        float enter = 0.0f;

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Debug.Log(hit.collider.gameObject);
        }

        if (plane.Raycast(ray, out enter))
        {
            return ray.GetPoint(enter);
        }
        return Vector3.zero;
    }

    private void FixedUpdate()
    {
        SendInputToServer();
    }

    /// <summary>Sends player input to the server.</summary>
    private void SendInputToServer()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
            Input.GetKey(KeyCode.Space)
        };

        ClientSend.PlayerMovement(_inputs);

        Vector2 _inputDirection = Vector2.zero;
        if (_inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (_inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (_inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (_inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
        Vector2 _moveDirection = _inputDirection.normalized;
        _moveDirection *= moveSpeed*Time.fixedDeltaTime;

        transform.position += (Vector3)_moveDirection;
    }
}
