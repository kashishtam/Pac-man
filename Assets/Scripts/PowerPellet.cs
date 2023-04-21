using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPellet : MonoBehaviour, Edible
{
    public int points = 50;
    
    public float duration = 8.0f;

    public void Eat(){
        FindObjectOfType<GameManager>().PowerPelletEaten(this);
    }

    public void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.layer == LayerMask.NameToLayer("Pacman")){
            Eat();
        }
    }
}
