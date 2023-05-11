using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour {
    public enum MenuStatus { Menu, HighScores, Initials, None };

    public GameObject cursorImage;
    public GameObject[] cursorPositions;
    public TextMeshProUGUI initialsEntry;
    GameManager gameManager;
    UIManager ui;

    private int cursorPosition = 0;
    private int numOptions = 3;
    private MenuStatus currentStatus = MenuStatus.Menu;
    private string currentInitials = "";

    public void Start(){
        gameManager = gameObject.GetComponent<GameManager>();
        ui = gameObject.GetComponent<UIManager>();
    }

    public void Awake(){
        cursorPosition = 0;
    }

    //============ OO Pattern: Model-View-Controller(Controller) ============
    public void Update(){
        if(currentStatus == MenuStatus.Menu){
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                cursorPosition -= 1;
                if(cursorPosition < 0){
                    cursorPosition = numOptions;
                }
                cursorImage.transform.position = cursorPositions[cursorPosition].transform.position;
            }
            else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                cursorPosition += 1;
                if(cursorPosition > numOptions){
                    cursorPosition = 0;
                }
                cursorImage.transform.position = cursorPositions[cursorPosition].transform.position;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
                switch(cursorPosition){
                    // start game
                    case 0:
                        gameManager.setGameMode(GameManager.gameMode.Classic);
                        gameManager.newRound(false);
                        ui.openUI(UIManager.UIState.None);
                        currentStatus = MenuStatus.None;
                    break;

                    case 1:
                        gameManager.setGameMode(GameManager.gameMode.Extended);
                        gameManager.newRound(false);
                        ui.openUI(UIManager.UIState.None);
                        currentStatus = MenuStatus.None;
                    break;

                    case 2:
                        gameManager.setGameMode(GameManager.gameMode.Chaos);
                        gameManager.newRound(false);
                        ui.openUI(UIManager.UIState.None);
                        currentStatus = MenuStatus.None;
                    break;

                    case 3:
                        currentStatus = MenuStatus.HighScores;
                        ui.openUI(UIManager.UIState.HighScores);
                    break;
                }
            }
        }else if(currentStatus == MenuStatus.HighScores){
            if ( Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || 
                Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow) || 
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ) {
                currentStatus = MenuStatus.Menu;
                ui.openUI(UIManager.UIState.Menu);
            }
        }else if(currentStatus == MenuStatus.Initials){
            if(Input.inputString.Length > 0){
                string newInitials = currentInitials;
                int strLength = currentInitials.Length;

                // backspace handling
                if(Input.inputString.Contains('\b')){
                    strLength = Math.Max(0, strLength - 1);
                }else{
                    newInitials = (currentInitials + Input.inputString).ToUpper();
                    strLength = Math.Min(3, newInitials.Length);
                }

                currentInitials = newInitials.Substring(0, strLength);
                initialsEntry.text = "HIGH SCORE \nEnter your initials: \n" + currentInitials;
            }

            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)){
                gameManager.enterHighScore(currentInitials);

                currentStatus = MenuStatus.HighScores;
                ui.openUI(UIManager.UIState.HighScores);
            }
        }
    }

    public void SetStatus(MenuStatus status){
        currentStatus = status;
    }
}