using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Linq;

public class UIManager : MonoBehaviour
{
    [Header("UI References")]
    public Text levelInfoText;
    public Text requiredExpText;
    public Text purchaseResultText;
    public Image expGaugeImage;

    [Header("Button References")]
    public Button enhanceButton;
    public Button bruteForceButton;
    public Button minWasteButton;
    public Button maxEfficiencyButton;
    public Button maxExpButton;

    private EnhancementSystem enhancementSystem;
    private int currentLevel = 1;
    private int currentExp = 0;
    private int requiredExp;
    private string selectedMode = "BruteForce";

    void Start()
    {
        enhancementSystem = new EnhancementSystem();
        UpdateEnhancementInfo(currentLevel);
        SetupButtons();

        purchaseResultText.text = "구매 방식을 선택하고 강화하세요.";
    }

    private void SetupButtons()
    {
        enhanceButton.GetComponentInChildren<Text>().text = "강화하기";
        bruteForceButton.GetComponentInChildren<Text>().text = "Brute Force";
        minWasteButton.GetComponentInChildren<Text>().text = "경험치 낭비 최소";
        maxEfficiencyButton.GetComponentInChildren<Text>().text = "골드 효율 최대";
        maxExpButton.GetComponentInChildren<Text>().text = "exp 큰 거 부터";

        enhanceButton.onClick.AddListener(OnEnhanceButtonClick);
        bruteForceButton.onClick.AddListener(() => OnModeSelect("BruteForce"));
        minWasteButton.onClick.AddListener(() => OnModeSelect("MinWaste"));
        maxEfficiencyButton.onClick.AddListener(() => OnModeSelect("MaxEfficiency"));
        maxExpButton.onClick.AddListener(() => OnModeSelect("MaxExp"));

        OnModeSelect(selectedMode);
    }

    public void OnModeSelect(string mode)
    {
        selectedMode = mode;

        EnhancementSystem.PurchaseResult result = new EnhancementSystem.PurchaseResult();
        int expToNextLevel = requiredExp - currentExp;

        switch (selectedMode)
        {
            case "BruteForce":
                result = enhancementSystem.CalculateOptimalBruteForce(expToNextLevel);
                break;
            case "MinWaste":
                result = enhancementSystem.CalculateGreedyMinWaste(expToNextLevel);
                break;
            case "MaxEfficiency":
                result = enhancementSystem.CalculateGreedyMaxEfficiency(expToNextLevel);
                break;
            case "MaxExp":
                result = enhancementSystem.CalculateGreedyMaxExp(expToNextLevel);
                break;
        }

        DisplayResult(result);
    }

    private void UpdateEnhancementInfo(int current)
    {
        currentLevel = current;
        int targetLevel = current + 1;
        requiredExp = EnhancementFormula.CalculateRequiredExp(targetLevel);

        levelInfoText.text = $"+{currentLevel} -> +{targetLevel}";
        requiredExpText.text = $"필요 경험치 {currentExp}/{requiredExp}";

        if (expGaugeImage != null && requiredExp > 0)
        {
            expGaugeImage.fillAmount = (float)currentExp / requiredExp;
        }
        else if (expGaugeImage != null)
        {
            expGaugeImage.fillAmount = 0.0f;
        }
    }

    private void DisplayResult(EnhancementSystem.PurchaseResult result)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("--- 구매 결과 ---");
        var stones = enhancementSystem.GetStones();
        bool purchased = false;

        foreach (var kvp in result.StoneCounts)
        {
            if (kvp.Value > 0)
            {
                EnhancementSystem.Stone stone = stones.Find(s => s.Name == kvp.Key);
                sb.AppendLine($"강화석 {kvp.Key} (exp{stone.Exp}) x {kvp.Value}");
                purchased = true;
            }
        }

        if (!purchased && result.TotalExp == 0)
        {
            sb.AppendLine("구매된 강화석이 없습니다.");
        }

        sb.AppendLine($"총 가격 : {result.TotalGold} gold");
        sb.AppendLine($"총 경험치: {result.TotalExp}");
        sb.AppendLine($"경험치 낭비: {result.WastedExp}");


        purchaseResultText.text = sb.ToString();
    }

    public void OnEnhanceButtonClick()
    {
        int expToNextLevel = requiredExp - currentExp;

        EnhancementSystem.PurchaseResult result = new EnhancementSystem.PurchaseResult();
        switch (selectedMode)
        {
            case "BruteForce":
                result = enhancementSystem.CalculateOptimalBruteForce(expToNextLevel);
                break;
            case "MinWaste":
                result = enhancementSystem.CalculateGreedyMinWaste(expToNextLevel);
                break;
            case "MaxEfficiency":
                result = enhancementSystem.CalculateGreedyMaxEfficiency(expToNextLevel);
                break;
            case "MaxExp":
                result = enhancementSystem.CalculateGreedyMaxExp(expToNextLevel);
                break;
        }

        int gainedExp = result.TotalExp;

        while (gainedExp > 0)
        {
            expToNextLevel = EnhancementFormula.CalculateRequiredExp(currentLevel + 1) - currentExp;

            if (expToNextLevel <= 0) // 현재 레벨의 필요 경험치가 0이라면 (만렙이거나 시작 레벨)
            {
                // 다음 레벨로 넘어갈 수 있도록 처리 (만렙이 아니라면)
                if (currentLevel < 9) // 가정: 최대 9강
                {
                    currentLevel += 1;
                    currentExp = 0;
                    expToNextLevel = EnhancementFormula.CalculateRequiredExp(currentLevel + 1);
                    if (expToNextLevel == 0) break; // 혹시라도 레벨업 후에도 필요 경험치가 0이라면 종료
                }
                else
                {
                    break;
                }
            }

            if (gainedExp >= expToNextLevel)
            {
                gainedExp -= expToNextLevel;
                currentLevel += 1;
                currentExp = 0;

                Debug.Log($"Level UP! New Level: {currentLevel}");

            }
            else
            {
                currentExp += gainedExp;
                gainedExp = 0;
            }
        }

        UpdateEnhancementInfo(currentLevel);

        purchaseResultText.text = $"강화 성공! (EXP {result.TotalExp} 획득)\n새로운 레벨: +{currentLevel}";

        OnModeSelect(selectedMode); 
    }
}