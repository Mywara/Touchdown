using System.Collections;
using System.Collections.Generic;
using DigitalRuby.SoundManagerNamespace;

using UnityEngine;
using UnityEngine.UI;


public class BGMManager : MonoBehaviour
{

    public AudioSource[] MusicAudioSources;


    private void PlayMusic(int index)
    {
        MusicAudioSources[index]
            .PlayLoopingMusicManaged(0.1f, 0f, true);
    }

    // Use this for initialization
    void Awake()
    {
        
        if (SceneManagerHelper.ActiveSceneName == "RandomMap")
            PlayMusic(1);
        else if(SceneManagerHelper.ActiveSceneName == "MainMenu")
            PlayMusic(0);
            
    }

}
