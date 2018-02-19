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
    public static int GRAPE_DAMAGE = 10;
    //3 unités de profondeurs - enfin peut toucher trois ennemis 
    //alignés et qui sont collés les uns aux autres
    public static float GRAPE_RADIUS = 35;
    //angle du cône d'attaque du tir de mitraille
    //angle calculé du centre vers une extrémité du cône
    public static float GRAPE_ANGLE = 35;
}
