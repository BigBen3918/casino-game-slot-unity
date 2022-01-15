using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

    [Header("Text")]
    [SerializeField] TextMeshProUGUI _WinBalance;

    [Header("Button")]
    [SerializeField] Transform _payLinesParent;

    [Header("Components")]
    [SerializeField] Animator _animator;

    [Header("Winning")]
    [SerializeField] List<GameObject> _winningIcons;
    [SerializeField] List<Transform> _winningSlots;
    [SerializeField] List<GameObject> _winningFrames;
    [SerializeField] GameObject _bigWinAnimation;

    List<int[,]> winningCombinations = new List<int[,]>();
    List<int> winningCombinationLines = new List<int>();

    public static APIForm apiform;

    private bool _spinning;
    private float _spinTime;

    private bool _guiActived {
                get { return _bigWinAnimation.activeSelf; }
                set { }
    }

    public void StartSpin()
    {
        if (_spinning) return;
            StartCoroutine(SendSignal());
    }

    private IEnumerator SendSignal()
    {
        WWWForm form = new WWWForm();
        form.AddField("token", MenuManager.globalVariable._token);
        form.AddField("totalBet", MenuManager.globalVariable._totalBet);
        form.AddField("betLine", MenuManager.globalVariable._lines);
        form.AddField("betValue", MenuManager.globalVariable._bet);

        UnityWebRequest www = UnityWebRequest.Post(MenuManager.globalVariable.BaseUrl + "api/start-signal", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            string strdata = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
            apiform = JsonUtility.FromJson<APIForm>(strdata);

            if (apiform.status == true)
            {
                // Spin Start
                SlotMachineReady();
                _spinning = true;
            }
            else
            {
                Debug.Log(apiform.message);
            }
        }
    }

    private IEnumerator Spin()
    {
        _animator.SetTrigger("reels_start");

        yield return new WaitForSeconds(0.3f);

        for (int i = 0; i < _winningSlots.Count; i++)
        {
            //Destroy childs of winning slot 
            for (int j = 0; j < _winningSlots[i].childCount; j++)
            {
                _winningSlots[i].GetComponent<Image>().enabled = true;
                Destroy(_winningSlots[i].GetChild(0).gameObject);
            }
        }

        ToggleSlotsBlur(true);
        _spinTime = Random.Range(1f, 2.5f);
        yield return new WaitForSeconds(_spinTime - 0.3f);
        ToggleSlotsBlur(false);

        yield return new WaitForSeconds(0.45f);
        _animator.SetTrigger("reels_end");
        yield return new WaitForSeconds(0.15f);

        SetWinningCombination();

        yield return new WaitForSeconds(0.5f);
        EnableWiningFrames();
        yield return new WaitForSeconds(0.5f);

        if(winningCombinationLines.Count > 0)
        {
            _WinBalance.text = apiform.moneyResult.ToString("$0.00");
            _bigWinAnimation.SetActive(true);
            MenuManager.globalVariable._lastWinBalance = apiform.moneyResult;
            MenuManager.globalVariable._myBalance += (float)apiform.moneyResult;
        }

        _spinning = false;
    }

    private void SetWinningCombination()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var idx = winningCombinations[0][j, i];

                _winningSlots[j + i * 3].GetComponent<Image>().enabled = false;

                var obj = Instantiate(_winningIcons[Math.Abs(idx) - 1], _winningSlots[j + i * 3]);
                obj.transform.localPosition = Vector3.zero;
                obj.GetComponent<Animation>().Stop();
                foreach (Transform child in obj.transform) {
                    var anim = child.GetComponent<Animation>();
                    if (anim) anim.Stop();
                }
            }
        }
    }

    private void EnableWiningFrames()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var idx = winningCombinations[0][j, i];
                if (idx >= 0)
                {
                    var slot = _winningSlots[j + i * 3];
                    var anims = slot.GetComponentsInChildren<Animation>();
                    for (int k = 0; k < anims.Length; k++)
                        anims[k].Play();
                    _winningFrames[j + i * 3].SetActive(true);
                }
            }
        }

        for(int i = 0; i < winningCombinationLines.Count; i++)
        {
            var line = _payLinesParent.GetChild(winningCombinationLines[i] - 1);
            line.GetComponent<Animator>().SetTrigger("Highlighted");
        }
    }

    private void ToggleSlotsBlur(bool en)
    {
        foreach (Transform child in _animator.transform)
        {
            foreach (Transform subChild in child.transform)
            {
                var anim = subChild.GetComponent<Animator>();
                if (anim)
                {
                    if (en)
                        anim.SetTrigger("start_blur");
                    else
                        anim.SetTrigger("stop_blur");
                }
            }
        }
    }

    private void Start()
    {
        _bigWinAnimation.SetActive(false);
    }

    // Game Ready
    private void SlotMachineReady()
    {
        InitialPaylineAndWinframe();
        winningCombinationLines.Clear();
        winningCombinations.Clear();

        MenuManager.globalVariable._myBalance -= (float)MenuManager.globalVariable._totalBet;

        winningCombinations.Add(new int[,] { { apiform.randomNums[0], apiform.randomNums[1], apiform.randomNums[2], apiform.randomNums[3], apiform.randomNums[4] },
                                             { apiform.randomNums[5], apiform.randomNums[6], apiform.randomNums[7], apiform.randomNums[8], apiform.randomNums[9] },
                                             { apiform.randomNums[10], apiform.randomNums[11], apiform.randomNums[12], apiform.randomNums[13], apiform.randomNums[14] }});

        for (int i = 0; i < apiform.winpaylines.Length; i++)
        {
            winningCombinationLines.Add(apiform.winpaylines[i]);
        }

        StartCoroutine(Spin());
    }

    // Format Paylines and WinFrames
    private void InitialPaylineAndWinframe()
    {
        for (int i = 0; i < winningCombinationLines.Count; i++)
        {
            // Set normal status
            var line = _payLinesParent.GetChild(winningCombinationLines[i] - 1);
            line.GetComponent<Animator>().SetTrigger("Normal");
        }

        for (int i = 0; i < _winningSlots.Count; i++)
        {
            // Disable winning frames
            for (int j = 0; j < _winningFrames.Count; j++)
                _winningFrames[j].SetActive(false);
        }
    }

    public void ToggleLines()
    {
        for (int i = 0; i < _payLinesParent.childCount; i++)
        {
            if (i < MenuManager.globalVariable._lines)
                _payLinesParent.GetChild(i).GetComponent<Button>().interactable = true;
            else
                _payLinesParent.GetChild(i).GetComponent<Button>().interactable = false;
        }
    }
}
