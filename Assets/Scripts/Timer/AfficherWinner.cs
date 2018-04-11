using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AfficherWinner : MonoBehaviour {

    public AudioClip winClip;
    public AudioClip looseClip;

	// Use this for initialization
	void Start ()
    {
        Winner winnerManager = GameObject.Find("WinnerManager").GetComponent<Winner>();
        Image background = transform.GetComponentInChildren<Image>();
        AudioSource audioSource = GameObject.Find("EndCrystal").GetComponent<AudioSource>();

        // si égalité
        if (winnerManager.tie)
        {
            background.color = new Color(1f, 0f, 0f, .7f); // red
            audioSource.clip = looseClip;
            this.transform.GetComponentInChildren<Text>().text = "It's a tie! Nobody wins...\nMaybe next time ?";
        }
        // si le joueur local a gagné
        else if (winnerManager.hasWon)
        {
            background.color = new Color(.3f, 1f, .3f, .7f); // greenish
            audioSource.clip = winClip;
            this.transform.GetComponentInChildren<Text>().text = winnerManager.winner + " wins !\nCongratulations !";
        }
        // s'il a perdu
        else
        {
            background.color = new Color(1f, 0f, 0f, .7f); // red
            audioSource.clip = looseClip;
            this.transform.GetComponentInChildren<Text>().text = winnerManager.winner + " wins...\nMaybe next time ?";
        }

        audioSource.Play(1);
    }
}
