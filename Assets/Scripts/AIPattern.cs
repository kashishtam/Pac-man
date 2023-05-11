using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPool {
    public enum AIType { Blinky, Inky, Pinky, Clyde, Winky, Slinky, Thinky };
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

    //============ OO Pattern: Object Pool / Simple Factory ============
    public AIPattern get(AIType type){
        if(numTypes[type] < 1 || !locked){
            numTypes[type] += 1;
            
            switch(type){
                case AIType.Blinky: return new BlinkyPattern(pacman, jailNode);
                case AIType.Inky: return new InkyPattern(pacman, jailNode);
                case AIType.Pinky: return new PinkyPattern(pacman, jailNode);
                case AIType.Clyde: return new ClydePattern(pacman, jailNode);
                case AIType.Winky: return new WinkyPattern(pacman, jailNode);
                case AIType.Slinky: return new SlinkyPattern(pacman, jailNode);
                case AIType.Thinky: return new ThinkyPattern(pacman, jailNode);
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

    public void freeAll(){
        foreach(AIType type in Enum.GetValues(typeof(AIType))){
            numTypes[type] = 0;
        }
    }

    public static GameObject closestToGoal(Vector2 goal, Vector2 startPos, GameObject[] options){
        // 5% chance to move randomly instead, to avoid getting stuck
        if(UnityEngine.Random.Range(0f, 1f) < 0.05f){
            int chosen = (int)UnityEngine.Random.Range(0, options.Length);
            return options[chosen];
        }

        GameObject bestOption = null;
        float bestDist = Mathf.Infinity;
        foreach(GameObject option in options){
            Vector2 option2D = option.transform.position;
            float goalDist = Vector2.Distance(option2D, goal);
            float travelDist = Vector2.Distance(startPos, option2D);
            float dist = goalDist + travelDist;
            
            if(dist < bestDist || (dist == bestDist && option2D.y >= startPos.y)){
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

//============ OO Pattern: Strategy ============
public interface AIPattern {
    GameObject getNextMove(GameObject[] moves);
    GameObject getJailNode();
    string getName();
    Color getColor();
    void setAIMode(AIMode mode);
    void setParent(Ghost parent);
    AIMode getAIMode();
}

public class BlinkyPattern : AIPattern {
    public bool panic = false;
    AIMode currentMode;
    Color color = new Vector4(1f, 0f, 0f, 1f);
    Vector2 scatterPos = new Vector2(10f, 10.5f);
    GameObject pacman;
    Ghost parent;
    GameObject jailNode;

    public BlinkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        Vector2 currentPos = parent.gameObject.transform.position;

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 pacmanPos = pacman.transform.position;
                return AIPool.closestToGoal(pacmanPos, currentPos, moves);

            case AIMode.Scatter:
                // head to scatter corner
                if(panic){
                    // do chase pathfinding anyway
                    Vector2 pacmanPosition = pacman.transform.position;
                    return AIPool.closestToGoal(pacmanPosition, currentPos, moves);
                }else{
                    return AIPool.closestToGoal(scatterPos, currentPos, moves);
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
                return AIPool.closestToGoal(jailPos, currentPos, moves);

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

    public void setParent(Ghost parent){
        this.parent = parent;
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
    Ghost parent;
    GameObject jailNode;
    Ghost teamLeader = null;

    public InkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman.GetComponent<Pacman>();
        this.jailNode = jailNode;

        //pickLeader();
    }

    public GameObject getNextMove(GameObject[] moves){
        if(teamLeader == null){
            pickLeader();
        }

        moves = jailMoveCheck(moves);

        Vector2 currentPos = parent.gameObject.transform.position;

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                pickLeader();
                Vector2 pacmanPos = pacman.gameObject.transform.position;
                Vector2 leaderPos = teamLeader.gameObject.transform.position;
                Vector2 offsetA = (pacman.getDirection() * 1.5f);
                Vector2 offsetB = (pacmanPos + offsetA - leaderPos);

                return AIPool.closestToGoal(pacmanPos + offsetA + offsetB, currentPos, moves);

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, currentPos, moves);

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
                return AIPool.closestToGoal(jailPos, currentPos, moves);

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

    public void setParent(Ghost parent){
        this.parent = parent;
    }

    private int rankTeammate(Ghost teammate){
        switch(teammate.getAIPattern().getName()){
            case "Blinky": return 1;
            case "Pinky": return 2;
            case "Inky": return 7;
            case "Clyde": return 5;
            case "Slinky": return 3;
            case "Thinky": return 4;
            case "Winky": return 6;
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
    Ghost parent;
    GameObject jailNode;

    public PinkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman.GetComponent<Pacman>();
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        Vector2 currentPos = parent.gameObject.transform.position;

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 pacmanPos = pacman.gameObject.transform.position;
                Vector2 offset = (pacman.getDirection() * 3f);
                return AIPool.closestToGoal(pacmanPos + offset, currentPos, moves);

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, currentPos, moves);

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
                return AIPool.closestToGoal(jailPos, currentPos, moves);

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

    public void setParent(Ghost parent){
        this.parent = parent;
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
    Ghost parent;
    GameObject jailNode;

    public ClydePattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        Vector2 currentPos = parent.gameObject.transform.position;

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 pacmanPos = pacman.transform.position;

                if(Vector2.Distance(currentPos, pacmanPos) > 12f){
                    return AIPool.closestToGoal(pacmanPos, currentPos, moves);
                }else{
                    return AIPool.closestToGoal(scatterPos, currentPos, moves);
                }

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, currentPos, moves);

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
                return AIPool.closestToGoal(jailPos, currentPos, moves);

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

    public void setParent(Ghost parent){
        this.parent = parent;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>(moves);
        if(result.Contains(jailNode) && currentMode != AIMode.Eaten){
            result.Remove(jailNode);
        }
        return result.ToArray();
    }
}

public class WinkyPattern : AIPattern {
    AIMode currentMode;
    Color color = new Vector4(0f, 1f, 0f, 1f);
    Vector2 scatterPos = new Vector2(-10f, 10.5f);
    GameObject pacman;
    Ghost parent;
    GameObject jailNode;
    Vector2 currentDir = new Vector2(0f, 1f);

    public WinkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        Vector2 currentPos = parent.gameObject.transform.position;

        switch(currentMode){
            case AIMode.Chase:
                Vector2 pacmanPos = pacman.transform.position;

                // go in our current direction or turn
                GameObject bestOption = null;
                float minAngle = Mathf.Infinity;
                foreach(GameObject move in moves){
                    Vector2 movePos = ((Vector2)move.transform.position - currentPos).normalized;
                    float moveAngle = Vector2.Angle(movePos, currentDir);
                    if(moveAngle < minAngle){
                        minAngle = moveAngle;
                        bestOption = move;
                    }
                }
                if(minAngle != Mathf.Infinity && minAngle >= 90f){
                    // random move
                    if(moves.Length > 0){
                        int chosen = UnityEngine.Random.Range(0, moves.Length);
                        return moves[chosen];
                    }
                    return null;
                }else{
                    return bestOption;
                }

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, currentPos, moves);

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
                return AIPool.closestToGoal(jailPos, currentPos, moves);

            default:
                return null;
        }
    }

    public string getName(){
        return "Winky";
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

    public void setParent(Ghost parent){
        this.parent = parent;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>(moves);
        if(result.Contains(jailNode) && currentMode != AIMode.Eaten){
            result.Remove(jailNode);
        }
        return result.ToArray();
    }
}

public class SlinkyPattern : AIPattern {
    AIMode currentMode;
    Color color = new Vector4(185f/255f, 71f/255f, 185f/255f, 1f);
    Vector2 scatterPos = new Vector2(-11.5f, -14f);
    GameObject pacman;
    Ghost parent;
    GameObject jailNode;

    Vector2[] pelletLocations = {
        new Vector2(12.5f, 10.5f),
        new Vector2(12.5f, -14f),
        new Vector2(-12.5f, 10.5f),
        new Vector2(-12.5f, -14f)
    };
    int pelletIndex = 0;

    public SlinkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        Vector2 currentPos = parent.gameObject.transform.position;

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 targPos = pelletLocations[pelletIndex];

                if(Vector2.Distance(currentPos, targPos) > 0.2f){
                    pelletIndex += 1;
                    if(pelletIndex >= pelletLocations.Length){
                        pelletIndex = 0;
                    }
                    targPos = pelletLocations[pelletIndex];
                }
                return AIPool.closestToGoal(targPos, currentPos, moves);

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, currentPos, moves);

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
                return AIPool.closestToGoal(jailPos, currentPos, moves);

            default:
                return null;
        }
    }

    public string getName(){
        return "Slinky";
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

    public void setParent(Ghost parent){
        this.parent = parent;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>(moves);
        if(result.Contains(jailNode) && currentMode != AIMode.Eaten){
            result.Remove(jailNode);
        }
        return result.ToArray();
    }
}

public class ThinkyPattern : AIPattern {
    AIMode currentMode;
    Color color = new Vector4(71f/255f, 185f/255f, 175f/255f, 1f);
    Vector2 scatterPos = new Vector2(-11.5f, -14f);
    GameObject pacman;
    Ghost parent;
    GameObject jailNode;

    public ThinkyPattern(GameObject pacman, GameObject jailNode){
        currentMode = AIMode.Scatter;
        this.pacman = pacman;
        this.jailNode = jailNode;
    }

    public GameObject getNextMove(GameObject[] moves){
        moves = jailMoveCheck(moves);

        Vector2 currentPos = parent.gameObject.transform.position;

        switch(currentMode){
            case AIMode.Chase:
                // get the target tile
                Vector2 targPos = pacman.transform.position;

                if(Vector2.Distance(currentPos, targPos) < 18f){
                    return AIPool.closestToGoal(targPos, currentPos, moves);
                }else{
                    // random move
                    if(moves.Length > 0){
                        int chosen = UnityEngine.Random.Range(0, moves.Length);
                        return moves[chosen];
                    }
                    return null;
                }

            case AIMode.Scatter:
                // head to scatter corner
                return AIPool.closestToGoal(scatterPos, currentPos, moves);

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
                return AIPool.closestToGoal(jailPos, currentPos, moves);

            default:
                return null;
        }
    }

    public string getName(){
        return "Thinky";
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

    public void setParent(Ghost parent){
        this.parent = parent;
    }

    private GameObject[] jailMoveCheck(GameObject[] moves){
        List<GameObject> result = new List<GameObject>(moves);
        if(result.Contains(jailNode) && currentMode != AIMode.Eaten){
            result.Remove(jailNode);
        }
        return result.ToArray();
    }
}
