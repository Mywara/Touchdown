using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    // Vistesse de la camera
    public float CameraMoveSpeed = 120.0f;

    // L'objet a suivre
    public GameObject CameraFollowObj;

    // La position a suivre
    Vector3 FollowPOS;

    // Limite de l'angle de la camera
    public float minClampAngle = 10.0f;
    public float maxClampAngle = 20.0f;

    // La sensitivité de la camera
    public float inputSensitivity = 150.0f;

    // La camera
   //useless ? public GameObject CameraObj;
   //useless ? public GameObject PlayerObj;
    public float camDistanceXToPlayer;
    public float camDistanceYToPlayer;
    public float camDistanceZToPlayer;
    public float mouseX;
    public float mouseY;
    public float finalInputX;
    public float finalInputZ;

    public float smoothX;
    public float smoothY;
    public float rotX = 0.0f;
    public float rotY = 0.0f;
    [HideInInspector]
    public bool inGame = true;



    // Use this for initialization
    void Start () {

        Vector3 rot = transform.localRotation.eulerAngles;
        rotX = rot.x;
        rotY = rot.y;
        // Fige le curseur 
        Cursor.lockState = CursorLockMode.Locked;
        // Rend le curseur invisible
        Cursor.visible = false;
        


    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.L))
        {
            inGame = !inGame;
            //vue intergame
            Quaternion localRotation = Quaternion.Euler(90, 0, 0.0f);
            transform.rotation = localRotation;
            transform.position = new Vector3(0, 10, 0);
            // deFige le curseur 
            Cursor.lockState = CursorLockMode.None;
            // Rend le curseur visible
            Cursor.visible = true;

        }

        if (inGame) { 
        // Si on a quitté le jeu (Alt+Tab par exemple) et qu'on revient dessus on rebloque le curseur.
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            // Fige le curseur 
            Cursor.lockState = CursorLockMode.Locked;
            // Rend le curseur invisible
            Cursor.visible = false;
        }

        // On met en place la rotation du stick
        float inputX = Input.GetAxis("RightStickHorizontal");
        float inputZ = Input.GetAxis("RightStickVertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        finalInputX = inputX + mouseX;
        finalInputZ = inputZ + mouseY;

        rotY += finalInputX * inputSensitivity * Time.deltaTime;
        rotX += finalInputZ * inputSensitivity * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -minClampAngle, maxClampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        if(CameraFollowObj != null)
        {
            CameraFollowObj.transform.root.rotation = Quaternion.Euler(0.0f, rotY, 0.0f);
        }
        transform.rotation = localRotation;
        }
        else 
        {
            Vector3 MoveCamera = transform.position;
            if (Input.GetKey(KeyCode.D))
            {
                MoveCamera.x += 0.1f;
               
            }
            if (Input.GetKey(KeyCode.Q))
            {
                MoveCamera.x -= 0.1f;
            }
            if (Input.GetKey(KeyCode.Z))
            {
                MoveCamera.z += 0.1f;
            }
            if (Input.GetKey(KeyCode.S))
            {
                MoveCamera.z -= 0.1f;

            }
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
            {
                MoveCamera.y -= 0.1f;
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
            {
                MoveCamera.y += 0.1f;
            }
            transform.position = MoveCamera;

        }
    }

    void LateUpdate ()
    {
        if (inGame)
        {
            CameraUpdater();
        }
    }

    void CameraUpdater()
    {
        if(CameraFollowObj != null)
        {
            // Met en place la cible à suivre
            Transform target = CameraFollowObj.transform;

            // Se déplace vers la cible
            float step = CameraMoveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        } 
    }

    public void SetObjectToFollow(GameObject objToFollow)
    {
        this.CameraFollowObj = objToFollow;
       
    }

}
