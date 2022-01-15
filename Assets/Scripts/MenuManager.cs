using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using SimpleJSON;

public class MenuManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void GameController(string msg);

    [Header("Text")]
    [SerializeField] TextMeshProUGUI _myBalanceText;
    [SerializeField] TextMeshProUGUI _linesText;
    [SerializeField] TextMeshProUGUI _betText;
    [SerializeField] TextMeshProUGUI _totalBetText;
    [SerializeField] TextMeshProUGUI _lastWinText;

    [Header("Buttons")]
    [SerializeField] Button _spinButton;
    [SerializeField] Button _linesButtonPlus;
    [SerializeField] Button _linesButtonMinus;
    [SerializeField] Button _betButtonPlus;
    [SerializeField] Button _betButtonMinus;
    [SerializeField] Button _betButtonMax;    

    private GameManager gameManager;
    public static GlobalVariable globalVariable;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();

        _spinButton.onClick.AddListener(SpinHandler);
        _linesButtonPlus.onClick.AddListener(LinePlusClick);
        _linesButtonMinus.onClick.AddListener(LineMinusClick);
        _betButtonPlus.onClick.AddListener(BetPlusClick);
        _betButtonMinus.onClick.AddListener(BetMinusClick);
        _betButtonMax.onClick.AddListener(BetMax);

        globalVariable = new GlobalVariable();

        globalVariable._lines = 20;
        globalVariable._bet = 100;
        globalVariable._totalBet = globalVariable._bet * globalVariable._lines;
        globalVariable._lastWinBalance = 0;

#if UNITY_WEBGL == true && UNITY_EDITOR == false
    GameController("Ready");
#endif

        InitialText();
    }

    // Update is called once per frame
    void Update()
    {
        if (globalVariable._lines == 25)
        {
            _linesButtonPlus.interactable = false;
            _linesButtonMinus.interactable = true;
        }
        if (globalVariable._lines == 1)
        {
            _linesButtonPlus.interactable = true;
            _linesButtonMinus.interactable = false;
        }
        if (globalVariable._bet == 100)
        {
            _betButtonPlus.interactable = true;
            _betButtonMinus.interactable = false;
        }
        if (globalVariable._bet == 1500)
        {
            _betButtonPlus.interactable = false;
            _betButtonMinus.interactable = true;
        }
        _lastWinText.text = globalVariable._lastWinBalance.ToString("0.00");
        _myBalanceText.text = globalVariable._myBalance.ToString("0.00");
        _totalBetText.text = globalVariable._totalBet.ToString("0.00");
    }

    private void SpinHandler()
    {
        if (globalVariable._myBalance < (float)globalVariable._totalBet)
        {
#if UNITY_WEBGL == true && UNITY_EDITOR == false
    GameController("Control");
#endif
        }
        else
        {
            gameManager.StartSpin();
        }
    }

    private void InitialText()
    {
        _myBalanceText.text = globalVariable._myBalance.ToString("0.00");
        _linesText.text = globalVariable._lines.ToString();
        _betText.text = globalVariable._bet.ToString("0.00");
        _totalBetText.text = globalVariable._totalBet.ToString("0.00");
        _lastWinText.text = globalVariable._lastWinBalance.ToString("0.00");

        gameManager.ToggleLines();
        CalculateTotalBet();
    }

    private void BetMinusClick()
    {
        globalVariable._bet = Mathf.Clamp(globalVariable._bet - 100, 100, 1500);
        _betText.text = globalVariable._bet.ToString();
        
        _betButtonPlus.interactable = true;

        CalculateTotalBet();
    }

    private void BetPlusClick()
    {
        globalVariable._bet = Mathf.Clamp(globalVariable._bet + 100, 100, 1500);
        _betText.text = globalVariable._bet.ToString();

        _betButtonMinus.interactable = true;

        CalculateTotalBet();
    }

    private void LineMinusClick()
    {
        globalVariable._lines = Mathf.Clamp(--globalVariable._lines, 1, 25);
        _linesText.text = globalVariable._lines.ToString();

        _linesButtonPlus.interactable = true;

        gameManager.ToggleLines();
        CalculateTotalBet();
    }

    private void LinePlusClick()
    {
        globalVariable._lines = Mathf.Clamp(++globalVariable._lines, 1, 25);
        _linesText.text = globalVariable._lines.ToString();

        _linesButtonMinus.interactable = true;

        gameManager.ToggleLines();
        CalculateTotalBet();
    }

    private void BetMax()
    {
        globalVariable._lines = 25;
        _linesText.text = globalVariable._lines.ToString();

        globalVariable._bet = 1500;
        _betText.text = globalVariable._bet.ToString("0.00");

        gameManager.ToggleLines();
        CalculateTotalBet();
    }

    private void CalculateTotalBet()
    {
        globalVariable._totalBet = globalVariable._lines * globalVariable._bet;
    }

    public void RequestToken(string data)
    {
        JSONNode usersInfo = JSON.Parse(data);
        globalVariable._token = usersInfo["token"];
        globalVariable._myBalance = float.Parse(usersInfo["amount"]);
    }
}
