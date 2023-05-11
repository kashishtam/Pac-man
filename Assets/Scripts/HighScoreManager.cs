using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager instance;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI highScoreList;
    
    int highScore = 0;
    List<int> savedHighScores = new List<int>();
    List<string> savedInitials = new List<string>();
    string filepath = "./high_scores.txt";

    private void Awake() {
        instance = this;
    }
    void Start()
    {
        highScore = PlayerPrefs.GetInt("highScore",0);
        scoreText.text = "SCORE \n0";
        highScoreText.text = "HIGH SCORE \n" + highScore.ToString();

        if(File.Exists(filepath)){
            readScores();
            highScore = savedHighScores[0];
        }else{
            int[] defaultScores = { 2000, 1200, 800, 640, 42 };
            string[] defaultInitials = { "AAA", "BBB", "CCC", "DDD", "EEE" };
            savedHighScores = new List<int>(defaultScores);
            savedInitials = new List<string>(defaultInitials);
            writeScores();
        }
    }

    public void UpdateScore(int score)
    {
        scoreText.text = "SCORE \n" + score.ToString();
        if(score > highScore){
            PlayerPrefs.SetInt("highScore",score);
            highScore = score;
            highScoreText.text = "HIGH SCORE \n" + highScore.ToString();
        }
    }

    //============ OO Pattern: Data Access ============
    public void newScore(int score, string initials="OOP"){
        int index = savedHighScores.Count;
        while(index > 0 && savedHighScores[index-1] < score){
            index -= 1;
        }
        
        if(index < savedHighScores.Count){
            savedHighScores.Insert(index, score);
            savedInitials.Insert(index, initials);

            savedHighScores.RemoveAt(savedHighScores.Count-1);
            savedInitials.RemoveAt(savedInitials.Count-1);

            writeScores();

            String scoreList = listScores();
            highScoreList.text = "HIGH SCORES \n\n" + scoreList;
        }
    }

    public bool isHighScore(int score){
        return (score > savedHighScores[savedHighScores.Count - 1]);
    }

    private void writeScores(){
        StreamWriter writer = File.CreateText(filepath);

        for(int i=0; i<savedHighScores.Count; i++){
            writer.Write( savedInitials[i] + "\t" + savedHighScores[i] );
            writer.WriteLine();
        }

        writer.Close();
    }

    private void readScores(){
        FileStream file = File.Open(filepath, FileMode.Open);
        StreamReader reader = new StreamReader(file);

        for(int i=0; i<5; i++){
            string line = reader.ReadLine();
            if(line == null){
                break;
            }
            string[] parsed = line.Split("\t");
            savedInitials.Add(parsed[0]);
            savedHighScores.Add(int.Parse(parsed[1]));
        }

        reader.Close();

        String scoreList = listScores();
        highScoreList.text = "HIGH SCORES \n\n" + scoreList;
    }

    public String listScores(){
        string output = "";
        for(int i=0; i<savedHighScores.Count; i++){
            output += (savedInitials[i] + "\t" + savedHighScores[i] + "\n");
        }
        return output;
    }
}
