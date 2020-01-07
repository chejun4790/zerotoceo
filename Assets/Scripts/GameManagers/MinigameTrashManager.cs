﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Timers;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(MinigameManager))]
public class MinigameTrashManager : MonoBehaviour
{
    private int _trashCount;
    private GameObject _player, _characters, _ground, _ui;
    private TextMeshProUGUI _timerText;
    private MinigameManager _minigameManager;
    // Config
    private float _trashGravity;
    private int _playerMovementSpeed;
    private float _instantiateDelay;
    private int _timer;
    
    public List<Sprite> trashSprites;
    public GameObject trashPrefab;
    # region public config classes
    [System.Serializable]
    public class EasyDifficultyConfig
    {
        public float trashGravity = .75f;
        public int playerMovementSpeed = 8;
        public float instantiateDelay = 2f;
        public int timer = 30;
    }
    public EasyDifficultyConfig easyDifficultyConfig;
	
    [System.Serializable]
    public class MediumDifficultyConfig
    {
        public float trashGravity = .75f;
        public int playerMovementSpeed = 10;
        public float instantiateDelay = 1.5f;
        public int timer = 45;
    }
    public MediumDifficultyConfig mediumDifficultyConfig;
	
    [System.Serializable]
    public class HardDifficultyConfig
    {
        public float trashGravity = .75f;
        public int playerMovementSpeed = 12;
        public float instantiateDelay = 1f;
        public int timer = 60;
    }
    public HardDifficultyConfig hardDifficultyConfig;
    #endregion
    
    void Start()
    {
        _ui = GameObject.FindWithTag("UI");
        _timerText = _ui.transform.Find("HUD").gameObject.transform.Find("Timer").gameObject.GetComponent<TextMeshProUGUI>();
        _minigameManager = GetComponent<MinigameManager>();
        _player = GameObject.FindWithTag("Player");
        _characters = GameObject.Find("Characters");
        _ground = GameObject.Find("Minigame World").transform.Find("Ground").gameObject;
        
        _timerText.SetText($"Time left: {_timer} seconds");
        LoadDifficultyConfig();
        Invoke(nameof(StartGame), 3f);
    }

    private void StartGame()
    {
        StartCoroutine(InstantiateLoop());
        StartCoroutine(Timer());
    }
    
    private void LoadDifficultyConfig()
    {
        switch (MinigameManager.MinigameDifficulty)
        {
            case "Hard":
                _trashGravity = hardDifficultyConfig.trashGravity;
                _playerMovementSpeed = hardDifficultyConfig.playerMovementSpeed;
                _instantiateDelay = hardDifficultyConfig.instantiateDelay;
                _timer = hardDifficultyConfig.timer;
                break;
            case "Medium":
                _trashGravity = mediumDifficultyConfig.trashGravity;
                _playerMovementSpeed = mediumDifficultyConfig.playerMovementSpeed;
                _instantiateDelay = mediumDifficultyConfig.instantiateDelay;
                _timer = mediumDifficultyConfig.timer;
                break;
            default: // "Easy"
                _trashGravity = easyDifficultyConfig.trashGravity;
                _playerMovementSpeed = easyDifficultyConfig.playerMovementSpeed;
                _instantiateDelay = easyDifficultyConfig.instantiateDelay;
                _timer = easyDifficultyConfig.timer;
                break;
        }
    }

    private void InstantiateTrash()
    {
        // Instantiate trash
        var trash = Instantiate(trashPrefab, new Vector3(Random.Range(-8f, 8f), 6f, 0f), Quaternion.identity);
        trash.transform.parent = _characters.transform;
        trash.GetComponent<CollidableController>().targetObject = _ground;
        trash.GetComponent<CollidableController>().collisionMethod.AddListener(_minigameManager.Fail);
        trash.GetComponent<CollidableController>().collisionMethod.AddListener(delegate{ trash.GetComponent<CollidableController>().collisionEventsEnabled = false; });
        trash.GetComponent<CollidableController>().secondaryTargetObject = _player;
        trash.GetComponent<CollidableController>().secondaryCollisionMethod.AddListener(delegate{Destroy(trash);});
        trash.GetComponent<Rigidbody2D>().gravityScale = _trashGravity;
        
        // Set random sprite
        trash.GetComponent<SpriteRenderer>().sprite = trashSprites[Random.Range(0, trashSprites.Count)];
    }

    private IEnumerator InstantiateLoop()
    {
        while (true)
        {
            InstantiateTrash();
            yield return new WaitForSeconds(_instantiateDelay);
        }
    }

    private IEnumerator Timer()
    {
        for (var i = _timer; i >= 0; i--)
        {
            _timerText.SetText($"Time left: {i} seconds");
            yield return new WaitForSeconds(1);
        }
        _minigameManager.Pass();
    }

    void Update()
    {
        #region player movement
        if (Input.GetAxisRaw("Horizontal") == 1)
        {
            _player.transform.Translate(Vector3.right * Time.deltaTime * _playerMovementSpeed);
            if (_player.transform.localScale.x < 0f && Time.timeScale == 1f)
                _player.transform.localScale =
                    new Vector3(-_player.transform.localScale.x, _player.transform.localScale.y, _player.transform.localScale.z);
        }
	
        if (Input.GetAxisRaw("Horizontal") == -1)
        {
            _player.transform.Translate(Vector3.left * Time.deltaTime * _playerMovementSpeed);
            if (_player.transform.localScale.x > 0f && Time.timeScale == 1f)
                _player.transform.localScale =
                    new Vector3(-_player.transform.localScale.x, _player.transform.localScale.y, _player.transform.localScale.z);
        }
        #endregion
    }
}
