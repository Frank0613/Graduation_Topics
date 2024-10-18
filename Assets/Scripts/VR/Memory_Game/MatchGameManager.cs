using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchGameManager : MonoBehaviour
{
    [Header("Game Rules")]
    [SerializeField] private float GameTime = 0.0f;
    private float remainingTime;
    [SerializeField] private Touch_Card[] cards;
    private List<Touch_Card> flippedCards = new List<Touch_Card>();
    private bool isChecking = false;

    [Header("UI Settings")]
    [SerializeField] private GameObject Progress_Bar;
    [SerializeField] private GameObject Tutorial;
    [SerializeField] private GameObject Start_Button;
    [SerializeField] private RectTransform Fill_Area;
    [SerializeField] private GameObject Vague_Photo;
    private float initialWidth;

    [Header("Test Mode Setting")]
    [SerializeField] private bool Test_Mode = false;
    [SerializeField] private KeyCode[] keyCodes;

    // Game State Settings
    private enum Game_Status
    {
        WaitForMatchGameStart,
        MatchGameStart,
        MatchGameInProgress,
        MatchGameEnd,
    }
    private Game_Status state = Game_Status.WaitForMatchGameStart;
    private Coroutine gameInProgressCoroutine;
    public EventHandler OnMatchGame_StateChanged;
    void Start()
    {
        OnMatchGame_StateChanged += Handle_MatchGameStateChanged;

        // Initialize
        remainingTime = GameTime;
        initialWidth = Fill_Area.sizeDelta.x;
        Vague_Photo.SetActive(true);

        // Hide UI when start   
        Display_MatchGame_Tutorial(false);
        Display_MatchGame_UI(false);
    }
    void Update()
    {
        TestMode();
    }

    //Event Settings
    void Handle_MatchGameStateChanged(object sender, EventArgs e)
    {
        GameStatus_Detect();
    }
    #region Cards Functions
    public void RotateCard(Touch_Card card)
    {
        if (!card.isRotating && !isChecking && !flippedCards.Contains(card))
        {
            // Rotate.
            card.Rotate(180f, 0.5f);
            // Add two cards into the flippedCards list.
            flippedCards.Add(card);
            if (flippedCards.Count == 2)
            {
                // Match two cards
                StartCoroutine(CheckMatch());
            }
        }
    }
    private IEnumerator CheckMatch()
    {
        isChecking = true;

        yield return new WaitForSeconds(1f);

        if (flippedCards[0].card_Tag == flippedCards[1].card_Tag)
        {
            foreach (Touch_Card card in flippedCards)
            {
                // If two cards is the same
                card.gameObject.SetActive(false);
                // write the code here when finish ...
            }
        }
        else
        {
            // If two cards is not the same, turning back
            foreach (Touch_Card card in flippedCards)
            {
                card.Rotate(-180f, 0.5f);
            }
        }
        flippedCards.Clear();
        isChecking = false;
    }
    private bool AreAllCardsInState(bool checkForActive) //Detect if all the cards are active or inactive.
    {
        foreach (Touch_Card card in cards)
        {
            if (card.gameObject.activeInHierarchy != checkForActive)
            {
                return false;
            }
        }
        return true;
    }
    #endregion

    #region UI
    private void UpdateFillArea()
    {
        float fillAmount = remainingTime / GameTime;
        Vector2 sizeDelta = Fill_Area.sizeDelta;
        sizeDelta.x = initialWidth * fillAmount;
        Fill_Area.sizeDelta = sizeDelta;
    }
    public void Display_MatchGame_UI(bool show)
    {
        Progress_Bar.SetActive(show);
        foreach (Touch_Card card in cards)
        {
            card.gameObject.SetActive(show);
        }
    }
    public void Display_MatchGame_Tutorial(bool show)
    {
        Tutorial.SetActive(show);
        Start_Button.SetActive(show);

    }
    #endregion

    #region Game State
    public void ChangeGameStatus(string nextStatusIndex)
    {
        if (Enum.TryParse(nextStatusIndex, out Game_Status _nextStatusIndex))
        {
            state = _nextStatusIndex;
            OnMatchGame_StateChanged?.Invoke(this, EventArgs.Empty);
        }

    }
    private void GameStatus_Detect()
    {
        switch (state)
        {
            case Game_Status.MatchGameStart:
                //=============================================== Change the state.
                Display_MatchGame_Tutorial(false);
                Display_MatchGame_UI(true);

                if (AreAllCardsInState(true))
                {
                    state = Game_Status.MatchGameInProgress;
                    OnMatchGame_StateChanged?.Invoke(this, EventArgs.Empty);

                }
                break;

            case Game_Status.MatchGameInProgress:
                if (gameInProgressCoroutine == null)
                {
                    gameInProgressCoroutine = StartCoroutine(GameInProgress_Coroutine());
                }
                break;

            case Game_Status.MatchGameEnd:
                Stop_GameInProgress_Coroutine();
                Display_MatchGame_UI(false);
                Display_MatchGame_Tutorial(false);
                Vague_Photo.SetActive(false);
                break;

        }
    }
    private IEnumerator GameInProgress_Coroutine()
    {
        // ============================================== Start counting times.
        while (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
            UpdateFillArea();

            //=============================================== If matches all the cards, game end. 
            if (AreAllCardsInState(false) || remainingTime <= 0.01f)
            {
                state = Game_Status.MatchGameEnd;
                OnMatchGame_StateChanged?.Invoke(this, EventArgs.Empty);
                yield break;
            }
            yield return null;
        }
    }
    private void Stop_GameInProgress_Coroutine()
    {
        if (gameInProgressCoroutine != null)
        {
            StopCoroutine(GameInProgress_Coroutine());
            gameInProgressCoroutine = null;
        }
    }
    #endregion

    #region TestMode
    private void TestMode()
    {
        if (Test_Mode)
        {
            // Test mode settings.
            for (int i = 0; i < cards.Length; i++)
            {
                if (Input.GetKeyDown(keyCodes[i]))
                {
                    RotateCard(cards[i]);
                }
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                ChangeGameStatus("MatchGameStart");
            }
        }

    }
    #endregion

}
