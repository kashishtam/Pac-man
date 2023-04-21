using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPool {
    public enum AIType { Blinky, Inky, Pinky, Clyde };
    Dictionary<AIType, int> numTypes; // holds a count of how many of each type are in use
    GameObject pacman;
    GameObject jailNode;
    bool locked = true;

    public AIPool(GameObject pacman, GameObject jailNode){
        numTypes = new Dictionary<AIType, int>();
        foreach(AIType type in Enum.GetValues(typeof(AIType))){
            numTypes.Add(type, 0);
        }
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public AIPattern get(AIType type){
        if(numTypes[type] < 1 || !locked){
            numTypes[type] += 1;
            
            switch(type){
                case AIType.Blinky: return new BlinkyPattern(pacman, jailNode);
                case AIType.Inky: return new InkyPattern(pacman, jailNode);
                case AIType.Pinky: return new PinkyPattern(pacman, jailNode);
                case AIType.Clyde: return new ClydePattern(pacman, jailNode);
            }
        }
        return null;
    }

    public AIPattern getRandom(){
        AIType[] types = (AIType[])Enum.GetValues(typeof(AIType));
        int index = -1;
        while(index < 0 || (locked && numTypes[types[index]] >= 1)){
            index = (int)Mathf.Floor(UnityEngine.Random.Range(0, types.Length));
        }
        return get(types[index]);
    }

    public static GameObject closestToGoal(Vector2 goal, GameObject[] options){
        GameObject bestOption = null;
        float bestDist = Mathf.Infinity;
        foreach(GameObject option in options){
            Vector2 option2D = option.transform.position;
            float dist = Vector2.Distance(option2D, goal);
            
            if(dist < bestDist){
                bestDist = dist;
                bestOption = option;
            }
        }
        return bestOption;
    }

    public void lockPool(){
        locked = true;
    }

    public void unlockPool(){
        locked = false;
    }
}

// ======================== AI Patterns ========================
/*
  The behavior patterns for Blinky, Pinky, Inky and Clyde are based on the high-level overview sections of 
  this video (https://www.youtube.com/watch?v=ataGotQ7ir8) by Retro Game Mechanics Explained.

  Modifications were made to accomodate a grid-based pathfinding approach rather than a greedy 
  direction-picking approach and to eliminate some bugs and quirks in the orginal behavior.
*/


public enum AIMode { Chase, Scatter, Frightened, Eaten };

public interface AIPattern {
    GameObject getNextMove(GameObject[] moves);
    GameObject getJailNode();
    string getName();
    Color getColor();
    void setAIMode(AIMode mode);
    AIMode getAIMode();
}

public class BlinkyPattern : AIPattern {
    public bool panic = false;
    AIMode currentMode;
    Color color = new Vector4(1f, 0f, 0f, 1f);
    Vector2 scatterPos = new Vector2(10f, 10.5f);
    GameObject pacman;
    GameObject jailNode;

    public BlinkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 pacmanPos = pacman.transform.position;
                return AIPool.closestToGoal(pacmanPos, moves);

            case AIMode.Scatter:
                // head to scatter corner
                if(panic){
                    // do chase pathfinding anyway
                    Vector2 pacmanPosition = pacman.transform.position;
                    return AIPool.closestToGoal(pacmanPosition, moves);
                }else{
                    return AIPool.closestToGoal(scatterPos, moves);
                }

            case AIMode.Frightened:
                // random move
                if(moves.Length > 0){
                    int chosen = UnityEngine.Random.Range(0, moves.Length);
                    return moves[chosen];
                }
                return null;

            case AIMode.Eaten:
                // head to jail node
                Vector2 jailPos = jailNode.transform.position;
                return AIPool.closestToGoal(jailPos, moves);

            default:
                return null;
        }
    }

    public string getName(){
        return "Blinky";
    }

    public void setAIMode(AIMode mode){
        currentMode = mode;
    }
    public AIMode getAIMode(){
        return currentMode;
    }

    public Color getColor(){
        return color;
    }

    public GameObject getJailNode(){
        return jailNode;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>();
        foreach(GameObject node in moves){
            if(node != jailNode || currentMode == AIMode.Eaten){
                result.Add(node);
            }
        }
        return result.ToArray();
    }
}

public class InkyPattern : AIPattern {
    AIMode currentMode;
    Color color = new Vector4(71f/255f, 185f/255f, 255f/255f, 1f);
    Vector2 scatterPos = new Vector2(11.5f, -14f);
    Pacman pacman;
    GameObject jailNode;
    Ghost teamLeader;

    public InkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman.GetComponent<Pacman>();
        this.jailNode = jailNode;

        pickLeader();
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                pickLeader();
                Vector2 pacmanPos = pacman.gameObject.transform.position;
                Vector2 leaderPos = teamLeader.gameObject.transform.position;
                Vector2 offsetA = (pacman.getDirection() * 1.5f);
                Vector2 offsetB = (pacmanPos + offsetA - leaderPos);

