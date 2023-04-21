using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MazeEntity : MonoBehaviour {
    public LayerMask obstacleLayer;

    private static Vector2[] checkOffsets = {
        new Vector2(-2.5f, 0f),
        new Vector2(2.5f, 0f),
        new Vector2(0f, -2.5f),
        new Vector2(0f, 2.5f)
    };

    public MazeEntity(){}

    protected GameObject[] getMoves(GameObject current){
        NavNode currentNode = current.GetComponent<NavNode>();
        return currentNode.adjacent.ToArray();
    }

    protected void stepTowardsTarget(Transform movingObject, Vector2 targetPos, float moveSpeed){
        float x_dist = (movingObject.position[0] - targetPos[0]);
        float y_dist = (movingObject.position[1] - targetPos[1]);
        float[] new_pos = {0f, 0f};
        float moveSpeedDT = (moveSpeed * Time.deltaTime);
        
        if(x_dist >= moveSpeedDT){
            new_pos[0] -= moveSpeedDT;
        }else if(x_dist <= -moveSpeedDT){
            new_pos[0] += moveSpeedDT;
        }else{
            new_pos[0] -= x_dist;
        }

        if(y_dist >= moveSpeedDT){
            new_pos[1] -= moveSpeedDT;
        }else if(y_dist <= -moveSpeedDT){
            new_pos[1] += moveSpeedDT;
        }else{
            new_pos[1] -= y_dist;
        }

        movingObject.position = (movingObject.position + (Vector3.right * new_pos[0]) + (Vector3.up * new_pos[1]));
    }

    public bool checkCollision(Vector2 direction){
        // If no collider is hit then there is no obstacle in that direction
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.75f, 0f, direction, 1.5f, obstacleLayer);
        return hit.collider != null;
    }

    public abstract void move();
    public abstract void setParent(GameManager parent);
    public abstract void reset();
}
