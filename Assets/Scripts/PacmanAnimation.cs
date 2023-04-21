using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PacmanAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer {get; private set;}
    public Sprite[] sprites;

    public float animationTime = 0.125f;
    public int frame {get; private set;}

    private void Awake() {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {
        InvokeRepeating(nameof(Advance), this.animationTime, this.animationTime);
    }

    private void Advance(){
        if(!this.spriteRenderer.enabled){
            return;
        }
        this.frame ++;
        if(this.frame > this.sprites.Length){
            this.frame = 0;
        }
        if(this.frame >= 0 && this.frame < this.sprites.Length){
            this.spriteRenderer.sprite = this.sprites[this.frame];
        }
    }
}
