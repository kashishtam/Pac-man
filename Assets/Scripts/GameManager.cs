using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour, Subject
{
    public GameObject ghostPrefab;
    public GameObject jailNode;
    List<Observer> subscribers;
    List<MazeEntity> mazeEntities;
    List<Transform> mazeEatables;
    AIPool pool;

    public Pacman pacman;
    public int score {get; private set;}
    public int ghostMultiplier{ get; private set;} = 1;
    public int lives{get; private set;} = 3;

    public GameSounds gameSound;

    float AIModeSwapTimer = 0;
    int AIModeSwapIndex = 0;
    float powerPelletTimer = 0f;
    bool powerPelletActive = false;

    void Start(){
        subscribers = new List<Observer>();
        mazeEntities = new List<MazeEntity>();

        // get pacman
        pacman = GameObject.Find("Pacman").GetComponent<Pacman>();
        pacman.setParent(this);
        mazeEntities.Add(pacman);

        // get edibles
        GameObject[] edibles = GameObject.FindGameObjectsWithTag("Edible");
        mazeEatables = new List<Transform>();
        foreach(GameObject edible in edibles){
            mazeEatables.Add(edible.GetComponent<Transform>());
        }

        // initialize AI pool
        pool = new AIPool(pacman.gameObject, jailNode);

        // get ghosts
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        int ghostIndex = 0;
        foreach(GameObject ghostObj in ghosts){
            Ghost ghost = ghostObj.GetComponent<Ghost>();
            ghost.init(jailNode);

            AIPattern pattern = pool.getRandom();
            pattern.setAIMode(AIMode.Chase);
            ghost.setAIPattern(pattern);

            ghost.setParent(this);
            subscribe(ghost);
            mazeEntities.Add(ghost);
            Ghost.team[ghostIndex] = ghost;
            ghostIndex += 1;
        }
    }

    void Update(){
        foreach (MazeEntity entity in mazeEntities){
            entity.move();
        }

        // update power pellet
        if(powerPelletActive){
            if(powerPelletTimer > 0){
                powerPelletTimer -= Time.deltaTime;
            }else{
                powerPelletTimer = 0;
                powerPelletActive = false;
                ResetGhostMultiplier();
                Event pelletEndEvent = new Event(Event.eventType.PowerPellet, false);
                notifySubscribers(pelletEndEvent);
            }
        }

        // update AI modes
        if(AIModeSwapIndex < 7){
            if(AIModeSwapTimer > 0f){
                AIModeSwapTimer -= Time.deltaTime;
            }else{
                AIModeSwapTimer = getNextSwapTimer(AIModeSwapIndex);
                AIMode newMode = getNextAIMode(AIModeSwapIndex);
                AIModeSwapIndex += 1;

                Event AIModeEvent = new Event(Event.eventType.AIModeChange, false, newMode);
                notifySubscribers(AIModeEvent);
                Debug.Log("Changing to AI mode "+newMode);
            }
        }
    }

    public void notifySubscribers(Event newEvent){
        foreach(Observer subscriber in subscribers){
            subscriber.eventUpdate(newEvent);
        }
    }
    public void subscribe(Observer subscriber){
        subscribers.Add(subscriber);
    }
    public void unsubscribe(Observer subscriber){
        subscribers.Remove(subscriber);
    }


    private void SetScore(int score)
    {
        this.score = score;
    }

    public void GhostEaten(Ghost ghost){
        SetScore(this.score + (ghost.points * this.ghostMultiplier));
        this.ghostMultiplier++;
        //pacman.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
        Debug.Log("Ghost "+ghost.getAIPattern().getName()+" has been eaten");
    }

    public void PacmanEaten()
    {
        newRound();
        lives -= 1;
        Debug.Log("Pacman has been eaten");
    }

    public void PelletEaten(Pellet pellet){
        pellet.gameObject.SetActive(false);
        gameSound.playMunchSound();
        SetScore(this.score + pellet.points);
        
        int remaining = remainingPellets();
        if(remaining <= 0){
            // All pellets eaten
            newRound();
        }else if(remaining <= 20){
            // activate ghost panic mode
            Event panicEvent = new Event(Event.eventType.Panic);
            notifySubscribers(panicEvent);
        }
    }
    public void PowerPelletEaten(PowerPellet pellet){
        pellet.gameObject.SetActive(false);
        gameSound.playMunchSound();
        Event powerPelletEvent = new Event(Event.eventType.PowerPellet, true);
        notifySubscribers(powerPelletEvent);
        powerPelletTimer += pellet.duration;
        powerPelletActive = true;

        SetScore(this.score + pellet.points);
        
        int remaining = remainingPellets();
        if(remaining <= 0){
            // All pellets eaten
            newRound();
        }else if(remaining <= 20){
            // activate ghost panic mode
            Event panicEvent = new Event(Event.eventType.Panic);
            notifySubscribers(panicEvent);
        }
    }

    private void ResetGhostMultiplier(){
        this.ghostMultiplier=1;
    }

    private void newRound(){
        foreach(MazeEntity entity in mazeEntities){
            entity.reset();
        }
        if(lives == 0){
            foreach(Transform pellet in mazeEatables){
                pellet.gameObject.SetActive(true);
            }
        }

        score = 0;
        powerPelletActive = false;
        powerPelletTimer = 0;
    }

    private int remainingPellets(){
        int total = 0;
        foreach (Transform pellet in this.mazeEatables)
        {
            if(pellet.gameObject.activeSelf){
                total += 1;
            }
        }
        return total;
    }

    private float getNextSwapTimer(int swapIndex){
        switch(swapIndex){
            case 0: return 7f;
            case 1: return 20f;
            case 2: return 7f;
            case 3: return 20f;
            case 4: return 5f;
            case 5: return 20f;
            case 6: return 5f;
            case 7: return 20f;
            default: return 7f;
        }
    }

    private AIMode getNextAIMode(int swapIndex){
        switch(swapIndex){
            case 0: return AIMode.Scatter;
            case 1: return AIMode.Chase;
            case 2: return AIMode.Scatter;
            case 3: return AIMode.Chase;
            case 4: return AIMode.Scatter;
            case 5: return AIMode.Chase;
            case 6: return AIMode.Scatter;
            case 7: return AIMode.Chase;
            default: return AIMode.Scatter;
        }
    }
}
