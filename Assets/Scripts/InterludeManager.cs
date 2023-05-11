using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterludeManager {
    GameManager parent;
    InterludeAction action = null;
    UIManager ui = null;

    public InterludeManager(List<GameObject> additions, UIManager ui){
        this.ui = ui;
        action = new DisplayLives(ui);
        foreach(GameObject addition in additions){
            GameObject image = ui.getGhostImage(addition);
            action = new DisplayIntro(action, addition, image, ui);
        }
        foreach(Ghost member in Ghost.team){
            Image ghostImg = ui.getGhostImage(member.gameObject).GetComponent<Image>();
            ghostImg.color = member.getAIPattern().getColor();
        }

        if(additions.Count > 0){
            ui.openUI(UIManager.UIState.Introduction, "");
        }
    }

    public bool IsInterludeFinished(){
        return action.tick();
    }

    public void setParent(GameManager parent){
        this.parent = parent;
        action.setParent(parent);
    }
}

public interface InterludeAction {
    bool tick();
    void setParent(GameManager parent);
}

public class DisplayLives : InterludeAction {
    private GameManager parent;
    private UIManager ui;
    private float timer;
    private bool firstTick;

    public DisplayLives(UIManager ui){
        this.ui = ui;
        timer = 2f;
        firstTick = true;
    }

    public bool tick(){
        if(firstTick){
            firstTick = false;
            ui.openUI(UIManager.UIState.Lives, "", parent.lives);
        }

        if(timer > 0){
            timer -= Time.deltaTime;
            return false;
        }else{
            //ui.closeUI();
            ui.openUI(UIManager.UIState.None);
            return true;
        }
    }

    public void setParent(GameManager parent){
        this.parent = parent;
    }
}

//============ OO Pattern: Decorator ============
public class DisplayIntro : InterludeAction {
    public enum interludeState { StepUp, Wait, StepDown, Done };

    private GameManager parent;
    private UIManager ui;
    private InterludeAction next;
    private float timer;
    private interludeState currentState = interludeState.StepUp;

    private GameObject ghost;
    private GameObject ghostImage;
    private Vector2 baseGhostPos;
    private Vector2 stepUpOffset = new Vector2(0f, 16f);

    public DisplayIntro(InterludeAction next, GameObject ghost, GameObject ghostImage, UIManager ui){
        timer = 0.3f;
        this.next = next;
        this.ghost = ghost;
        this.ghostImage = ghostImage;
        this.ui = ui;
        baseGhostPos = ghostImage.transform.position;
        ghostImage.GetComponent<Image>().color = ghost.GetComponent<Ghost>().getAIPattern().getColor();
    }

    public bool tick(){
        if(timer > 0){
            timer -= Time.deltaTime;

            switch(currentState){
                case interludeState.StepUp:
                    // increase offset
                    ghostImage.transform.position = baseGhostPos + Vector2.Lerp(Vector2.zero, stepUpOffset, 1f - (timer / 0.3f));
                break;

                case interludeState.StepDown:
                    // decrease offset
                    ghostImage.transform.position = baseGhostPos + Vector2.Lerp(Vector2.zero, stepUpOffset, (timer / 0.3f));
                break;
            }

            return false;
        }else{
            switch(currentState){
                case interludeState.StepUp:
                    timer = 0.8f;
                    currentState = interludeState.Wait;
                    ui.openUI(UIManager.UIState.Introduction, ghost.GetComponent<Ghost>().getAIPattern().getName());
                    return false;

                case interludeState.Wait:
                    timer = 0.3f;
                    currentState = interludeState.StepDown;
                    return false;

                case interludeState.StepDown:
                    timer = 0f;
                    currentState = interludeState.Done;
                    ghostImage.transform.position = baseGhostPos;
                    return false;

                case interludeState.Done:
                    if(next != null){
                        return next.tick();
                    }
                    return true;
            }

            if(next != null){
                return next.tick();
            }
            return true;
        }
    }

    public void setParent(GameManager parent){
        this.parent = parent;
        if(next != null){
            next.setParent(parent);
        }
    }
}
