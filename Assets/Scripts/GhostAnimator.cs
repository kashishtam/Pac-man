using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostAnimator : MonoBehaviour{
    public Sprite[] sprBody;
    public GameObject objEyes;
    public Sprite[] sprEyes;

    enum Facing {Left, Right, Up, Down};

    float animDelay = 0f;
    int animFrame = 0;
    int flashFrames = 1;
    AIMode currentMode = AIMode.Scatter;
    Color color = Color.green;
    Facing facingDir = Facing.Left;
    SpriteRenderer render;
    SpriteRenderer eyeRender;
    
    // Start is called before the first frame update
    void Start(){
        render = GetComponent<SpriteRenderer>();
        eyeRender = objEyes.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update(){
        if(animDelay > 0){
            animDelay -= Time.deltaTime;
        }else{
            animDelay = 0.25f;
            animFrame += 1;
            if(animFrame > 1){
                animFrame = 0;
            }

            // update sprite
            switch(currentMode){
                case AIMode.Frightened:
                    if(flashFrames > 0){
                        flashFrames -= 1;
                        render.sprite = sprBody[2 + animFrame];
                    }else{
                        if(flashFrames > -1){
                            flashFrames -= 1;
                        }else{
                            flashFrames = 1;
                        }
                        render.sprite = sprBody[4 + animFrame];
                    }
                break;
                default:
                    render.sprite = sprBody[animFrame];
                break;
            }
        }

        // update color
        switch(currentMode){
            case AIMode.Frightened:
                render.color = Color.white;
                eyeRender.enabled = false;
            break;
            case AIMode.Eaten:
                render.enabled = false;
                eyeRender.enabled = true;
            break;
            default:
                render.enabled = true;
                render.color = color;
                eyeRender.enabled = true;
            break;
        }

        // update eyes
        switch(facingDir){
            case Facing.Left: eyeRender.sprite = sprEyes[0]; break;
            case Facing.Right: eyeRender.sprite = sprEyes[1]; break;
            case Facing.Up: eyeRender.sprite = sprEyes[2]; break;
            case Facing.Down: eyeRender.sprite = sprEyes[3]; break;
        }
    }

    public void setFacing(Vector2 direction){
        if(direction.normalized == new Vector2(-1, 0)){
            facingDir = Facing.Left;
        }else if(direction.normalized == new Vector2(1, 0)){
            facingDir = Facing.Right;
        }else if(direction.normalized == new Vector2(0, 1)){
            facingDir = Facing.Up;
        }else{
            facingDir = Facing.Down;
        }
    }

    public void setAIMode(AIMode mode){
        currentMode = mode;
    }

    public void setColor(Color newColor){
        color = newColor;
    }
}
