using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Plant_Progress : MonoBehaviour
{
    [Header("UI Objects Setting")]
    [SerializeField] private Slider progressBar;

    [SerializeField] private Image[] Count_Success_Dots;
    [SerializeField] private RectTransform Fill_Area;

    [Header("GameRule Setting")]
    [SerializeField] private float Cursor_Speed = 1f;
    [SerializeField] private float[] Every_Level;

    // progressBar Value
    private float Total_PosX;
    private float Start_PosX_To_Normal;
    private float End_PosX_To_Normal;

    // Game State Values
    public int Level_index { get; private set; } = 0;
    public bool In_Range { get; private set; } = false;
    public static event EventHandler OnGameStateChange;

    // Game State Events       
    public event EventHandler OnEnterRange;
    public event EventHandler OnExitRange;

    void Start()
    {
        Initialized_ProgressBar();
        Change_Level();

    }
    void Update()
    {
        ProgressBar_PingPong();
        Detect_Wether_Spray_IsFinished();
        Detect_Wether_In_Range();
    }
    #region Initialization
    private void Initialized_ProgressBar()
    {
        // Reset the level.
        Level_index = 0;

        // Get total width of the progress bar
        Total_PosX = progressBar.GetComponent<RectTransform>().rect.width;

        // OnValueChanged event settings 
        //progressBar.onValueChanged.AddListener(OnProgressBarValueChanged);

        // Success_Dot settings 
        for (int i = 0; i < Count_Success_Dots.Length; i++)
        {
            Change_Success_Dot(i, 0);
        }
    }
    private void OnProgressBarValueChanged(float value)
    {
        //Detect_Wether_In_Range();
        //Detect_Wether_Spray_IsFinished();
    }
    #endregion
    private void ProgressBar_PingPong()
    {
        if (progressBar != null)
        {
            progressBar.value = Mathf.PingPong(Time.time * Cursor_Speed, 1f); // Cursor movement

        }
        else
        {
            Debug.LogError("No progressBar being assigned.");
        }
    }
    #region State Detect Function
    private void Detect_Wether_In_Range()
    {
        bool newInRange = progressBar.value >= Start_PosX_To_Normal && progressBar.value <= End_PosX_To_Normal;
        if (newInRange != In_Range)
        {
            In_Range = newInRange;

            // Weather in range
            if (In_Range)
            {
                OnEnterRange?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                OnExitRange?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    private void Detect_Wether_Spray_IsFinished()
    {
        if (progressBar.value >= 1 || Level_index > Every_Level.Length)
        {
            // When GameOver
            Level_index = 0;
            GameStateManager.Instance.game_Status = GameStateManager.Game_Status.SprayGameEnd;
            OnGameStateChange?.Invoke(this, EventArgs.Empty);


        }
    }
    #endregion

    #region Level Function
    private void Level(float fill_size)
    {
        // Fill size settings.
        Vector2 sizeDelta = Fill_Area.sizeDelta;
        sizeDelta.x = fill_size;
        Fill_Area.sizeDelta = sizeDelta;

        // Fill size spawning range setting.
        float maxSpawnPosX = Total_PosX - fill_size;
        float startPosX = UnityEngine.Random.Range(0, maxSpawnPosX);
        Fill_Area.anchoredPosition = new Vector2(startPosX, Fill_Area.anchoredPosition.y);

        //Normalization
        Start_PosX_To_Normal = startPosX / Total_PosX;
        End_PosX_To_Normal = (startPosX + fill_size) / Total_PosX;
    }
    public void Change_Level()
    {
        Level_index++;
        if (Level_index <= Every_Level.Length)
        {
            Level(Every_Level[Level_index - 1]);
        }

    }
    #endregion

    #region UI Change Function
    public void Change_Success_Dot(int dot_index, float alpha)
    {
        Color color = Count_Success_Dots[dot_index].color;
        color.a = alpha;
        Count_Success_Dots[dot_index].color = color;
    }

    #endregion
    void OnDisable()
    {
        //Remove Listener
        progressBar.onValueChanged.RemoveListener(OnProgressBarValueChanged);
    }

}
