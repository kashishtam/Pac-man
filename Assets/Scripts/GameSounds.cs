using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSounds : MonoBehaviour
{
    public AudioSource munch1;
    public AudioSource munch2;
    public AudioSource death;
    public AudioSource ghostEaten;
    public AudioSource powerPellet;
    private int currentMunch = 0;

    public void playMunchSound(){
        if(currentMunch == 0){
            munch1.Play();
            currentMunch = 1;
        }else{
            munch2.Play();
            currentMunch = 0;
        }
    }

    public void playDeathSound(){
        death.Play();
    }
    public void playGhostEatenSound(){
        ghostEaten.Play();
    }
    public void playPowerPelletSound(float duration){
        powerPellet.Play();
        powerPellet.SetScheduledEndTime(AudioSettings.dspTime+(duration));
    }
}
