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
    [SerializeField] TMP_InputField _linesText;
    [SerializeField] TMP_InputField _betText;
    [SerializeField] TextMeshProUGUI _totalBetText;

    [Header("Buttons")]
    [SerializeField] Button _spinButton;
    [SerializeField] Button _linesButtonPlus;
    [SerializeField] Button _linesButtonMinus;
    [SerializeField] Button _betButtonPlus;
    [SerializeField] Button _betButtonMinus;

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
        _linesText.onValueChanged.AddListener(lineChange);
        _betText.onValueChanged.AddListener(betChange);

        globalVariable = new GlobalVariable();

        globalVariable._lines = 20;
        globalVariable._bet = 100;
        globalVariable._totalBet = globalVariable._bet * globalVariable._lines;
        globalVariable._myBalance = 10000f;

#if UNITY_WEBGL == true && UNITY_EDITOR == false
    GameController("Ready");
#endif

        globalVariable._token = "token";
        globalVariable._myBalance = float.Parse("100000");
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
        if (globalVariable._bet == 10)
        {
            _betButtonPlus.interactable = true;
            _betButtonMinus.interactable = false;
        }
        if (globalVariable._bet == 10000)
        {
            _betButtonPlus.interactable = false;
            _betButtonMinus.interactable = true;
        }
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
            _spinButton.interactable = false;
            gameManager.StartSpin();
        }
    }

    public void setEnableSpin()
    {
        _spinButton.interactable = true;
    }

    private void InitialText()
    {
        _myBalanceText.text = globalVariable._myBalance.ToString("0.00");
        _linesText.text = globalVariable._lines.ToString();
        _betText.text = globalVariable._bet.ToString();
        _totalBetText.text = globalVariable._totalBet.ToString("0.00");

        gameManager.ToggleLines();
        CalculateTotalBet();
    }

    private void BetMinusClick()
    {
        globalVariable._bet = Mathf.Clamp(globalVariable._bet - 10, 10, 10000);
        _betText.text = globalVariable._bet.ToString();
        
        _betButtonPlus.interactable = true;

        CalculateTotalBet();
    }

    private void BetPlusClick()
    {
        globalVariable._bet = Mathf.Clamp(globalVariable._bet + 10, 10, 10000);
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

    void lineChange(string inputData)
    {
        if(string.IsNullOrEmpty(inputData))
        {
            globalVariable._lines = 1;
            _linesText.text = "1";

            gameManager.ToggleLines();
            CalculateTotalBet();
            return;
        }
        else
        {
            if ( int.Parse(inputData) > 25)
            {
                globalVariable._lines = 25;
                _linesText.text = "25";

                gameManager.ToggleLines();
                CalculateTotalBet();
                return;
            }

            if( int.Parse(inputData) < 1)
            {
                globalVariable._lines = 1;
                _linesText.text = "1";

                gameManager.ToggleLines();
                CalculateTotalBet();
                return;
            }
        }


        globalVariable._lines =  int.Parse(inputData);

        _linesButtonPlus.interactable = true;
        _linesButtonMinus.interactable = true;
        gameManager.ToggleLines();
        CalculateTotalBet();
    }

    void betChange(string inputData)
    {
        if (string.IsNullOrEmpty(inputData))
        {
            globalVariable._bet = 10;
            _betText.text = "10";

            CalculateTotalBet();
            return;
        }
        else
        {
            if (int.Parse(inputData) > 10000)
            {
                globalVariable._bet = 10000;
                _betText.text = "10000";

                CalculateTotalBet();
                return;
            }

            if (int.Parse(inputData) < 10)
            {
                globalVariable._bet = 10;
                _betText.text = "10";

                CalculateTotalBet();
                return;
            }
        }

        globalVariable._bet = int.Parse(inputData);
        _betText.text = globalVariable._bet.ToString();
        _betButtonMinus.interactable = true;
        _betButtonPlus.interactable = true;
        CalculateTotalBet();
    }
}
