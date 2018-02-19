 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//attaque large peu profonde
//repousse les ennemis et fait peu de dégâts
public class GrapeShot : DistanceHitZone
{
    public void Start()
    {
        damage = Constants.GRAPE_DAMAGE;
        radius = Constants.GRAPE_RADIUS;
        angle  = Constants.GRAPE_ANGLE;
    }

    //fonction pour le repoussement des ennemis
    public void repulse(GameObject target)
    {
        //direction de la cible
        Vector3 targetDir = target.transform.position - transform.position;
        //angle entre la cible et le point de visée du joueur
        float angle = Vector3.Angle(targetDir, transform.forward);

        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);

        target.transform.SetPositionAndRotation(transform.position + q * Vector3.right * radius, q);
    }

    /*
     * Et là genre t'as le OnTriggerStay qui prend toutes les instances
     * Et puis le Update qui applique les dégats et l'effet de recul
     * Mais je sais pas faire un GO pour chaque attaque on va vite se retrouver
     * avec des tas et des tas de GO qu'il faut instancier et libérer à chaque
     * lancer ça va bouffer tellement le jeu il va lag avec tous ces objets 
     * sur la scène imagine tout le monde lance un sort en même temps ça va être full 
     * instanciation
     */
}
