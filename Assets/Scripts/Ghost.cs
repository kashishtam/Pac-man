using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MazeEntity, Observer {
    static float moveSpeed = 4;
    public static Ghost[] team = new Ghost[4];
    public LayerMask pacmanLayer;

    public int points = 100;
    GameObject moveTarget;
    AIPattern currentPattern;
    Transform trans;
    GameObject currentNode;
    GhostAnimator anim;
    Vector2 colliderSize;
    GameManager parent;
    
    public void Start(){
        trans = gameObject.GetComponent<Transform>();
        anim = gameObject.GetComponent<GhostAnimator>();
        colliderSize = gameObject.GetComponent<BoxCollider2D>().size;
    }

    public void init(GameObject jailNode){
        currentNode = jailNode;
        moveTarget = jailNode;
    }
    
    public override void move(){
        // move to target position (move first, decide later)
        float x_dist = (trans.position[0] - moveTarget.transform.position[0]);
        float y_dist = (trans.position[1] - moveTarget.transform.position[1]);
        float moveSpeedDT = (moveSpeed * Time.deltaTime);
        float modifiedSpeed = moveSpeed;
        if(currentPattern.getAIMode() == AIMode.Eaten){
            modifiedSpeed *= 2;
        }
        stepTowardsTarget(trans, moveTarget.transform.position, modifiedSpeed);

        // update animator
        anim.setAIMode(currentPattern.getAIMode());
        anim.setFacing(new Vector2(-x_dist, -y_dist));

        // check for pacman collision
        Collider2D result = Physics2D.OverlapBox(transform.position, colliderSize, 0f, pacmanLayer);
        if(result != null && result.gameObject == parent.pacman.gameObject){
            if(currentPattern.getAIMode() != AIMode.Frightened && currentPattern.getAIMode() != AIMode.Eaten){
                parent.PacmanEaten();
            }else{
                currentPattern.setAIMode(AIMode.Eaten);
                parent.GhostEaten(this);
            }
        }

        // decide on a new target
        if(moveTarget == null || (Mathf.Abs(x_dist) < moveSpeedDT && Mathf.Abs(y_dist) < moveSpeedDT)){
            // exit eaten mode
            if(moveTarget == currentPattern.getJailNode() && currentPattern.getAIMode() == AIMode.Eaten){
                currentPattern.setAIMode(AIMode.Scatter);
            }

            currentNode = moveTarget;
            GameObject[] moves = base.getMoves(currentNode);
            moveTarget = currentPattern.getNextMove(moves);
        }
    }

    public void eventUpdate(Event newEvent){
        switch(newEvent.getEventType()){
            case Event.eventType.PowerPellet:
                AIMode currentMode = currentPattern.getAIMode();
                if(currentMode != AIMode.Eaten){
                    if(newEvent.pelletIsActive()){
                        currentPattern.setAIMode(AIMode.Frightened);
                    }else{
                        currentPattern.setAIMode(AIMode.Chase);
                    }
                }
            break;

            case Event.eventType.AIModeChange:
                if(currentPattern.getAIMode() != AIMode.Frightened && currentPattern.getAIMode() != AIMode.Eaten){
                    currentPattern.setAIMode(newEvent.getNewMode());
                }
            break;

            case Event.eventType.Panic:
                if(currentPattern.getName() == "Blinky"){
                    ((BlinkyPattern)currentPattern).panic = true;
                }
            break;
        }
    }

    public void setAIPattern(AIPattern newPattern){
        currentPattern = newPattern;
        currentPattern.setParent(this);
        anim.setColor(currentPattern.getColor());
    }

    public AIPattern getAIPattern(){
        return currentPattern;
    }

    public override void setParent(GameManager parent){
        this.parent = parent;
    }

    public override void reset(){
        transform.position = currentPattern.getJailNode().transform.position;
        moveTarget = currentPattern.getJailNode();
    }
}
