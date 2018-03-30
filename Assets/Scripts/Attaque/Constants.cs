using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class contains all static values used in the game
 */
public static class Constants : object {

    /*
     * Static values associated to the GrapheShot
     */
    public static int GRAPE_DAMAGE = 120;
    //3 unités de profondeurs - enfin peut toucher trois ennemis 
    //alignés et qui sont collés les uns aux autres
    public static float GRAPE_RADIUS = 5;
    //angle du cône d'attaque du tir de mitraille
    //angle calculé du centre vers une extrémité du cône
    public static float GRAPE_ANGLE = 2;

    /*
     * Static values associated to the Pirate's passive
     */
    // nombres d'auto-attaques nécessaires au déclenchement du passif
    public static int PIRATE_HITSTACK = 4;
    // taux de critique du passif
    public static float PIRATE_CRITICAL_RATE = 1.2f;

    /*
     * Static values associated to the Undead's passive
     */
    // taux de PV drainés par le vol de vie
    public static float UNDEAD_LEECHLIFE_RATE = .35f;

    /*
     * Static values associated to the CurseDoT
     */
    //durée en secondes de la malédiction
    public static float CURSEDOT_DURATION = 4f;
    public static int CURSEDOT_DAMAGE = 35;
    //Indique la range du projectile
    public static float CURSEDOT_RADIUS = 12;
    //nombre de tics appliqué à la cible
    public static float CURSEDOT_TIC = 7;
    //nombre de cible maximum pouvant être touchées
    public static float CURSEDOT_TARGETS = 5;

    /*
     * Static values associated to the Warbear's passive
     */
    // nombres d'auto-attaques nécessaires au déclenchement du passif
    public static int WARBEAR_HITSTACK = 5;
    // durée du stun occasionné
    public static float WARBEAR_STUN_DURATION = 1f;
    // laps de temps après une auto-attauqe au bout duquel la hit stack est reset à 0
    public static float WARBEAR_TIME_BEFORE_STACK_RESET = 5f; // à mettre en place ?

    /*
     * Static values associated to the ShoulderShot
     */
    public static int SHOULDER_DAMAGE = 200;
    public static float SHOULDER_STUN_TIMER = 1.5f;
}
