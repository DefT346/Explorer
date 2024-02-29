using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunObservable
{
    [Header("Movement")]
    public float currientMoveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform camera;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;
    public float yRotation;
    public float xRotation;

    public float sens = 1;
    public float smoothing = 0.8f;

    public PhotonView photonView;
    public TMP_Text sitLabel;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        photonView = GetComponent<PhotonView>();

        if (!photonView.IsMine)
        {
            camera.gameObject.SetActive(false);
        }
    }

    private void CameraRayCast()
    {
        int layerMask = 1 << 8;

        layerMask = ~layerMask;

        RaycastHit hit;
        if (Physics.Raycast(camera.transform.position, camera.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask))
        {
            var isCar = hit.transform.tag == "Car";
            sitLabel.gameObject.SetActive(isCar);
            if (isCar)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    var carObject = hit.collider.gameObject;
                    var carController = carObject.GetComponent<CarController>();

                    if (carObject.GetComponent<PhotonView>().IsMine)
                    {
                        DisableWithParent(carObject.transform);
                        photonView.RPC("Disable", RpcTarget.Others, null);
                        carController.Sit(this);
                    }
                    else
                        sitLabel.text = "Машина не ваша";
                }
            }
            else
                sitLabel.text = "Сесть";
        }
        else
            sitLabel.gameObject.SetActive(false);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(active);
        }
        else
        {
            var isActive = (bool)stream.ReceiveNext();
            if (active != isActive)
            {
                if (isActive) Enable(); else DisableWithParent(null);
            }
        }
    }

    private bool active;

    [PunRPC]
    public void Disable()
    {
        DisableWithParent(null);
    }

    public void DisableWithParent(Transform parent)
    {
        active = false;
        this.gameObject.SetActive(false);
        this.transform.parent = parent;
    }

    [PunRPC]
    public void Enable()
    {
        active = true;
        this.gameObject.SetActive(true);
        this.transform.parent = null;
    }

    private void CreateCar()
    {
        GameManager.Instance.SpawnCar(transform.position + transform.forward * 4 + transform.up * 2);
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Cursor.visible = !Cursor.visible;

            if (Input.GetKeyDown(KeyCode.Q))
                PhotonManager.LeaveRoom();

            if (Input.GetKeyDown(KeyCode.K))
                CreateCar();

            CameraRayCast();


            CameraRotate();

            // ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

            MyInput();
            SpeedControl();

            // handle drag
            if (grounded)
                rb.drag = groundDrag;
            else
                rb.drag = 0;
        }
    }

    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            MovePlayer();
        }
    }

    Vector2 smoothV;
    Vector2 mouseLook;
    private void CameraRotate()
    {
        var md = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        md = Vector2.Scale(md, new Vector2(sens * smoothing, sens * smoothing));
        smoothV.x = Mathf.Lerp(smoothV.x, md.x, 1f / smoothing);
        smoothV.y = Mathf.Lerp(smoothV.y, md.y, 1f / smoothing);
        mouseLook += smoothV;
        mouseLook.y = Mathf.Clamp(mouseLook.y, -90f, 90f);

        camera.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(mouseLook.x, Vector3.up);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * currientMoveSpeed * 10f, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * currientMoveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        if (Input.GetKey(sprintKey))
            currientMoveSpeed = sprintSpeed;
        else
            currientMoveSpeed = walkSpeed;

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > currientMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currientMoveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }
}