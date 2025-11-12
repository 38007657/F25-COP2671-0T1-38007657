using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages saving and loading game state with multiple save slots
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }

    [Header("Save Settings")]
    [SerializeField] private string saveFolderName = "Saves";
    [SerializeField] private bool useEncryption = false;

    [Header("ScriptableObject References")]
    [SerializeField] private List<InventoryItem> allInventoryItems = new List<InventoryItem>();
    [SerializeField] private List<SeedPacket> allSeedPackets = new List<SeedPacket>();

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Playtime tracking
    private float sessionStartTime;
    private float totalPlayTime = 0f;
    private string currentSaveId = null;

    private string SaveFolderPath => Path.Combine(Application.persistentDataPath, saveFolderName);

    public float TotalPlayTime => totalPlayTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create save folder if it doesn't exist
        if (!Directory.Exists(SaveFolderPath))
        {
            Directory.CreateDirectory(SaveFolderPath);
        }

        sessionStartTime = Time.time;
    }

    private void Update()
    {
        // Track playtime
        if (currentSaveId != null)
        {
            totalPlayTime += Time.deltaTime;
        }
    }

    // ===== SAVE GAME =====

    /// <summary>
    /// Save the current game state to a new or existing save slot
    /// </summary>
    public bool SaveGame(string saveName, string saveId = null)
    {
        try
        {
            SaveData data;

            if (saveId != null && SaveExists(saveId))
            {
                // Load existing save to preserve metadata
                data = LoadSaveData(saveId);
                data.saveName = saveName;
                data.saveDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            }
            else
            {
                // Create new save
                data = new SaveData();
                data.saveName = saveName;
                saveId = data.saveId;
            }

            // Update playtime
            data.totalPlayTime = totalPlayTime;

            // Save all game state
            SavePlayerData(data);
            SaveTimeData(data);
            SaveInventoryData(data);
            SaveCropData(data);

            // Write to file
            string filePath = GetSaveFilePath(saveId);
            string json = JsonUtility.ToJson(data, true);

            if (useEncryption)
            {
                json = EncryptDecrypt(json);
            }

            File.WriteAllText(filePath, json);

            currentSaveId = saveId;

            if (showDebugLogs)
            {
                Debug.Log($"[SaveLoadManager] Game saved: {saveName} (ID: {saveId})");
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Failed to save game: {e.Message}");
            return false;
        }
    }

    private void SavePlayerData(SaveData data)
    {
        if (PlayerInventory.Instance != null)
        {
            data.playerMoney = PlayerInventory.Instance.CurrentMoney;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Vector3 pos = player.transform.position;
            data.playerPositionX = pos.x;
            data.playerPositionY = pos.y;
            data.playerPositionZ = pos.z;
        }
    }

    private void SaveTimeData(SaveData data)
    {
        if (CropManager.Instance != null)
        {
            data.currentDay = CropManager.Instance.CurrentDay;
        }

        if (TimeManager.Instance != null)
        {
            data.currentTime = TimeManager.Instance.CurrentTime;
        }
    }

    private void SaveInventoryData(SaveData data)
    {
        if (PlayerInventory.Instance == null) return;

        data.harvestedItems.Clear();
        data.seedPackets.Clear();

        // Save harvested items
        foreach (var kvp in PlayerInventory.Instance.HarvestedItems)
        {
            data.harvestedItems.Add(new SavedInventoryItem
            {
                itemName = kvp.Key.itemName,
                quantity = kvp.Value
            });
        }

        // Save seed packets
        foreach (var kvp in PlayerInventory.Instance.SeedPackets)
        {
            data.seedPackets.Add(new SavedSeedPacket
            {
                seedName = kvp.Key.cropName,
                quantity = kvp.Value
            });
        }

        if (showDebugLogs)
        {
            Debug.Log($"[SaveLoadManager] Saved {data.harvestedItems.Count} items, {data.seedPackets.Count} seed types");
        }
    }

    private void SaveCropData(SaveData data)
    {
        if (CropManager.Instance == null) return;

        data.plantedCrops.Clear();

        // Get all planted crops - we'll need to add a public method to CropManager
        List<CropBlock> plantedCrops = CropManager.Instance.GetAllPlantedCrops();

        foreach (CropBlock block in plantedCrops)
        {
            if (block != null && block.isPlanted && block.seedPacket != null)
            {
                SavedCropBlock savedBlock = new SavedCropBlock
                {
                    gridPositionX = block.gridPosition.x,
                    gridPositionY = block.gridPosition.y,
                    isTilled = block.isTilled,
                    isWatered = block.isWatered,
                    isPlanted = block.isPlanted,
                    isWilted = block.isWilted,
                    seedPacketName = block.seedPacket.cropName,
                    currentGrowthStage = block.currentGrowthStage,
                    growthTimer = block.growthTimer,
                    dayPlanted = block.dayPlanted,
                    lastWateredDay = block.lastWateredDay,
                    daysWithoutWater = block.daysWithoutWater
                };

                data.plantedCrops.Add(savedBlock);
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[SaveLoadManager] Saved {data.plantedCrops.Count} planted crops");
        }
    }

    // ===== LOAD GAME =====

    /// <summary>
    /// Load a specific save by ID
    /// </summary>
    public bool LoadGame(string saveId)
    {
        if (!SaveExists(saveId))
        {
            Debug.LogError($"[SaveLoadManager] Save not found: {saveId}");
            return false;
        }

        try
        {
            SaveData data = LoadSaveData(saveId);

            // Load all game state
            LoadPlayerData(data);
            LoadTimeData(data);
            LoadInventoryData(data);
            LoadCropData(data);

            // Set current save and playtime
            currentSaveId = saveId;
            totalPlayTime = data.totalPlayTime;
            sessionStartTime = Time.time;

            if (showDebugLogs)
            {
                Debug.Log($"[SaveLoadManager] Game loaded: {data.saveName} (Day {data.currentDay})");
            }

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Failed to load game: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Load the most recent save automatically
    /// </summary>
    public bool LoadLatestSave()
    {
        List<SaveSlotInfo> saves = GetAllSaves();

        if (saves.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.Log("[SaveLoadManager] No saves found to auto-load");
            }
            return false;
        }

        // Sort by date (most recent first)
        saves = saves.OrderByDescending(s => s.saveDate).ToList();

        return LoadGame(saves[0].saveId);
    }

    private SaveData LoadSaveData(string saveId)
    {
        string filePath = GetSaveFilePath(saveId);
        string json = File.ReadAllText(filePath);

        if (useEncryption)
        {
            json = EncryptDecrypt(json);
        }

        return JsonUtility.FromJson<SaveData>(json);
    }

    private void LoadPlayerData(SaveData data)
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.SetMoney(data.playerMoney);
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = new Vector3(
                data.playerPositionX,
                data.playerPositionY,
                data.playerPositionZ
            );
        }
    }

    private void LoadTimeData(SaveData data)
    {
        if (CropManager.Instance != null)
        {
            CropManager.Instance.SetCurrentDay(data.currentDay);
        }

        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.SetTime(data.currentTime);
        }
    }

    private void LoadInventoryData(SaveData data)
    {
        if (PlayerInventory.Instance == null) return;

        // Clear existing inventory
        PlayerInventory.Instance.ClearInventory();

        // Load harvested items
        foreach (SavedInventoryItem savedItem in data.harvestedItems)
        {
            InventoryItem item = FindInventoryItemByName(savedItem.itemName);
            if (item != null)
            {
                PlayerInventory.Instance.AddHarvestedItem(item, savedItem.quantity);
            }
        }

        // Load seed packets
        foreach (SavedSeedPacket savedPacket in data.seedPackets)
        {
            SeedPacket packet = FindSeedPacketByName(savedPacket.seedName);
            if (packet != null)
            {
                PlayerInventory.Instance.AddSeedPacket(packet, savedPacket.quantity);
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[SaveLoadManager] Loaded {data.harvestedItems.Count} items, {data.seedPackets.Count} seed types");
        }
    }

    private void LoadCropData(SaveData data)
    {
        if (CropManager.Instance == null) return;

        // Clear all existing crops
        CropManager.Instance.ClearAllCrops();

        // Restore saved crops
        foreach (SavedCropBlock savedBlock in data.plantedCrops)
        {
            Vector2Int gridPos = new Vector2Int(savedBlock.gridPositionX, savedBlock.gridPositionY);
            CropBlock block = CropManager.Instance.GetBlockAtPosition(gridPos);

            if (block != null)
            {
                SeedPacket packet = FindSeedPacketByName(savedBlock.seedPacketName);

                if (packet != null)
                {
                    // Restore block state
                    block.RestoreState(
                        savedBlock,
                        packet
                    );
                }
            }
        }

        if (showDebugLogs)
        {
            Debug.Log($"[SaveLoadManager] Loaded {data.plantedCrops.Count} planted crops");
        }
    }

    // ===== SAVE MANAGEMENT =====

    /// <summary>
    /// Get all available saves
    /// </summary>
    public List<SaveSlotInfo> GetAllSaves()
    {
        List<SaveSlotInfo> saves = new List<SaveSlotInfo>();

        if (!Directory.Exists(SaveFolderPath))
        {
            return saves;
        }

        string[] files = Directory.GetFiles(SaveFolderPath, "*.json");

        foreach (string file in files)
        {
            try
            {
                string json = File.ReadAllText(file);

                if (useEncryption)
                {
                    json = EncryptDecrypt(json);
                }

                SaveData data = JsonUtility.FromJson<SaveData>(json);

                SaveSlotInfo info = new SaveSlotInfo
                {
                    saveId = data.saveId,
                    saveName = data.saveName,
                    saveDate = data.saveDate,
                    day = data.currentDay,
                    money = data.playerMoney,
                    playTime = FormatPlayTime(data.totalPlayTime)
                };

                saves.Add(info);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SaveLoadManager] Failed to read save file {file}: {e.Message}");
            }
        }

        return saves;
    }

    /// <summary>
    /// Delete a save by ID
    /// </summary>
    public bool DeleteSave(string saveId)
    {
        try
        {
            string filePath = GetSaveFilePath(saveId);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);

                if (currentSaveId == saveId)
                {
                    currentSaveId = null;
                }

                if (showDebugLogs)
                {
                    Debug.Log($"[SaveLoadManager] Deleted save: {saveId}");
                }

                return true;
            }

            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveLoadManager] Failed to delete save: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if a save exists
    /// </summary>
    public bool SaveExists(string saveId)
    {
        return File.Exists(GetSaveFilePath(saveId));
    }

    /// <summary>
    /// Check if any saves exist
    /// </summary>
    public bool AnySavesExist()
    {
        return GetAllSaves().Count > 0;
    }

    // ===== HELPER METHODS =====

    private string GetSaveFilePath(string saveId)
    {
        return Path.Combine(SaveFolderPath, $"save_{saveId}.json");
    }

    private InventoryItem FindInventoryItemByName(string itemName)
    {
        foreach (InventoryItem item in allInventoryItems)
        {
            if (item != null && item.itemName == itemName)
            {
                return item;
            }
        }

        Debug.LogWarning($"[SaveLoadManager] Could not find InventoryItem: {itemName}");
        return null;
    }

    private SeedPacket FindSeedPacketByName(string seedName)
    {
        foreach (SeedPacket packet in allSeedPackets)
        {
            if (packet != null && packet.cropName == seedName)
            {
                return packet;
            }
        }

        Debug.LogWarning($"[SaveLoadManager] Could not find SeedPacket: {seedName}");
        return null;
    }

    private string FormatPlayTime(float seconds)
    {
        int hours = Mathf.FloorToInt(seconds / 3600f);
        int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
        return $"{hours}h {minutes}m";
    }

    private string EncryptDecrypt(string data)
    {
        string key = "FarmingGame2024";
        string output = "";

        for (int i = 0; i < data.Length; i++)
        {
            output += (char)(data[i] ^ key[i % key.Length]);
        }

        return output;
    }

    // ===== PUBLIC GETTERS =====

    public string GetCurrentSaveId() => currentSaveId;

    public string GetFormattedPlayTime()
    {
        return FormatPlayTime(totalPlayTime);
    }
}