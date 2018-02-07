using UnityEngine;
using System.Collections;

public class PlayerController : Photon.PunBehaviour
{

    public float movementSpeed = 10;
    public float turningSpeed = 60;
    public float rotY = 0.0f;
    public float mouseX;
    public float finalInputX;
    public float inputSensitivity = 150.0f;
    public GameObject cameraFollow;

    private Rigidbody rb;
    private float VerticalVelocity;
    public float jumpForce = 10.0f;
    private bool isGrounded = true;



    void Start ()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rb = GetComponent<Rigidbody>();
        if(photonView.isMine)
        {
            GameObject cameraPrefab = Camera.main.transform.root.gameObject;
            if(cameraPrefab != null)
            {
                CameraFollow cameraFollowScript = cameraPrefab.GetComponent<CameraFollow>();
                if(cameraFollowScript != null)
                {
                    cameraFollowScript.SetObjectToFollow(cameraFollow);
                }
                else
                {
                    Debug.Log("Main camera found, but don't have CameraFollow script");
                }
            }
            else
            {
                Debug.Log("No main Camera / no CameraPrefab");
            }
        }
    }


    void FixedUpdate()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        // saut perso
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
        }
    }


    void Update()
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        // translation perso
        float horizontal = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
        transform.Translate(horizontal, 0, 0);

        float vertical = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;
        transform.Translate(0, 0, vertical);

        // rotation perso
        float inputX = Input.GetAxis("RightStickHorizontal");
        mouseX = Input.GetAxis("Mouse X");
        finalInputX = inputX + mouseX;

        rotY += finalInputX * inputSensitivity * Time.deltaTime;
        Quaternion localRotation = Quaternion.Euler(0.0f, rotY, 0.0f);
        transform.rotation = localRotation;
        
    }

    void OnCollisionEnter(Collision other)
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        if (other.gameObject.tag == "Floor" && isGrounded == false)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision other)
    {
        if (!photonView.isMine && PhotonNetwork.connected == true)
        {
            return;
        }
        if (other.gameObject.tag == "Floor" && isGrounded == true)
        {
            isGrounded = false;
        }
    }

}
