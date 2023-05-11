using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PacmanAnimation : MonoBehaviour
{
    public enum Animation { Move, Death };

    public SpriteRenderer spriteRenderer {get; private set;}
    public Sprite[] moveAnim;
    public Sprite[] deathAnim;

    public float animationTime = 0.125f;
    public int frame {get; private set;}
    private Animation currentAnim = Animation.Move;
    private Sprite[] sprites;

    private void Awake() {
        this.spriteRenderer = GetComponent<SpriteRenderer>();
        sprites = moveAnim;
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
            if(currentAnim == Animation.Death){
                this.frame = this.sprites.Length - 1;
            }else{
                this.frame = 0;
            }
        }
        if(this.frame >= 0 && this.frame < this.sprites.Length){
            this.spriteRenderer.sprite = this.sprites[this.frame];
        }

        if(currentAnim == Animation.Death){
            transform.rotation = Quaternion.identity;
        }
    }

    public void setAnimation(Animation animation){
        this.currentAnim = animation;
        this.frame = 0;
        switch(animation){
            case Animation.Move:
            this.sprites = moveAnim;
            break;

            case Animation.Death:
            this.sprites = deathAnim;
            break;
        }
    }
}
