using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinManager : MonoBehaviour {

    //Vector3 initPos; // Initial position of pin 1 (all other pins set based on this position)
    public int maxPins = 10;
    int pinsRemaining;

    // Use this for initialization
    void Start() {
        //pinsRemaining = maxPins;
        //maxPins = pinsRemaining;
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        // Detach the camera when we get close enough 
        if (other.tag == "MainCamera")
        {
            other.transform.parent = null;
        }

        if (other.tag == "Pin")
        {
            pinsRemaining++;
            other.GetComponent<PinSolo>().UnmarkForDeletion();
        }
    }

    void OnTriggerExit (Collider other) {
        // If a pin leaves the hitbox, decrement the number of remaining pins
        if (other.tag == "Pin")
        {
            pinsRemaining--;
            other.GetComponent<PinSolo>().MarkForDeletion();
        }
    }

    // Reset the pin positions
    void ResetPins () {
        //pin1.transform.position = initPos;
    }

    // return the number of pins still standing
    public int GetRemainingPins () {
        return pinsRemaining;
    }

    public int GetMaxPins () {
        return maxPins;
    }
}
