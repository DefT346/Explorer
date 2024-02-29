using Photon.Pun;
using UnityEngine;

public class CarController : MonoBehaviour, IPunObservable
{
    private float horizontalInput, verticalInput, resetInput;
    private float currentSteerAngle, currentbreakForce;
    private bool isBreaking = true;

    // Settings
    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    // Wheel Colliders
    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    // Wheels
    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    // Camera
    [SerializeField] private Transform camera;
    public Vector3 camOffset = new Vector3(0f, 2f, -4f);
    public float camDist = 10f;

    private bool _active = false;
    public bool active { 
        get
        {
            return _active;
        }
        private set
        {
            _active = value;
            camera.gameObject.SetActive(_active);
        }
    }

    private PlayerController tempPlayer;
    public void Sit(PlayerController player)
    {
        active = true;
        tempPlayer = player;
    }

    public void Exit()
    {
        active = false;
        tempPlayer.Enable();
        isBreaking = true;
    }

    private void Update()
    {
        if (active)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                PhotonManager.LeaveRoom();

            if (Input.GetKeyDown(KeyCode.R))
                ResetCar();

            if (Input.GetKeyDown(KeyCode.F))
                Exit();
        }
    }

    private void FixedUpdate()
    {
        if (active)
        {
            GetInput();
            CameraRotation();
        }

        HandleMotor();
        HandleSteering();
        UpdateWheels();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.SendNext(currentSteerAngle);
            //stream.SendNext(currentbreakForce);
            stream.SendNext(isBreaking);
        }
        else
        {
            isBreaking = (bool)stream.ReceiveNext();
            //currentbreakForce = (float)stream.ReceiveNext();
            //currentSteerAngle = (float)stream.ReceiveNext();
        }
    }

    private void CameraRotation()
    {

        var targetPosition = transform.TransformPoint(camOffset);
        camera.transform.position = Vector3.Lerp(transform.position, targetPosition, camDist * Time.deltaTime);

        var direction = transform.position - camera.transform.position;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        camera.transform.rotation = Quaternion.Lerp(transform.rotation, rotation, camDist * Time.deltaTime);

    }

    private void GetInput()
    {
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        isBreaking = Input.GetKey(KeyCode.Space);
    }

    private void ResetCar()
    {
        transform.rotation = Quaternion.identity;
        transform.position += transform.up * 3f;
    }

    private void HandleMotor()
    {
        frontLeftWheelCollider.motorTorque = verticalInput * motorForce;
        frontRightWheelCollider.motorTorque = verticalInput * motorForce;
        currentbreakForce = isBreaking ? breakForce : 0f;
        ApplyBreaking();
    }

    private void ApplyBreaking()
    {
        frontRightWheelCollider.brakeTorque = currentbreakForce;
        frontLeftWheelCollider.brakeTorque = currentbreakForce;
        rearLeftWheelCollider.brakeTorque = currentbreakForce;
        rearRightWheelCollider.brakeTorque = currentbreakForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        frontLeftWheelCollider.steerAngle = currentSteerAngle;
        frontRightWheelCollider.steerAngle = currentSteerAngle;
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }
}
