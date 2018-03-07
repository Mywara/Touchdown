using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PGlaceEffect : Photon.PunBehaviour {

    // Use this for initialization
    public float duration = 10;
    private Animator anim;

    void Start () {

        
    }

    private void Awake()
    {
        StartCoroutine(StopEffect(duration));
    }

    // Update is called once per frame
    void Update () {
        
    }

    IEnumerator StopEffect(float timeEffect)
    {
        while (timeEffect > 0)
        {
            timeEffect -= Time.deltaTime;
            if (timeEffect <= 0)
            {
                photonView.RPC("DestroyEffect", PhotonTargets.All);
            }
            yield return null;
        }

    }

    [PunRPC]
    void DestroyEffect()
    {
        Destroy(this.transform.gameObject);
    }

    public void OnTriggerEnter(Collider other)
    {
        //immobilise
        if(other.transform.tag == "Player") {
            other.transform.GetComponent<PlayerController>().immobilization = true;
            other.transform.GetComponent<PlayerController>().activeTrap = this.transform;
            ApplyDamage(other.transform.gameObject, 10);
           
            }
    }

    public void OnTriggerStay(Collider other)
    {
        //stop les animations de déplacement
        anim = other.transform.GetComponent<Animator>();
        anim.SetBool("RunningForward", false);
        anim.SetBool("RunningLeft", false);
        anim.SetBool("RunningRight", false);
    }

    private void ApplyDamage(GameObject target, int damage)
    {
        PUNTutorial.HealthScript healthScript = target.GetComponent<PUNTutorial.HealthScript>();
        if (healthScript != null)
        {
            healthScript.photonView.RPC("Damage", PhotonTargets.All, damage);
        }

        PUNTutorial.HealthScript2 healthScript2 = target.GetComponent<PUNTutorial.HealthScript2>();
        if (healthScript2 != null)
        {
            healthScript2.photonView.RPC("Damage2", PhotonTargets.All, damage);
        }
    }
}
