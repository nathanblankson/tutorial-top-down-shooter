using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerController))]
public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;

    private PlayerController _playerController;
    private Camera _camera;

    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        _camera = Camera.main;
    }

    private void Update()
    {
        // Move
        float inputHorizontal = Input.GetAxisRaw("Horizontal");
        float inputVertical = Input.GetAxisRaw("Vertical");
        Vector3 moveInput = new Vector3(inputHorizontal, 0, inputVertical);
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        _playerController.Move(moveVelocity);

        // Look At
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position); // using transform.position to prevent looking up/down
        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            // Debug.DrawLine(ray.origin, point, Color.red);
            _playerController.LookAt(point);
        }
    }

}
