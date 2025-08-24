# 🎭 Dialogue System - Complete Implementation Guide

## 📋 Table of Contents
1. [Quick Setup Checklist](#quick-setup-checklist)
2. [Step-by-Step Scene Implementation](#step-by-step-scene-implementation)
3. [CSV Data Preparation](#csv-data-preparation)
4. [UI Setup](#ui-setup)
5. [Character Images Setup](#character-images-setup)
6. [Testing and Debugging](#testing-and-debugging)
7. [Troubleshooting](#troubleshooting)

---

## ✅ Quick Setup Checklist

- [ ] CSV file with dialogue data ready
- [ ] Character images prepared (PNG/JPG)
- [ ] DialogueDatabase ScriptableObject created
- [ ] DialogueManager in scene
- [ ] DialogueUI setup with Canvas
- [ ] Test buttons configured
- [ ] Character images assigned to database

---

## 🔧 Step-by-Step Scene Implementation

### **Step 1: Prepare Your CSV Data**

Create a CSV file with this exact format:
```csv
# of dialog,chinese name,english name of character,content in chinese,content in english
1,狐狸,Fox,动弹不得，我好像被封印在这张桌子前面了。,I'm stuck… It's like I'm sealed in front of this table.
2,玩家,Player,你好！你需要帮助吗？,Hello! Do you need help?
3,狐狸,Fox,是的！我被困在这里很久了。,Yes! I've been trapped here for a long time.
```

**Important Notes:**
- Use tabs or commas as separators
- English name serves as Character ID
- Escape special characters if needed
- Save as UTF-8 encoding for Chinese characters

### **Step 2: Import CSV Data to Unity**

1. **Open Unity Editor**
2. **Go to Menu**: `Tools > Dialogue > Import CSV to ScriptableObject`
3. **Select your CSV file**
4. **Asset Created**: `DialogueDatabase.asset` will be created in `Assets/Scripts/DialogueSystem/`
5. **Verify Import**: Check the asset in Inspector to confirm data imported correctly

### **Step 3: Prepare Character Images**

1. **Create folder**: `Assets/Images/Characters/` (or your preferred location)
2. **Add character images** with names matching English character names:
   - `Fox.png` (for character ID "Fox")
   - `Player.png` (for character ID "Player")
   - etc.
3. **Set Import Settings**:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Single
   - Filter Mode: Bilinear
   - Max Size: 512 or higher

### **Step 4: Create DialogueManager in Scene**

#### **Option A: Automatic Creation**
1. **Create empty GameObject**: `GameObject > Create Empty`
2. **Name it**: "DialogueManager"
3. **Add Component**: `DialogueManager` script
4. **The script will auto-setup as singleton**

#### **Option B: Manual Setup**
1. **Drag DialogueManager script** to any GameObject in scene
2. **Assign DialogueDatabase** in Inspector
3. **Set language preference** (Chinese = true/false)

### **Step 5: Create Dialogue UI**

#### **5.1 Create Canvas**
1. **Right-click in Hierarchy**: `UI > Canvas`
2. **Name it**: "DialogueCanvas"
3. **Set Canvas Scaler**: 
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080
   - Match: 0.5

#### **5.2 Create Dialogue Panel**
1. **Right-click Canvas**: `UI > Panel`
2. **Name it**: "DialoguePanel"
3. **Set Anchors**: Bottom of screen or full screen
4. **Set Background**: Semi-transparent dark color
5. **Add CanvasGroup** component for fade effects

#### **5.3 Add UI Components**

**Character Image:**
1. **Right-click DialoguePanel**: `UI > Image`
2. **Name**: "CharacterImage"
3. **Position**: Left side of panel
4. **Size**: 200x200 pixels (adjust as needed)
5. **Preserve Aspect**: Enabled

**Character Name Text:**
1. **Right-click DialoguePanel**: `UI > Text - TextMeshPro`
2. **Name**: "CharacterNameText"
3. **Position**: Top-left of dialogue area
4. **Font Size**: 24-32
5. **Color**: White or contrasting color

**Dialogue Content Text:**
1. **Right-click DialoguePanel**: `UI > Text - TextMeshPro`
2. **Name**: "DialogueText"
3. **Position**: Main content area
4. **Font Size**: 18-24
5. **Text Wrapping**: Enabled
6. **Rich Text**: Enabled (for formatting)

**Control Buttons:**
1. **Next Button**: 
   - Position: Bottom-right
   - Text: "Next" / "下一个"
2. **Close Button**: 
   - Position: Top-right corner
   - Text: "×"
3. **Language Toggle Button**: 
   - Position: Bottom-left
   - Text: "中/EN"

### **Step 6: Setup DialogueUI Script**

1. **Add DialogueUI script** to DialoguePanel
2. **Assign all UI references** in Inspector:
   - Dialogue Panel: The DialoguePanel GameObject
   - Character Image: Character Image component
   - Character Name Text: Character name TextMeshPro
   - Dialogue Text: Main dialogue TextMeshPro
   - Next Button: Next button component
   - Close Button: Close button component
   - Language Toggle Button: Language toggle component
3. **Configure typewriter settings**:
   - Typewriter Speed: 0.05 (adjust for preference)
   - Use Typewriter Effect: True
4. **Set default character sprite** if desired

### **Step 7: Assign Character Images to Database**

1. **Select DialogueDatabase asset** in Project window
2. **Expand "Character Images" list** in Inspector
3. **Add entries for each character**:
   - Character ID: "Fox"
   - Character Sprite: Fox.png
   - Character ID: "Player" 
   - Character Sprite: Player.png
4. **Match Character IDs** exactly with English names from CSV

### **Step 8: Add Test Buttons (Development Only)**

1. **Create Test Panel**:
   - Right-click Canvas: `UI > Panel`
   - Name: "TestPanel"
   - Position: Top-right corner
   - Size: Small (200x150)

2. **Add DialogueTestButton script** to TestPanel
3. **Create Test Buttons**:
   - "Start Dialogue" button
   - "Toggle Language" button  
   - "Next Dialogue" button
   - "End Dialogue" button

4. **Assign button references** in DialogueTestButton Inspector
5. **Assign test database** reference

### **Step 9: Add Dialogue Triggers (Optional)**

For automatic dialogue triggering:

1. **Add DialogueTrigger script** to NPCs or interaction objects
2. **Configure trigger settings**:
   - Dialogue Number: Starting dialogue number
   - Trigger Type: OnTriggerEnter, KeyPress, OnClick, or Manual
   - Player Tag: "Player"
   - Trigger Key: E (if using KeyPress)
3. **Add Collider** with "Is Trigger" enabled
4. **Create interaction prompt** GameObject if using KeyPress

### **Step 10: Final Testing**

1. **Enter Play Mode**
2. **Click "Start Dialogue" test button**
3. **Verify**:
   - Dialogue panel appears
   - Character image loads correctly
   - Text displays in current language
   - Typewriter effect works
   - Next button advances dialogue
   - Language toggle switches text
   - Dialogue ends properly

---

## 🎨 UI Layout Example

```
┌─────────────────────────────────────────────────────────┐
│                    DialogueCanvas                       │
│  ┌─────────────────────────────────────────────────┐    │
│  │              DialoguePanel                      │[×] │
│  │  ┌─────┐  Character Name                        │    │
│  │  │     │  ┌─────────────────────────────────┐   │    │
│  │  │Char │  │                                 │   │    │
│  │  │ IMG │  │     Dialogue Content Text       │   │    │
│  │  │     │  │                                 │   │    │
│  │  └─────┘  └─────────────────────────────────────┘   │    │
│  │  [中/EN]                                    [Next] │    │
│  └─────────────────────────────────────────────────┘    │
│                                                         │
│  ┌─TestPanel─┐                                          │
│  │[Start]    │                                          │
│  │[Language] │                                          │
│  │[Next]     │                                          │
│  │[End]      │                                          │
│  └───────────┘                                          │
└─────────────────────────────────────────────────────────┘
```

---

## 🎯 Quick Start Commands

### **Inspector Setup Checklist:**

**DialogueManager:**
- ✅ Dialogue Database: Assign your DialogueDatabase asset
- ✅ Use Chinese: True/False

**DialogueUI:**
- ✅ Dialogue Panel: DialoguePanel GameObject
- ✅ Character Image: Image component
- ✅ Character Name Text: TextMeshPro component  
- ✅ Dialogue Text: TextMeshPro component
- ✅ Next Button: Button component
- ✅ Close Button: Button component
- ✅ Language Toggle Button: Button component
- ✅ Typewriter Speed: 0.05
- ✅ Use Typewriter Effect: True

**DialogueTestButton:**
- ✅ Test Database: Your DialogueDatabase asset
- ✅ Test Dialogue Number: 1
- ✅ All button references assigned

---

## 🔧 Runtime Testing

### **Test Sequence:**
1. **Start Play Mode**
2. **Click "Start Dialogue"** → Should show dialogue #1
3. **Click "Next"** → Should advance to dialogue #2
4. **Click "Language Toggle"** → Should switch Chinese/English
5. **Click "Next"** until end → Should close dialogue panel
6. **Check Console** → Should see debug messages

### **Expected Behavior:**
- ✅ Dialogue panel appears smoothly
- ✅ Character image matches character ID
- ✅ Text appears with typewriter effect
- ✅ Language switching works instantly
- ✅ Navigation works correctly
- ✅ Dialogue ends properly

---

## 🚨 Troubleshooting

### **Common Issues:**

**"DialogueManager not found!"**
- Solution: Make sure GameObject with DialogueManager script exists in scene

**"No dialogue database assigned!"**
- Solution: Assign DialogueDatabase asset to DialogueManager in Inspector

**"Character image not found for ID: Fox"**
- Solution: Add character image to DialogueDatabase with matching Character ID

**"CSV file not found"**
- Solution: Check file path and ensure CSV is in correct location

**"Invalid CSV format"**
- Solution: Verify CSV has exactly 5 columns and proper encoding

**Typewriter effect not working**
- Solution: Check DialogueUI has Use Typewriter Effect enabled

**Language toggle not working**
- Solution: Verify button is connected to DialogueTestButton.ToggleLanguage()

### **Debug Features:**

Use these console commands during play:
- Right-click DialogueTestButton → "Test Dialogue System"
- Right-click DialogueTestButton → "Print All Dialogues"  
- Right-click DialogueTestButton → "Print Character Images"

---

## 🎉 You're Done!

Your dialogue system is now fully implemented and ready for production use. Remember to remove or disable test buttons before building your final game!

### **Next Steps:**
- Add more character images
- Create additional CSV files for different scenes
- Implement dialogue branching (modify DialogueEntry for choices)
- Add audio support for voice acting
- Create dialogue editor tools for designers
