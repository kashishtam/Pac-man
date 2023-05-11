using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour, Edible
{
    public int points = 10;
    public void Eat(){
        FindObjectOfType<GameManager>().PelletEaten(this);
    }

    public void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Pacman")){
            Eat();
        }
    }
}