                return AIPool.closestToGoal(pacmanPos + offsetA + offsetB, moves);

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, moves);

            case AIMode.Frightened:
                // random move
                if(moves.Length > 0){
                    int chosen = UnityEngine.Random.Range(0, moves.Length);
                    return moves[chosen];
                }
                return null;

            case AIMode.Eaten:
                // head to jail node
                Vector2 jailPos = jailNode.transform.position;
                return AIPool.closestToGoal(jailPos, moves);

            default:
                return null;
        }
    }

    public string getName(){
        return "Inky";
    }

    public void setAIMode(AIMode mode){
        currentMode = mode;
    }
    public AIMode getAIMode(){
        return currentMode;
    }

    public Color getColor(){
        return color;
    }

    public GameObject getJailNode(){
        return jailNode;
    }

    private int rankTeammate(Ghost teammate){
        switch(teammate.getAIPattern().getName()){
            case "Blinky": return 1;
            case "Pinky": return 2;
            case "Inky": return 4;
            case "Clyde": return 3;
            default: return 64;
        }
    }

    private void pickLeader(){
        int bestRank = 42;
        Ghost bestMember = Ghost.team[0];
        foreach(Ghost member in Ghost.team){
            if(member != null){
                int rank = rankTeammate(member);
                if(rank < bestRank){
                    bestRank = rank;
                    bestMember = member;
                }
            }
        }
        teamLeader = bestMember;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>();
        foreach(GameObject node in moves){
            if(node != jailNode || currentMode == AIMode.Eaten){
                result.Add(node);
            }
        }
        return result.ToArray();
    }
}

public class PinkyPattern : AIPattern {
    AIMode currentMode;
    Color color = new Vector4(255f/255f, 184f/255f, 255f/255f, 1f);
    Vector2 scatterPos = new Vector2(-10f, 10.5f);
    Pacman pacman;
    GameObject jailNode;

    public PinkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman.GetComponent<Pacman>();
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 pacmanPos = pacman.gameObject.transform.position;
                Vector2 offset = (pacman.getDirection() * 3f);
                return AIPool.closestToGoal(pacmanPos + offset, moves);

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, moves);

            case AIMode.Frightened:
                // random move
                if(moves.Length > 0){
                    int chosen = UnityEngine.Random.Range(0, moves.Length);
                    return moves[chosen];
                }
                return null;

            case AIMode.Eaten:
                // head to jail node
                Vector2 jailPos = jailNode.transform.position;
                return AIPool.closestToGoal(jailPos, moves);

            default:
                return null;
        }
    }

    public string getName(){
        return "Pinky";
    }

    public void setAIMode(AIMode mode){
        currentMode = mode;
    }
    public AIMode getAIMode(){
        return currentMode;
    }

    public Color getColor(){
        return color;
    }

    public GameObject getJailNode(){
        return jailNode;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>();
        foreach(GameObject node in moves){
            if(node != jailNode || currentMode == AIMode.Eaten){
                result.Add(node);
            }
        }
        return result.ToArray();
    }
}

public class ClydePattern : AIPattern {
    AIMode currentMode;
    Color color = new Vector4(255f/255f, 185f/255f, 81f/255f, 1f);
    Vector2 scatterPos = new Vector2(-11.5f, -14f);
    GameObject pacman;
    GameObject jailNode;

    public ClydePattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 pacmanPos = pacman.transform.position;
                Vector2 selfPos = pacmanPos;
                foreach(Ghost member in Ghost.team){
                    if(member.getAIPattern() == this){
                        selfPos = member.gameObject.transform.position;
                    }
                }

                if(Vector2.Distance(selfPos, pacmanPos) > 12f){
                    return AIPool.closestToGoal(pacmanPos, moves);
                }else{
                    return AIPool.closestToGoal(scatterPos, moves);
                }

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, moves);

            case AIMode.Frightened:
                // random move
                if(moves.Length > 0){
                    int chosen = UnityEngine.Random.Range(0, moves.Length);
                    return moves[chosen];
                }
                return null;

            case AIMode.Eaten:
                // head to jail node
                Vector2 jailPos = jailNode.transform.position;
                return AIPool.closestToGoal(jailPos, moves);

            default:
                return null;
        }
    }

    public string getName(){
        return "Clyde";
    }

    public void setAIMode(AIMode mode){
        currentMode = mode;
    }
    public AIMode getAIMode(){
        return currentMode;
    }

    public Color getColor(){
        return color;
    }

    public GameObject getJailNode(){
        return jailNode;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>(moves);
        if(result.Contains(jailNode) && currentMode != AIMode.Eaten){
            result.Remove(jailNode);
        }
        return result.ToArray();
    }
}