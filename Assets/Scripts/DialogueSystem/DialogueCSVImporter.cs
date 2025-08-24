using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DialogueCSVImporter
{
    /// <summary>
    /// Import dialogue data from CSV file
    /// CSV Format: # of dialog, chinese name, english name of character, content in chinese, content in english
    /// Note: English name serves as character ID
    /// </summary>
    public static DialogueDatabase ImportFromCSV(string csvPath)
    {
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found at path: {csvPath}");
            return null;
        }
        
        var database = ScriptableObject.CreateInstance<DialogueDatabase>();
        database.dialogueEntries = new List<DialogueEntry>();
        
        string[] lines = File.ReadAllLines(csvPath);
        
        // Skip header line (if exists)
        int startLine = lines.Length > 0 && lines[0].Contains("dialog") ? 1 : 0;
        
        for (int i = startLine; i < lines.Length; i++)
        {
            string[] values = ParseCSVLine(lines[i]);
            
            if (values.Length >= 5)
            {
                var entry = new DialogueEntry();
                
                // Parse dialog number
                if (int.TryParse(values[0].Trim(), out int dialogNumber))
                {
                    entry.dialogNumber = dialogNumber;
                }
                else
                {
                    Debug.LogWarning($"Invalid dialog number in line {i + 1}: {values[0]}");
                    continue;
                }
                
                // New CSV format: # of dialog, chinese name, english name, content in chinese, content in english
                entry.characterNameChinese = values[1].Trim();
                entry.characterNameEnglish = values[2].Trim(); // This serves as character ID
                entry.contentChinese = values[3].Trim().Replace("\\n", "\n");
                entry.contentEnglish = values[4].Trim().Replace("\\n", "\n");
                
                database.dialogueEntries.Add(entry);
            }
            else
            {
                Debug.LogWarning($"Invalid CSV format in line {i + 1}. Expected 5 columns, got {values.Length}");
            }
        }
        
        Debug.Log($"Imported {database.dialogueEntries.Count} dialogue entries from CSV");
        return database;
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
    
#if UNITY_EDITOR
    /// <summary>
    /// Editor utility to import CSV and create ScriptableObject asset
    /// </summary>
    [MenuItem("Tools/Dialogue/Import CSV to ScriptableObject")]
    public static void ImportCSVToAsset()
    {
        string csvPath = EditorUtility.OpenFilePanel("Select Dialogue CSV", Application.dataPath, "csv");
        
        if (!string.IsNullOrEmpty(csvPath))
        {
            var database = ImportFromCSV(csvPath);
            
            if (database != null)
            {
                string assetPath = "Assets/Scripts/DialogueSystem/DialogueDatabase.asset";
                AssetDatabase.CreateAsset(database, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = database;
                
                Debug.Log($"Dialogue database created at: {assetPath}");
            }
        }
    }
    
    /// <summary>
    /// Update existing dialogue database from CSV
    /// </summary>
    [MenuItem("Tools/Dialogue/Update Existing Database from CSV")]
    public static void UpdateDatabaseFromCSV()
    {
        var database = Selection.activeObject as DialogueDatabase;
        
        if (database == null)
        {
            EditorUtility.DisplayDialog("Error", "Please select a DialogueDatabase asset first.", "OK");
            return;
        }
        
        string csvPath = EditorUtility.OpenFilePanel("Select Dialogue CSV", Application.dataPath, "csv");
        
        if (!string.IsNullOrEmpty(csvPath))
        {
            var newDatabase = ImportFromCSV(csvPath);
            
            if (newDatabase != null)
            {
                database.dialogueEntries = newDatabase.dialogueEntries;
                
                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssets();
                
                Debug.Log("Dialogue database updated successfully!");
            }
        }
    }
#endif
}
