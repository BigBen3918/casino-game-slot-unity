public class APIForm
{
    public bool status;
    public int[] winpaylines;
    public int[] randomNums;
    public int moneyResult;
    public string message;
}

public class GlobalVariable
{
    public string BaseUrl = "http://45.132.242.139/";

    // Game Status Variable
    public int _lines;
    public int _bet;
    public float _myBalance;
    public int _totalBet;
    public int _lastWinBalance;
    public string _token;
}