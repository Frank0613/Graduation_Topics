using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class SprayGameManager : MonoBehaviour
{


    public static SprayGameManager Instance;

    [Header("GameObject Setting")]
    [SerializeField] private GameObject SprayBottle;
    private Vector3 SprayBottle_Init_Pos;
    [SerializeField] private GameObject SprayBottle_UI;
    [SerializeField] private GameObject Tutorial_UI;

    [Header("TestMode Setting")]
    [Tooltip("Press T")][SerializeField] private bool TestMode;

    [SerializeField] float RotateAngle;
    [SerializeField] private GameStateManager gameStateManager;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        gameStateManager = GetComponent<GameStateManager>();

        SprayBottle_Init_Pos = SprayBottle.transform.position;

        Display_SprayBottle_UI(false);
        Display_SprayGameTutorial(false);

    }

    void Update()
    {
        Show_UI_When_Pick();
        TestModeFunction();
    }
    public void AfterSprayGame()
    {
        StartCoroutine(SprayGameEnd_Coroutine());
    }

    IEnumerator SprayGameEnd_Coroutine()
    {
        yield return new WaitForSeconds(1);
        Display_SprayBottle_UI(false);
        Display_SprayGameTutorial(false);
        yield return new WaitForSeconds(1);
        Player_Movement.instance.Rotation_Player(Player_Movement.instance.gameObject, RotateAngle);

    }
    private void Show_UI_When_Pick()
    {
        if (SprayBottle.transform.position != SprayBottle_Init_Pos && GameStateManager.Instance.game_Status == GameStateManager.Game_Status.SprayGameStart)
        {
            Display_SprayBottle_UI(true);
            SprayBottle_Init_Pos = Vector3.zero;
        }
    }
    public void Display_SprayBottle_UI(bool visible)
    {

        SprayBottle_UI.SetActive(visible);
    }
    public void Display_SprayGameTutorial(bool visible)
    {
        Tutorial_UI.SetActive(visible);
    }


    #region TestMode
    private void TestModeFunction()
    {
        if (TestMode)
        {
            if (Input.GetKey(KeyCode.T) && SprayBottle.activeInHierarchy && SprayBottle_UI.activeInHierarchy)
            {
                SprayBottle.GetComponent<Water_Spray>().ComputeUseStrength(1);
            }
            else
            {
                SprayBottle.GetComponent<Water_Spray>().ComputeUseStrength(0);
            }
        }
    }
    #endregion
}
