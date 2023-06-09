using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacman : MazeEntity{
    public float speed = 3f;
    public float speedMultiplier = 1f;
    public Vector2 initialDirection;
    public Vector2 nextDirection { get; private set; }
    public Vector3 startingPosition { get; private set; }

    protected new Rigidbody2D rigidbody { get; private set; }
    protected Vector2 direction { get; private set; }
    GameManager parent;
    PacmanAnimation animator;
    private bool stopRotating = false;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<PacmanAnimation>();
        startingPosition = transform.position;
        direction = initialDirection;

        rigidbody.isKinematic = true;
    }

    private void Update() {
        if(!stopRotating){
            // Rotate pacman to face the movement direction
            float angle = Mathf.Atan2(direction.y, direction.x);
            transform.rotation = Quaternion.AngleAxis(angle * Mathf.Rad2Deg, Vector3.forward);
        }
    }

    public override void move(){
        // Set the new direction based on the current input
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            SetDirection(Vector2.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            SetDirection(Vector2.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            SetDirection(Vector2.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            SetDirection(Vector2.right);
        }

        // Try to move in the next direction while it's queued to make movements
        if (nextDirection != Vector2.zero) {
            SetDirection(nextDirection);
        }

        float DTSpeed = (speed * speedMultiplier * Time.deltaTime);
        if(!checkCollision(direction, DTSpeed)){
            transform.position = (Vector2)transform.position + (direction * DTSpeed);
        }
    }

    public void SetDirection(Vector2 direction, bool forced = false)
    {
        // Only set the direction if the tile in that direction is available
        // otherwise we set it as the next direction
        if (forced || !checkCollision(direction))
        {
            this.direction = direction;
            nextDirection = Vector2.zero;
        }
        else
        {
            nextDirection = direction;
        }
    }

    public Vector2 getDirection(){
        return direction;
    }

    public override void setParent(GameManager parent){
        this.parent = parent;
    }

    public override void reset(){
        transform.position = startingPosition;
        animator.setAnimation(PacmanAnimation.Animation.Move);
        stopRotating = false;
    }

    public void die(){
        animator.setAnimation(PacmanAnimation.Animation.Death);
        stopRotating = true;
    }
}
