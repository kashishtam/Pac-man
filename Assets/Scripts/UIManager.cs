using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public enum UIState { Menu, Lives, Introduction, HighScores, Initials, None };
    UIState state = UIState.None;

    public GameObject livesCounter;
    public GameObject livesImage;
    public GameObject introText;
    public GameObject ghostLabel;
    public GameObject[] ghostImages;
    public GameObject UIBackdrop;
    public GameObject titleText;
    public GameObject optionText;
    public GameObject cursorImage;
    public GameObject highScoreText;
    public GameObject initialsEntry;
    public GameObject swapTimer;
    
    MainMenu menu;
    GameManager gameManager;

    public void Start(){
        menu = gameObject.GetComponent<MainMenu>();
        gameManager = gameObject.GetComponent<GameManager>();
    }

    //============ OO Pattern: Model-View-Controller(View) ============
    public void openUI(UIState state = UIState.Lives, string ghostName = "", int lives = 2){
        this.state = state;
        resetUI();

        UIBackdrop.SetActive(true);
        switch(state){
            case UIState.Menu:
                titleText.SetActive(true);
                optionText.SetActive(true);
                cursorImage.SetActive(true);
                menu.SetStatus(MainMenu.MenuStatus.Menu);
            break;

            case UIState.HighScores:
                highScoreText.SetActive(true);
                menu.SetStatus(MainMenu.MenuStatus.HighScores);
            break;

            case UIState.Initials:
                initialsEntry.SetActive(true);
                menu.SetStatus(MainMenu.MenuStatus.Initials);
            break;

            case UIState.Lives:
                livesImage.SetActive(true);
                livesCounter.GetComponent<Text>().text = "x " + lives.ToString();
                livesCounter.SetActive(true);
            break;

            case UIState.Introduction:
                introText.SetActive(true);
                ghostLabel.GetComponent<Text>().text = ghostName;
                ghostLabel.SetActive(true);
                foreach(GameObject ghostImage in ghostImages){
                    ghostImage.SetActive(true);
                }
            break;

            case UIState.None:
                if(gameManager.getGameMode() == GameManager.gameMode.Chaos){
                    swapTimer.SetActive(true);
                }
                UIBackdrop.SetActive(false);
            break;
        }
    }

    public void closeUI(){
        this.state = UIState.None;
        resetUI();
    }

    public GameObject getGhostImage(GameObject ghost){
        int teamIndex = Array.IndexOf(Ghost.team, ghost.GetComponent<Ghost>());
        if(teamIndex >= 0){
            return ghostImages[teamIndex];
        }else{
            return null;
        }
    }

    private void resetUI(){
        livesCounter.SetActive(false);
        livesImage.SetActive(false);
        introText.SetActive(false);
        ghostLabel.SetActive(false);
        UIBackdrop.SetActive(false);
        foreach(GameObject ghostImage in ghostImages){
            ghostImage.SetActive(false);
        }
        titleText.SetActive(false);
        optionText.SetActive(false);
        cursorImage.SetActive(false);
        menu.SetStatus(MainMenu.MenuStatus.None);
        highScoreText.SetActive(false);
        initialsEntry.SetActive(false);
        swapTimer.SetActive(false);
    }
}