using ChainAbstractions;
using ChainAbstractions.Stacks;
using StacksForce.Stacks;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    private const float SPEED_COEFF = 0.06f;
    private const float OBSTACLE_MIN_RANGE = 3.5f;
    private const float OBSTACLE_MAX_RANGE = 5f;
    private const float Y_BOUNDS_MIN = -4f;
    private const float Y_BOUNDS_MAX = 4f;

    private static readonly Vector3 PLAYER_START_POS = new Vector3(-3.5f, 0, 0);

    [SerializeField] private GameObject obstacles;
    [SerializeField] private Flappy player;
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private GameObject bumpEffectPrefab;

    public enum State
    {
        Login,
        Selecting,
        Selected,
        Playing,
        Completed
    }

    public float Distance => -obstacles.transform.localPosition.x;
    public Flappy Player => player;
    public State CurrentState => _state;
    public IWalletInfo Wallet => _wallet;
    public INFT NFT => _nft;
    static public Game Current { get; private set; }

    private State _state = State.Login;
    private IWalletInfo _wallet;
    private INFT _nft;

    public void CompleteSelection()
    {
        _state = State.Selected;
    }

    public void AssignNft(INFT nft)
    {
        _nft = nft;

        if (_nft != null)
        {
            var id = nft.GetNFTId();
            var assetId = nft.GetNFTTypeId();

            uint hash = SigningUtils.GetStringHashCode(id + assetId);

            uint sizeSource = hash & 0xff;
            uint speedSource = (hash >> 8) & 0xff;
            uint strSource = (hash >> 16) & 0xff;
            uint gravitySource = (hash >> 24) & 0xff;
            uint luckSource = (hash >> 32) & 0xff;

            player.strength = ToRange(strSource, 30f, 60f);
            player.size = ToRange(sizeSource, 0.7f, 1.3f);
            player.speed = ToRange(speedSource, 1f, 1.5f);
            player.gravity = ToRange(gravitySource, 1f, 1.5f);
            player.luck = ToRange(luckSource, 0f, 0.3f);
        } else
        {
            FillDefaultStats();
        }

        player.UpdateStats();
    }

    public void Login(IWalletInfo wallet)
    {
        _wallet = wallet;
        _state = State.Selecting;
    }

    public void Restart(bool needNftSelection)
    {
        _state = needNftSelection ? State.Selecting : State.Selected;
        PrepareForNewGame();
    }

    private void Awake()
    {
        Obstacle.OnTriggerCollision += Obstacle_OnTriggerCollision;

        Current = this;
    }

    private void Start()
    {
        PrepareForNewGame();
    }

    private void Obstacle_OnTriggerCollision(GameObject obj)
    {
        CompleteGame();
    }

    private void FixedUpdate()
    {
        if (_state == State.Selecting || _state == State.Login)
            return;

        bool keyPressed = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        if (_state == State.Selected)
        {
            if (keyPressed)
                StartGame();
        } else if (_state == State.Playing)
        {
            HandleLogicTick(keyPressed);
        }
    }


    public void HandleLogicTick(bool jump) {
        MoveMap();
        GenerateObstacles();

        if (!IsInPlayableArea())
            CompleteGame();
        else
        {
            if (jump)
                player.Body.AddForce(new Vector2(0, player.strength), ForceMode2D.Force);
        }
    }

    private bool IsInPlayableArea()
    {
        return player.Body.transform.position.y < Y_BOUNDS_MAX && 
               player.Body.transform.position.y > Y_BOUNDS_MIN;
    }

    private void GenerateObstacles()
    {
        while (Obstacle.Count < 10) {
            Obstacle.Generate(obstacles, obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)],
                OBSTACLE_MIN_RANGE + player.luck, OBSTACLE_MAX_RANGE + player.luck);
        }
    }

    private void PrepareForNewGame()
    {
        // clear all remembered controls to avoid triggering interface buttons with 'space'
        EventSystem.current?.SetSelectedGameObject(null); 

        player.transform.localPosition = PLAYER_START_POS;
        player.ResetValues();

        obstacles.transform.localPosition = Vector3.zero;

        Obstacle.ClearAll();
        GenerateObstacles();
    }

    private void MoveMap()
    {
        var p = obstacles.transform.localPosition;
        obstacles.transform.localPosition = new Vector3(p.x - SPEED_COEFF * player.speed, 0, 0);
    }

    public void StartGame()
    {
        _state = State.Playing;
        player.Body.simulated = true;
    }

    private void CompleteGame()
    {
        var score = (int)Math.Floor(Distance);
        HighScores.Add(_nft != null ? _nft.Name : "No nft", score);

        if (!Application.isEditor)
        {
#if UNITY_WEBGL
            PortalJS.SendComplete(score, GameLoader.Token);
#endif
        }

        _state = State.Completed;
        player.Complete();

        if (bumpEffectPrefab != null)
            Instantiate(bumpEffectPrefab, player.Body.transform.position, Quaternion.identity);
    }
    private float ToRange(uint source, float min, float max)
    {
        float fSource = (source % 20) / 19f;
        return Mathf.Lerp(min, max, fSource);
    }

    private void FillDefaultStats()
    {
        player.strength = 30f;
        player.size = 1.3f;
        player.speed = 1.5f;
        player.gravity = 1f;
        player.luck = 0f;
    }
}
