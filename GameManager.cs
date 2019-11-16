using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager instance = null;

    [SerializeField] LayerMask whatIsGround = 1; // Allows setting of the layer used to signify ground. By default it will use the "default" layer.
    [SerializeField] LayerMask whatIsPlayer = 1; // Sets the layer mask for signifying the player object.
    [SerializeField] GameObject respawnPoint = null; // The game object containing the respawn point for the level.
    [SerializeField] Sprite [] sunPieceSprites = null; // Stores the sprite images that represent each of the sun piece collection states.
    [SerializeField] float lightIncreaseDuration = 3; // The duration that the light increases to the new brighter value over when a sun piece is collected.
    [SerializeField] GameObject logo = null; // The logo animation prefab triggered when the player completes a level.
    [SerializeField] int logoDelayTime = 1; // The time it takes for the logo to appear after the final sun piece is collected.
    [SerializeField] int menuButtonDelay = 3; // The time is takes for the menu button to appear after the final sun piece is collected.

    GameObject player;
    int sunPiecesCollected;
    int sunPiecesInLevel = 0;
    float initialLightIntensity;
    GameObject [] sunPieces;
    GameObject sunPieceUIImage;
    Light directionalLight;
    bool increaseLight;
    float lerpCounter = 0;
    float currentLightIntensity;
    bool levelComplete;
    bool levelLoaded;
    GameObject mainMenuBtn;
    GameObject [] enemies;

    // Getters + Setters
    public GameObject Player {
        get {
            return player;
        } set {
            player = value;
        }
    }

    public GameObject [] Enemies {
        get {
            return enemies;
        }
        set {
            enemies = value;
        }
    }

    public LayerMask WhatIsGround {
        get {
            return whatIsGround;
        }
    }

    public LayerMask WhatIsPlayer {
        get {
            return whatIsPlayer;
        }
    }

    public bool LevelComplete {
        get {
            return levelComplete;
        }
    }

    public GameObject RespawnPoint {
        get {
            return respawnPoint;
        }
    }

    public Sprite [] SunPieceSprites {
        get {
            return sunPieceSprites;
        } set {
            sunPieceSprites  = value;
        }
    }

    public GameObject SunPieceUIImage {
        get {
            return sunPieceUIImage;
        }
    }

    public int SunPiecesCollected {
        get {
            return sunPiecesCollected;
        }
        set {
            sunPiecesCollected = value;
        }
    }

    public GameObject [] SunPieces {
        get {
            return sunPieces;
        }
        set {
            sunPieces = value;
        }
    }

    public Light DirectionalLight {
        get {
            return directionalLight;
        } set {
            directionalLight = value;
        }
    }

    public float InitialLightIntensity {
        get {
            return initialLightIntensity;
        } set {
            initialLightIntensity = value;
        }
    }

    public bool IncreaseLight {
        get {
            return increaseLight;
        } set {
            increaseLight = value;
        }
    }

    public float CurrentLightIntensity {
        get {
            return currentLightIntensity;
        } set {
            currentLightIntensity = value;
        }
    }

    public float LerpCounter {
        get {
            return lerpCounter;
        } set {
            lerpCounter = value;
        }
    }

    void Awake () {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy (gameObject);
        }

        DontDestroyOnLoad (gameObject);

        // Attaches new delgate to sceneloaded event.
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update () {
        if (increaseLight) {
            LightIntensity (currentLightIntensity);
        }
        if (levelLoaded) {
            IsLevelComplete (); // Checks to see if the level has been completed.
        }
    }

    // Method used to increase the light intensity of the diretional light used in the scene.
    public void LightIntensity (float startIntensity) {
        // Calculates the decimal value of what the scene lighting should be based upon the number of sun pieces collected.
        float calculatedLightIncrease;
        if (initialLightIntensity != 0) { // If initial intensity is not 0
            // Calculate the offset needed to keep the brightness intensity inside the 0-1 range if initial intensity is set to something other than 0.
            float offset = ((float) sunPiecesCollected / (float) sunPieces.Length) * initialLightIntensity;
            calculatedLightIncrease = ((float) sunPiecesCollected / (float) sunPieces.Length) - offset; // Calculate intensity value minus the offset.
        } else { // If initial intensity is 0
            calculatedLightIncrease = (float) sunPiecesCollected / (float) sunPieces.Length; // Calculate the intensity value without an offset.
        }
        // Set the intensity of the directional light to the calculated value over a period of time using Lerp.
        lerpCounter += Time.deltaTime / lightIncreaseDuration;
        float endIntensity = initialLightIntensity + calculatedLightIncrease; // The new intensity value.
        if (lerpCounter < 1) {
            directionalLight.intensity = Mathf.Lerp (startIntensity, endIntensity, lerpCounter);
        } else {
            lerpCounter = 0;
            increaseLight = false;
        }
    }

    // Method used to trigger the logo animation when the player has collected all sun pieces, finished the level and is on the ground.
    void IsLevelComplete () {
        if (sunPiecesCollected == 1 && !levelComplete && player.GetComponent<PlayerController> ().Grounded) {
            levelComplete = true;
            player.GetComponent<Rigidbody2D> ().constraints = RigidbodyConstraints2D.FreezeAll; // Stop player from moving.
            MusicManager.instance.Source.clip = MusicManager.instance.LevelCompleteClip; // Sets the audio source clip to the level complete clip.
            MusicManager.instance.Source.Play ();// Play the audio.
            Invoke ("ShowLogo", logoDelayTime);
            Invoke ("ShowMainMenuBtn", menuButtonDelay);
        }
    }

    public void ResetLevel () {
        player.transform.position = respawnPoint.transform.position; // Set player position to the respawn point.
        sunPiecesCollected = 0; // Reset the number of sun pieces collected to zero.
        // Get the UI sun piece colected image and change the sprite to represent the no pieces image.
        sunPieceUIImage.GetComponent<Image> ().sprite = sunPieceSprites [0];
        // Resets all collected sun pieces to become active again.
        // Essentially resets the level.
        foreach (GameObject piece in sunPieces) {
            piece.SetActive (true);
        }
        // Resets the light intensity back to starting value.
        directionalLight.intensity = initialLightIntensity;
        foreach (GameObject enemy in enemies) {
            enemy.SetActive (true);
        }
    }

    // Sets up the game environment on scene loads.
    void OnSceneLoaded (Scene scene, LoadSceneMode mode) {
        int sceneIndex = SceneManager.GetActiveScene().buildIndex;
        MusicManager.instance.Source.clip = MusicManager.instance.LevelMusicClips[sceneIndex];
        MusicManager.instance.Source.Play();
        if (scene.name != "mainmenu") {
            enemies = GameObject.FindGameObjectsWithTag ("Enemy"); // Finds all enemies in the level.
            sunPieces = GameObject.FindGameObjectsWithTag ("Sun Piece"); // Finds all sun pieces in the level and assigns to an array.
            sunPiecesInLevel = sunPieces.Length; // Sets the total number of sun pieces in the level.
            sunPieceUIImage = GameObject.FindGameObjectWithTag ("Sun Piece UI");
            // Get the UI sun piece colected image and change the sprite to represent the no pieces image.
            sunPieceUIImage.GetComponent<Image> ().sprite = sunPieceSprites [0];
            // Disable the main menu button in a level.
            mainMenuBtn = GameObject.FindGameObjectWithTag ("Main Menu Button");
            if (mainMenuBtn != null) {
                mainMenuBtn.SetActive (false);
            }
            levelComplete = false;
            sunPiecesCollected = 0;
            levelLoaded = true;
        }
    }

    void ShowLogo () {
        Vector3 logoPos = Camera.main.transform.position; // Get the position of the main camera and assign as logo position.
        logoPos.z = 0; // Set the logo z position to 0;
        Instantiate (logo, logoPos, Quaternion.identity); // Create a new logo instance.
    }

    void ShowMainMenuBtn () {
        mainMenuBtn.SetActive (true);
    }

    // Returns player to the main menu.
    public void ReturnToMainMenu () {
        levelLoaded = false;
        SceneManager.LoadScene ("mainmenu");
    }

    // Loads the first level.
    public void Play () {
        SceneManager.LoadScene ("level1");
    }

    // Quits to desktop.
    public void Quit () {
        Application.Quit ();
    }

}
