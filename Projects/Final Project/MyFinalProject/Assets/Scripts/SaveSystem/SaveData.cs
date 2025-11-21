using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data structure for saving/loading game state
/// </summary>
[System.Serializable]
public class SaveData
{
    // Save Metadata
    public string saveId; // Unique identifier
    public string saveName;
    public string saveDate;
    public float totalPlayTime; // In seconds

    // Player Data
    public int playerMoney;
    public float playerPositionX;
    public float playerPositionY;
    public float playerPositionZ;

    // Time Data
    public int currentDay;
    public float currentTime; // 0-24 hours

    // Inventory Data
    public List<SavedInventoryItem> harvestedItems = new List<SavedInventoryItem>();
    public List<SavedSeedPacket> seedPackets = new List<SavedSeedPacket>();

    // Crop Data
    public List<SavedCropBlock> plantedCrops = new List<SavedCropBlock>();

    public SaveData()
    {
        saveId = System.Guid.NewGuid().ToString();
        saveDate = System.DateTime.Now.ToString("MMM dd, yyyy • h:mm tt");
    }
}

[System.Serializable]
public class SavedInventoryItem
{
    public string itemName;
    public int quantity;
}

[System.Serializable]
public class SavedSeedPacket
{
    public string seedName;
    public int quantity;
}

[System.Serializable]
public class SavedCropBlock
{
    // Position
    public int gridPositionX;
    public int gridPositionY;

    // State
    public bool isTilled;
    public bool isWatered;
    public bool isPlanted;
    public bool isWilted;

    // Crop info
    public string seedPacketName;
    public int currentGrowthStage;
    public float growthTimer;

    // Time tracking
    public int dayPlanted;
    public int lastWateredDay;
    public int daysWithoutWater;

    // Hour-based growth tracking
    public float timePlanted;
    public float hoursGrown;
}

/// <summary>
/// Save slot info for UI display
/// </summary>
[System.Serializable]
public class SaveSlotInfo
{
    public string saveId;
    public string saveName;
    public string saveDate;
    public int day;
    public int money;
    public string playTime; // Formatted string
}