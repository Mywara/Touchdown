using UnityEngine;
using System.Collections;

public class AnimationRotation : MonoBehaviour
{

    public float vitesseDeRotation;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        this.transform.Rotate(Vector3.up * vitesseDeRotation * Time.deltaTime);

    }
}
