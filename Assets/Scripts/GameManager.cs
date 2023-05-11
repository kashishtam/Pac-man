using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour, Subject
{
    enum gameState { Gameplay, Interlude, Standby };
    public enum gameMode { Classic, Extended, Chaos };
    
    public GameObject ghostPrefab;
    public GameObject jailNode;
    List<Observer> subscribers;
    List<MazeEntity> mazeEntities;
    List<Transform> mazeEatables;
    AIPool pool;
    InterludeManager interlude;
    UIManager ui;
    GameSounds gameSound;
    public TextMeshProUGUI AISwapText;

    public Pacman pacman;
    public int score {get; private set;}
    public int ghostMultiplier{ get; private set;} = 1;
    public int lives{get; private set;} = 3;

    float AIModeSwapTimer = 0;
    int AIModeSwapIndex = 0;
    float chaosSwapTimer = 12f;
    float powerPelletTimer = 0f;
    bool powerPelletActive = false;
    bool gameWon = false;
    gameState currentState = gameState.Standby;
    gameMode currentMode = gameMode.Classic;

    void Start(){
        subscribers = new List<Observer>();
        mazeEntities = new List<MazeEntity>();
        ui = GetComponent<UIManager>();
        gameSound = GetComponent<GameSounds>();

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
            ghost.setParent(this);
            subscribe(ghost);
            mazeEntities.Add(ghost);
            Ghost.team[ghostIndex] = ghost;
            ghostIndex += 1;
        }
    }

    void Update(){
        switch(currentState){
            case gameState.Gameplay:
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
                }
            }

            // update timer for chaos mode
            if(currentMode == gameMode.Chaos){
                if(chaosSwapTimer > 0){
                    chaosSwapTimer -= Time.deltaTime;
                }else if(powerPelletTimer <= 0){
                    AISwap();
                    chaosSwapTimer = 12f;
                }
                AISwapText.text = "Ghosts Swap In: \n"+((int)chaosSwapTimer);
            }
            break;

            case gameState.Interlude:
            if(interlude == null){
                currentState = gameState.Gameplay;
            }else{
                if(interlude.IsInterludeFinished()){
                    currentState = gameState.Gameplay;
                }
            }
            break;
        }
    }

    //============ OO Pattern: Observer(Subject) ============
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
        HighScoreManager.instance.UpdateScore(score);
    }

    public void GhostEaten(Ghost ghost){
        gameSound.playGhostEatenSound();
        SetScore(this.score + (ghost.points * this.ghostMultiplier));
        this.ghostMultiplier++;
    }

    public void PacmanEaten()
    {
        gameSound.playDeathSound();
        pacman.die();
        lives -= 1;
        currentState = gameState.Standby;
        gameWon = false;
        Invoke("newRound", 1.3f);
    }

    public void PelletEaten(Pellet pellet){
        pellet.gameObject.SetActive(false);
        gameSound.playMunchSound();

        SetScore(this.score + pellet.points);
        
        int remaining = remainingPellets();
        if(remaining <= 0){
            // All pellets eaten
            currentState = gameState.Standby;
            gameWon = true;
            Invoke("newRound", 1.3f);
        }else if(remaining <= 20){
            // activate ghost panic mode
            Event panicEvent = new Event(Event.eventType.Panic);
            notifySubscribers(panicEvent);
        }
    }
    public void PowerPelletEaten(PowerPellet pellet){
        gameSound.playPowerPelletSound(pellet.duration);
        pellet.gameObject.SetActive(false);
        Event powerPelletEvent = new Event(Event.eventType.PowerPellet, true);
        notifySubscribers(powerPelletEvent);
        powerPelletTimer = pellet.duration;
        powerPelletActive = true;

        SetScore(this.score + pellet.points);
        
        int remaining = remainingPellets();
        if(remaining <= 0){
            // All pellets eaten
            currentState = gameState.Standby;
            gameWon = true;
            Invoke("newRound", 1.3f);
        }else if(remaining <= 20){
            // activate ghost panic mode
            Event panicEvent = new Event(Event.eventType.Panic);
            notifySubscribers(panicEvent);
        }
    }

    public void setGameMode(gameMode newMode){
        this.currentMode = newMode;
        AISetup();
    }

    public void enterHighScore(string initials){
        HighScoreManager.instance.newScore(score, initials);
        SetScore(0);
    }

    public gameMode getGameMode(){
        return currentMode;
    }

    private void ResetGhostMultiplier(){
        this.ghostMultiplier=1;
    }

    public void newRound(bool won){
        gameWon = won;
        newRound();
    }

    private void newRound(){
        foreach(MazeEntity entity in mazeEntities){
            entity.reset();
        }
        AIModeSwapTimer = 0;
        AIModeSwapIndex = 0;

        if(gameWon || lives == 0){
            foreach(Transform pellet in mazeEatables){
                pellet.gameObject.SetActive(true);
            }
        }

        if(gameWon){
            switch(currentMode){
                case gameMode.Extended:
                    interlude = new InterludeManager(AISwap(), ui);
                break;

                case gameMode.Chaos:
                    chaosSwapTimer = 12f;
                    interlude = new InterludeManager(new List<GameObject>(), ui);
                break;

                default:
                    interlude = new InterludeManager(new List<GameObject>(), ui);
                break;
            }
            currentState = gameState.Interlude;
            gameWon = false;
        }else{
            if(lives <= 0){
                lives = 3;
                currentState = gameState.Standby;
                if(HighScoreManager.instance.isHighScore(score)){
                    ui.openUI(UIManager.UIState.Initials);
                }else{
                    SetScore(0);
                    ui.openUI(UIManager.UIState.Menu);
                }
            }else{
                currentState = gameState.Interlude;
                interlude = new InterludeManager(new List<GameObject>(), ui);
                ui.openUI(UIManager.UIState.Lives, "", lives);
            }
        }
        interlude.setParent(this);

        powerPelletActive = false;
        powerPelletTimer = 0;
        Event powerPelletOverride = new Event(Event.eventType.PowerPellet, false);
        notifySubscribers(powerPelletOverride);
    }

    private List<GameObject> AISwap(){
        List<string> priorPatterns = new List<string>();
        List<GameObject> newAdditions = new List<GameObject>();
        pool.freeAll();
        pool.lockPool();

        if(currentMode == gameMode.Chaos && remainingPellets() < 25 && UnityEngine.Random.Range(0f,1f) < 0.2f){
            // all-blinky rush
            pool.unlockPool();
            Ghost.team[0].setAIPattern(pool.get(AIPool.AIType.Blinky));
            Ghost.team[1].setAIPattern(pool.get(AIPool.AIType.Blinky));
            Ghost.team[2].setAIPattern(pool.get(AIPool.AIType.Blinky));
            Ghost.team[3].setAIPattern(pool.get(AIPool.AIType.Blinky));
        }else{
            foreach(Ghost member in Ghost.team){
                priorPatterns.Add(member.getAIPattern().getName());
                member.setAIPattern(pool.getRandom());
                if(currentMode == gameMode.Chaos){
                    member.getAIPattern().setAIMode(AIMode.Chase);
                }
            }
        }

        foreach(Ghost member in Ghost.team){
            if(!priorPatterns.Contains(member.getAIPattern().getName())){
                newAdditions.Add(member.gameObject);
            }
        }

        return newAdditions;
    }

    private void AISetup(){
        pool.freeAll();

        if(currentMode == gameMode.Classic){
            Ghost.team[0].setAIPattern(pool.get(AIPool.AIType.Blinky));
            Ghost.team[1].setAIPattern(pool.get(AIPool.AIType.Pinky));
            Ghost.team[2].setAIPattern(pool.get(AIPool.AIType.Inky));
            Ghost.team[3].setAIPattern(pool.get(AIPool.AIType.Clyde));
        }else{
            foreach(Ghost member in Ghost.team){
                AIPattern pattern = pool.getRandom();
                pattern.setAIMode(AIMode.Scatter);
                member.setAIPattern(pattern);
            }
        }
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
