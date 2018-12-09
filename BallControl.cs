using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BallControl : MonoBehaviour {

    enum Mode
    {
        TRANSLATE,
        ROTATE,
        POWER,
        SPIN,
        MODE_MAX
    };

    [SerializeField] GameObject aimAssist;
    [SerializeField] GameObject playerCam;
    [SerializeField] GameObject body;
    [SerializeField] GameObject bowlingLane;
    [SerializeField] GameObject powerCanvas;
    [SerializeField] GameObject spinCanvas;
    [SerializeField] GameObject scoreCanvas;
    [SerializeField] GameObject exitCanvas;
    [SerializeField] GameObject resultsCanvas;
    [SerializeField] GameObject moveArrows;
    [SerializeField] GameObject rotateArrows;
    [SerializeField] AudioSource audioPlayer;
    [SerializeField] AudioClip ballRoll;
    public bool manualRotate = true;
    bool inReset;

    float moveSpeed = 0.05f;
    public float moveRange = 1.5f;
    float rotateSpeed = 0.25f; // Radians
    float turnSpeed = 1.2f; // For non-manual rotation
    public float turnRange = 0.2f; // For non-manual rotation
    public float maxPower = 15.0f;
    public float minPower = 5.0f;
    float power = 1.0f;
    float spin = 0.0f;
    public float spinMult = 30.0f;

    float timer;
    float timerMax = 10.0f;

    int roundCounter = 0;

    Vector3 initPos;
    Vector3 initCamPos;
    Quaternion initCamRot;
    Vector3 initBodyPos;
    Quaternion initBodyRot;

    Mode currentMode;

    // Use this for initialization
    void Start () {
        currentMode = Mode.TRANSLATE;
        inReset = false;
        powerCanvas.SetActive(false);
        spinCanvas.SetActive(false);
        exitCanvas.SetActive(false);
        initPos = transform.position;
        initCamPos = playerCam.transform.position;
        initCamRot = playerCam.transform.rotation;
        initBodyPos = body.transform.position;
        initBodyRot = body.transform.rotation;

        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            exitCanvas.GetComponent<SceneSelect>().ToggleActive();
        }

		switch(currentMode)
        {
            // Move the player side to side
            case Mode.TRANSLATE:
                // Move left
                if ((transform.position.x > -moveRange) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
                {
                    transform.Translate(-moveSpeed, 0.0f, 0.0f, Space.World);
                } 
                // Move right
                else if ((transform.position.x < moveRange) && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
                {
                    transform.Translate(moveSpeed, 0.0f, 0.0f, Space.World);
                }
                // Player can switch between translation and rotation
                if (manualRotate)
                {
                    // Switch mode
                    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        moveArrows.SetActive(false);
                        rotateArrows.SetActive(true);

                        currentMode = Mode.ROTATE;
                    }
                    // Confirm
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        moveArrows.SetActive(false);
                        rotateArrows.SetActive(false);

                        powerCanvas.SetActive(true);

                        currentMode = Mode.POWER;
                    }
                }
                // Player must lock in translation before doing rotation
                else
                {
                    // Confirm
                    if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    {
                        currentMode = Mode.ROTATE;
                    }
                }

                break;

            // Rotate the player
            case Mode.ROTATE:
                // If the player is in control of setting rotation
                if (manualRotate)
                {
                    // Rotate left
                    if ((transform.rotation.y > -turnRange) && (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)))
                    {
                        transform.Rotate(0.0f, -rotateSpeed, 0.0f);
                    }
                    // Rotate right
                    else if ((transform.rotation.y < turnRange) && (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)))
                    {
                        transform.Rotate(0.0f, rotateSpeed, 0.0f);
                    }

                    // Switch mode
                    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                    {
                        moveArrows.SetActive(true);
                        rotateArrows.SetActive(false);

                        currentMode = Mode.TRANSLATE;
                    }
                }
                // If the rotation is set via timed button press
                else
                {
                    if (Mathf.Abs(aimAssist.transform.rotation.y) > turnRange) { turnSpeed *= -1; }
                    aimAssist.transform.Rotate(0.0f, turnSpeed, 0.0f);
                }
                
                // Confirm
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    // Set the rotation if not player controlled
                    if (!manualRotate)
                    {
                        transform.rotation = aimAssist.transform.rotation;
                        aimAssist.transform.localRotation = Quaternion.identity;
                    }

                    moveArrows.SetActive(false);
                    rotateArrows.SetActive(false);

                    powerCanvas.SetActive(true);

                    currentMode = Mode.POWER;
                }

                break;

            // Set throwing power
            case Mode.POWER:
                // Confirm
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    // stop the bar from moving
                    powerCanvas.GetComponent<PowerControl>().powerSpeed = 0.0f;
                    power = minPower + maxPower * powerCanvas.GetComponent<PowerControl>().GetRatio();

                    spinCanvas.SetActive(true);

                    currentMode = Mode.SPIN;
                }

                break;

            // Apply spin to the ball
            case Mode.SPIN:
                // Confirm
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    // Stop the spin pointer
                    spinCanvas.GetComponent<SpinControl>().spinSpeed = 0;

                    // Turn off HUD elements
                    aimAssist.SetActive(false);
                    powerCanvas.SetActive(false);
                    spinCanvas.SetActive(false);

                    // Set spin
                    spin = -spinMult * spinCanvas.GetComponent<SpinControl>().spinPointer.transform.localRotation.z * powerCanvas.GetComponent<PowerControl>().GetRatio();

                    // Set velocity
                    GetComponent<Rigidbody>().velocity = power * transform.forward;

                    // Detach body
                    body.transform.parent = null;

                    // Reset timer
                    timer = 0.0f;
                    inReset = false;

                    // Start roll sound
                    audioPlayer.PlayOneShot(ballRoll, 2.0f);

                    currentMode = Mode.MODE_MAX;
                }

                break;
        }
	}

    private void FixedUpdate() {
        if (currentMode == Mode.MODE_MAX)
        {
            // Uncontrolled ball movement
            GetComponent<Rigidbody>().AddRelativeForce(spin, 0.0f, 0.0f);

            // Increment the counter
            if (timer < timerMax) //dMathf.RoundToInt(timerMax / powerCanvas.GetComponent<PowerControl>().GetRatio()))
            {
                timer += Time.fixedDeltaTime;
            }
            else
            {
                ResetBall();
            }

            // Make the ball point in the direction it's moving
            transform.forward = Vector3.Lerp(transform.forward, GetComponent<Rigidbody>().velocity.normalized, 0.1f);
        }
    }

    // Resets the ball's orientation and position
    public void ResetBall()
    {
        if (!inReset)
        {
            inReset = true;

            // Update score
            scoreCanvas.GetComponent<FrameManager>().UpdateScore(roundCounter++);

            if (scoreCanvas.GetComponent<FrameManager>().GetTotalScore() >= 10)
            {
                roundCounter = 2;
            }

            // After the ball has been thrown twice, reset the entire level
            if (roundCounter >= 2)
            {
                GetComponent<BallControl>().enabled = false;
                resultsCanvas.SetActive(true);

                if (scoreCanvas.GetComponent<FrameManager>().GetTotalScore() >= 10)
                {
                    GameObject.Find("SuccessText").SetActive(true);
                    GameObject.Find("FailText").SetActive(false);
                    bowlingLane.GetComponent<BackgroundMusicManager>().PlaySuccess();
                    
                }
                else
                {
                    GameObject.Find("SuccessText").SetActive(false);
                    GameObject.Find("FailText").SetActive(true);
                    bowlingLane.GetComponent<BackgroundMusicManager>().PlayFail();
                }
            }
            else
            {
                transform.position = initPos;
                transform.rotation = Quaternion.identity;
                GetComponent<Rigidbody>().velocity = Vector3.zero;

                currentMode = Mode.TRANSLATE;

                aimAssist.SetActive(true);

                moveArrows.SetActive(true);

                // Re-attach the player camera
                playerCam.transform.position = initCamPos;
                playerCam.transform.rotation = initCamRot;
                playerCam.transform.SetParent(transform);

                body.transform.position = initBodyPos;
                body.transform.rotation = initBodyRot;
                body.transform.SetParent(transform);

                spinCanvas.GetComponent<SpinControl>().Reset();
                powerCanvas.GetComponent<PowerControl>().Reset();
            }
        }
        else
        {
            inReset = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Pin")
        {
            audioPlayer.clip = (AudioClip)Resources.Load("SFX/BallHit" + Random.Range(0, 3));
            audioPlayer.pitch = 0.8f;
            audioPlayer.PlayOneShot(audioPlayer.clip, 2.5f);
        }
    }

    void LoadLevel () {
        SceneManager.LoadScene("TitleMenu");
    }
}
