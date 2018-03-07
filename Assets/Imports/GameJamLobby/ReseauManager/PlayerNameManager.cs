using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class PlayerNameManager : MonoBehaviour {

    static string playerNamePrefKey = "PlayerName";
   
    void Start()
    {
        string defaultName = "";
        InputField  _inputField = this.GetComponentInChildren<InputField>();
        if (_inputField != null)
        {
            if (PlayerPrefs.HasKey(playerNamePrefKey))
            {
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                if (!defaultName.Equals(""))
                {
                    _inputField.GetComponentInChildren<Text>().text = "Name : " + defaultName;
                }
            }
        }

        PhotonNetwork.playerName = defaultName;
    }

    public void SetPlayerName(string value)
    {
        // #Important
        PhotonNetwork.playerName = value + " "; // force a trailing space string in case value is an empty string, else playerName would not be updated.


        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
