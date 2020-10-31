using UnityEngine;

public class SimpleOrthoCameraController : MonoBehaviour {

    private float moveSpeed = 0.5f;
    private float scrollSpeed = 10f;

    private Camera _camera;

    void Start()
    {
        _camera = Camera.main;
        _camera.orthographic = true;
    }

    void Update () {
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0) {
            transform.position += moveSpeed * new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
        }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            _camera.orthographicSize += scrollSpeed * -Input.GetAxis("Mouse ScrollWheel");
        }
    }

}