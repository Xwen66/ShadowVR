using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PromptCSVImporter
{
    /// <summary>
    /// Import prompt data from CSV file
    /// CSV Format: index, content in chinese, content in english, progress (n/3, etc)
    /// </summary>
    public static PromptDatabaseSO ImportFromCSV(string csvPath)
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found at path: {csvPath}");
            return null;
        }
        
        var database = ScriptableObject.CreateInstance<PromptDatabaseSO>();
        database.promptEntries = new List<PromptEntry>();
        
        string[] lines = File.ReadAllLines(csvPath);
        
        // Skip header line (if exists)
        int startLine = lines.Length > 0 && lines[0].Contains("index") ? 1 : 0;
        
        for (int i = startLine; i < lines.Length; i++)
        {
            string[] values = ParseCSVLine(lines[i]);
            
            if (values.Length >= 4)
            {
                var entry = new PromptEntry();
                
                // Parse index
                if (int.TryParse(values[0].Trim(), out int index))
                {
                    entry.index = index;
                }
                else
                {
                    Debug.LogWarning($"Invalid index in line {i + 1}: {values[0]}");
                    continue;
                }
                
                // CSV format: index, content in chinese, content in english, progress
                entry.contentChinese = values[1].Trim().Replace("\\n", "\n");
                entry.contentEnglish = values[2].Trim().Replace("\\n", "\n");
                entry.progress = values[3].Trim();
                
                // Validate entry before adding
                if (entry.IsValid())
                {
                    database.promptEntries.Add(entry);
                }
                else
                {
                    Debug.LogWarning($"Invalid prompt entry in line {i + 1}: both Chinese and English content are empty");
                }
            }
            else
            {
                Debug.LogWarning($"Invalid CSV format in line {i + 1}. Expected 4 columns, got {values.Length}");
            }
        }
        
        Debug.Log($"Imported {database.promptEntries.Count} prompt entries from CSV");
        return database;
    }
    
    /// <summary>
    /// Export prompt data to CSV file
    /// </summary>
    public static void ExportToCSV(PromptDatabaseSO database, string csvPath)
    {
        if (database == null)
        {
            Debug.LogError("Cannot export: database is null");
            return;
        }
        
        List<string> lines = new List<string>();
        
        // Add header
        lines.Add("index,content_chinese,content_english,progress");
        
        // Add entries
        foreach (var entry in database.promptEntries)
        {
            string chineseContent = EscapeCSVField(entry.contentChinese.Replace("\n", "\\n"));
            string englishContent = EscapeCSVField(entry.contentEnglish.Replace("\n", "\\n"));
            string progress = EscapeCSVField(entry.progress);
            
            lines.Add($"{entry.index},{chineseContent},{englishContent},{progress}");
        }
        
        try
        {
            File.WriteAllLines(csvPath, lines);
            Debug.Log($"Exported {database.promptEntries.Count} prompt entries to CSV: {csvPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export CSV: {e.Message}");
        }
    }
    
    /// <summary>
    /// Parse a single CSV line, handling quoted strings with commas
    /// </summary>
    private static string[] ParseCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string currentField = "";
        
        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    // Double quote escape
                    currentField += '"';
                    i++; // Skip next quote
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(currentField);
                currentField = "";
            }
            else
            {
                currentField += c;
            }
        }
        
        result.Add(currentField);
        return result.ToArray();
    }
    
    /// <summary>
    /// Escape CSV field with quotes if necessary
    /// </summary>
    private static string EscapeCSVField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";
        
        // If field contains comma, newline, or quote, wrap in quotes
        if (field.Contains(",") || field.Contains("\n") || field.Contains("\""))
        {
            // Escape internal quotes by doubling them
            string escaped = field.Replace("\"", "\"\"");
            return $"\"{escaped}\"";
        }
        
        return field;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor utility to import CSV and create ScriptableObject asset
    /// </summary>
    [MenuItem("Tools/Prompt/Import CSV to ScriptableObject")]
    public static void ImportCSVToAsset()
    {
        string csvPath = EditorUtility.OpenFilePanel("Select Prompt CSV", Application.dataPath, "csv");
        
        if (!string.IsNullOrEmpty(csvPath))
        {
            var database = ImportFromCSV(csvPath);
            
            if (database != null)
            {
                string assetPath = "Assets/Scripts/PromptSystem/PromptDatabase.asset";
                AssetDatabase.CreateAsset(database, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = database;
                
                Debug.Log($"Prompt database created at: {assetPath}");
            }
        }
    }
    
    /// <summary>
    /// Update existing prompt database from CSV
    /// </summary>
    [MenuItem("Tools/Prompt/Update Existing Database from CSV")]
    public static void UpdateDatabaseFromCSV()
    {
        var database = Selection.activeObject as PromptDatabaseSO;
        
        if (database == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a PromptDatabaseSO asset first.", "OK");
            return;
        }
        
        string csvPath = EditorUtility.OpenFilePanel("Select Prompt CSV", Application.dataPath, "csv");
        
        if (!string.IsNullOrEmpty(csvPath))
        {
            var newDatabase = ImportFromCSV(csvPath);
            
            if (newDatabase != null)
            {
                database.promptEntries = newDatabase.promptEntries;
                
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                
                Debug.Log("Prompt database updated successfully!");
            }
        }
    }
    
    /// <summary>
    /// Export existing prompt database to CSV
    /// </summary>
    [MenuItem("Tools/Prompt/Export Database to CSV")]
    public static void ExportDatabaseToCSV()
    {
        var database = Selection.activeObject as PromptDatabaseSO;
        
        if (database == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a PromptDatabaseSO asset first.", "OK");
            return;
        }
        
        string csvPath = EditorUtility.SaveFilePanel("Export Prompt CSV", Application.dataPath, "PromptDatabase", "csv");
        
        if (!string.IsNullOrEmpty(csvPath))
        {
            ExportToCSV(database, csvPath);
            EditorUtility.DisplayDialog("Success", $"Prompt database exported to: {csvPath}", "OK");
        }
    }
    
    /// <summary>
    /// Create a sample CSV file with example prompt data
    /// </summary>
    [MenuItem("Tools/Prompt/Create Sample CSV")]
    public static void CreateSampleCSV()
    {
        string csvPath = EditorUtility.SaveFilePanel("Create Sample Prompt CSV", Application.dataPath, "SamplePrompts", "csv");
        
        if (!string.IsNullOrEmpty(csvPath))
        {
            List<string> lines = new List<string>
            {
                "index,content_chinese,content_english,progress",
                "0,\"按下X键与物体互动\",\"Press X button to interact with objects\",\"\"",
                "1,\"寻找发光的记忆碎片\",\"Look for glowing memory shards\",\"1/5\"",
                "2,\"变大以够到高处的物体\",\"Grow larger to reach objects at height\",\"\"",
                "3,\"变小以进入狭小的空间\",\"Shrink to enter tight spaces\",\"\"",
                "4,\"收集所有记忆碎片以获胜\",\"Collect all memory shards to win\",\"3/5\"",
                "5,\"利用光影解决谜题\",\"Use light and shadow to solve puzzles\",\"2/3\""
            };
            
            try
            {
                File.WriteAllLines(csvPath, lines);
                Debug.Log($"Sample prompt CSV created at: {csvPath}");
                EditorUtility.DisplayDialog("Success", $"Sample CSV created at: {csvPath}", "OK");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create sample CSV: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to create sample CSV: {e.Message}", "OK");
            }
        }
    }
    
    /// <summary>
    /// Validate prompt database entries
    /// </summary>
    [MenuItem("Tools/Prompt/Validate Database")]
    public static void ValidateDatabase()
    {
        var database = Selection.activeObject as PromptDatabaseSO;
        
        if (database == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a PromptDatabaseSO asset first.", "OK");
            return;
        }
        
        Debug.Log("=== PROMPT DATABASE VALIDATION ===");
        Debug.Log($"Total entries: {database.promptEntries.Count}");
        Debug.Log($"Valid entries: {database.GetValidPromptCount()}");
        
        // Check for duplicate indices
        var indices = database.GetAllPromptIndices();
        var duplicates = new List<int>();
        for (int i = 0; i < indices.Count - 1; i++)
        {
            if (indices[i] == indices[i + 1])
            {
                duplicates.Add(indices[i]);
            }
        }
        
        if (duplicates.Count > 0)
        {
            Debug.LogError($"Duplicate indices found: {string.Join(", ", duplicates)}");
        }
        
        // Check for invalid entries
        int invalidCount = 0;
        foreach (var entry in database.promptEntries)
        {
            if (!entry.IsValid())
            {
                Debug.LogWarning($"Invalid entry at index {entry.index}: both Chinese and English content are empty");
                invalidCount++;
            }
        }
        
        Debug.Log($"Invalid entries: {invalidCount}");
        Debug.Log("==================================");
        
        string message = $"Validation complete!\nTotal: {database.promptEntries.Count}\nValid: {database.GetValidPromptCount()}\nInvalid: {invalidCount}";
        if (duplicates.Count > 0)
        {
            message += $"\nDuplicates: {duplicates.Count}";
        }
        
        EditorUtility.DisplayDialog("Validation Results", message, "OK");
    }
#endif
}
