using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FrameManager : MonoBehaviour {

    [SerializeField] GameObject pinManager;

    int roundScore, totalScore, lastRemainingPins, maxPins;

    // Use this for initialization
    void Start () {
        maxPins = lastRemainingPins = pinManager.GetComponent<PinManager>().GetMaxPins();
        roundScore = 0;
        totalScore = 0;
	}

    public int GetTotalScore() {
        return totalScore;
    }

    public void UpdateScore (int roundCounter) {
        // Set the score for the round
        Text frame = GameObject.Find("Frame" + roundCounter).GetComponentInChildren<Text>();
        int currRemainingPins = pinManager.GetComponent<PinManager>().GetRemainingPins();

        roundScore = lastRemainingPins - currRemainingPins;
        lastRemainingPins = currRemainingPins;
        totalScore += roundScore;

        if (totalScore == maxPins)
        {
            if (roundCounter == 0)
            {
                frame.text = "X";
                totalScore += 20;
            }
            else
            {
                frame.text = "/";
                totalScore += 10;
            }
        }
        else
        {
            if (roundScore > 0)
            {
                frame.text = "" + roundScore;
            }
            else
            {
                frame.text = "-";
            }
        }

        // Update the total score
        GameObject.Find("TotalScore").GetComponent<Text>().text = "" + totalScore;
    }
}
