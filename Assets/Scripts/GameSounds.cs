using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSounds : MonoBehaviour
{
    public AudioSource munch1;
    public AudioSource munch2;
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
}
