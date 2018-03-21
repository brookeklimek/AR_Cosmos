using UnityEngine;

public class GalaxyCam : MonoBehaviour
{

    public Transform GalaxyPoint;
    public float Distance = 5;
    public float ViewSpeed = 1;
    public float ScrollSpeed;

    public Vector2 LimitY = new Vector2(-20, 80);
    public Vector2 LimitDistance = new Vector2(0.5f, 15);

    private Rigidbody Rbody;

    float _X = 0.0f;
    float _Y = 0.0f;

    private bool isEnable;

    private void Start()
    {
        Rbody = GetComponent<Rigidbody>();

        _X = transform.eulerAngles.y;
        _Y = transform.eulerAngles.x;

        Rbody.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape))
        {

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }
        if (Input.GetKey(KeyCode.Mouse0))
        {

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }

    }

    private void LateUpdate()
    {
        if (GalaxyPoint)
        {
            _X += Input.GetAxis("Mouse X") * ViewSpeed * Distance * 0.002f;
            _Y -= Input.GetAxis("Mouse Y") * ViewSpeed;

            _Y = ClampAngle(_Y, LimitY.x, LimitY.y);

            Quaternion _Rotation = Quaternion.Euler(_Y, _X, 0);

            Distance = Mathf.Clamp(Distance - Input.GetAxis("Mouse ScrollWheel") * ScrollSpeed, LimitDistance.x, LimitDistance.y);

            RaycastHit hit;
            if (Physics.Linecast(GalaxyPoint.position, transform.position, out hit))
            {
                Distance -= hit.distance;
            }

            Vector3 M_Distance = new Vector3(0.0f, 0.0f, -Distance);
            Vector3 _Position = _Rotation * M_Distance + GalaxyPoint.position;

            transform.rotation = _Rotation;
            transform.position = _Position;
        }
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;

        if (angle > 360F)
            angle -= 360F;

        return Mathf.Clamp(angle, min, max);
    }
}