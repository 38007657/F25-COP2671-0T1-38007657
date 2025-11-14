using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages the Bank/Money tab showing balance, transaction history, and stats
/// </summary>
public class BankTabManager : MonoBehaviour
{
    [Header("Balance Display")]
    [SerializeField] private TextMeshProUGUI currentBalanceText;
    [SerializeField] private TextMeshProUGUI totalEarnedText;
    [SerializeField] private TextMeshProUGUI totalSpentText;

    [Header("Transaction History")]
    [SerializeField] private Transform transactionContainer;
    [SerializeField] private GameObject transactionEntryPrefab;
    [SerializeField] private int maxTransactionsShown = 20;

    [Header("Stats")]
    [SerializeField] private TextMeshProUGUI totalCropsSoldText;
    [SerializeField] private TextMeshProUGUI totalSeedsPurchasedText;
    [SerializeField] private TextMeshProUGUI mostProfitableCropText;

    // Track transactions
    private List<Transaction> transactionHistory = new List<Transaction>();
    private int totalEarned = 0;
    private int totalSpent = 0;
    private Dictionary<string, int> cropSalesCount = new Dictionary<string, int>();
    private Dictionary<string, int> cropEarnings = new Dictionary<string, int>();

    private void Start()
    {
        // Subscribe to inventory events
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnMoneyChanged += OnMoneyChanged;
        }

        RefreshDisplay();
    }

    private void OnEnable()
    {
        RefreshDisplay();
    }

    private void OnDestroy()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnMoneyChanged -= OnMoneyChanged;
        }
    }

    /// <summary>
    /// Refresh all displays
    /// </summary>
    private void RefreshDisplay()
    {
        UpdateBalanceDisplay();
        UpdateTransactionHistory();
        UpdateStats();
    }

    /// <summary>
    /// Update balance display
    /// </summary>
    private void UpdateBalanceDisplay()
    {
        if (PlayerInventory.Instance == null) return;

        int currentBalance = PlayerInventory.Instance.CurrentMoney;

        if (currentBalanceText != null)
        {
            currentBalanceText.text = CurrencyFormatter.FormatCoins(currentBalance);
        }

        if (totalEarnedText != null)
        {
            totalEarnedText.text = $"Earned: {CurrencyFormatter.FormatCoins(totalEarned)}";
        }

        if (totalSpentText != null)
        {
            totalSpentText.text = $"Spent: {CurrencyFormatter.FormatCoins(totalSpent)}";
        }
    }

    /// <summary>
    /// Update transaction history display
    /// </summary>
    private void UpdateTransactionHistory()
    {
        // Clear existing entries
        foreach (Transform child in transactionContainer)
        {
            Destroy(child.gameObject);
        }

        // Show most recent transactions first
        int count = Mathf.Min(maxTransactionsShown, transactionHistory.Count);
        for (int i = transactionHistory.Count - 1; i >= transactionHistory.Count - count; i--)
        {
            Transaction transaction = transactionHistory[i];
            CreateTransactionEntry(transaction);
        }
    }

    /// <summary>
    /// Create a transaction entry in the list
    /// </summary>
    private void CreateTransactionEntry(Transaction transaction)
    {
        if (transactionEntryPrefab == null || transactionContainer == null) return;

        GameObject entryObj = Instantiate(transactionEntryPrefab, transactionContainer);

        // Find text components (adjust names based on your prefab)
        TextMeshProUGUI typeText = entryObj.transform.Find("TypeText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI descriptionText = entryObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI amountText = entryObj.transform.Find("AmountText")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI timeText = entryObj.transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();

        if (typeText != null)
        {
            typeText.text = transaction.isIncome ? "SALE" : "PURCHASE";
            typeText.color = transaction.isIncome ? Color.green : Color.red;
        }

        if (descriptionText != null)
        {
            descriptionText.text = transaction.description;
        }

        if (amountText != null)
        {
            string prefix = transaction.isIncome ? "+" : "-";
            amountText.text = $"{prefix}{CurrencyFormatter.FormatCoinsCompact(transaction.amount)}";
            amountText.color = transaction.isIncome ? Color.green : Color.red;
        }

        if (timeText != null)
        {
            timeText.text = transaction.timestamp;
        }
    }

    /// <summary>
    /// Update stats display
    /// </summary>
    private void UpdateStats()
    {
        if (totalCropsSoldText != null)
        {
            int totalSold = 0;
            foreach (int count in cropSalesCount.Values)
            {
                totalSold += count;
            }
            totalCropsSoldText.text = $"Crops Sold: {totalSold}";
        }

        if (mostProfitableCropText != null)
        {
            string mostProfitable = "None";
            int highestEarnings = 0;

            foreach (var kvp in cropEarnings)
            {
                if (kvp.Value > highestEarnings)
                {
                    highestEarnings = kvp.Value;
                    mostProfitable = kvp.Key;
                }
            }

            mostProfitableCropText.text = $"Best Crop: {mostProfitable}";
        }
    }

    /// <summary>
    /// Add a transaction (call this when money changes)
    /// </summary>
    public void AddTransaction(string description, int amount, bool isIncome, string itemName = "")
    {
        Transaction transaction = new Transaction
        {
            description = description,
            amount = amount,
            isIncome = isIncome,
            timestamp = System.DateTime.Now.ToString("HH:mm:ss")
        };

        transactionHistory.Add(transaction);

        // Update totals
        if (isIncome)
        {
            totalEarned += amount;

            // Track crop sales
            if (!string.IsNullOrEmpty(itemName))
            {
                if (!cropSalesCount.ContainsKey(itemName))
                {
                    cropSalesCount[itemName] = 0;
                    cropEarnings[itemName] = 0;
                }
                cropSalesCount[itemName]++;
                cropEarnings[itemName] += amount;
            }
        }
        else
        {
            totalSpent += amount;
        }

        RefreshDisplay();
    }

    /// <summary>
    /// Called when money changes
    /// </summary>
    private void OnMoneyChanged(int newAmount)
    {
        UpdateBalanceDisplay();
    }

    /// <summary>
    /// Clear all history (reset button)
    /// </summary>
    public void ClearHistory()
    {
        transactionHistory.Clear();
        totalEarned = 0;
        totalSpent = 0;
        cropSalesCount.Clear();
        cropEarnings.Clear();
        RefreshDisplay();
    }
}

/// <summary>
/// Simple transaction data structure
/// </summary>
[System.Serializable]
public class Transaction
{
    public string description;
    public int amount;
    public bool isIncome; // true = income, false = expense
    public string timestamp;
}