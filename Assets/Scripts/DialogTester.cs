using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class DialogTester : MonoBehaviour
{
    public Dialog testDialog;
    
    [ContextMenu("Create Test Dialog")]
    public void CreateTestDialog()
    {
        // Create a new Dialog ScriptableObject
        Dialog newDialog = ScriptableObject.CreateInstance<Dialog>();
        
        // Create a new DialogLine
        DialogLine line = new DialogLine();
        
        // We need to use reflection to set the private field since Text is read-only
        var field = typeof(DialogLine).GetField("text", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(line, "Hello, this is a test dialog.");
        }
        else
        {
            Debug.LogError("Could not find 'text' field in DialogLine");
        }
        
        // Access the lines field and add our line
        var linesField = typeof(Dialog).GetField("lines", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (linesField != null)
        {
            var linesList = linesField.GetValue(newDialog) as List<DialogLine>;
            if (linesList != null)
            {
                linesList.Add(line);
            }
        }
        
        // Assign it
        testDialog = newDialog;
        
        Debug.Log("Created test dialog with 1 line");
    }
    
    [ContextMenu("Create Simple Dialog Asset")]
    public void CreateDialogAsset()
    {
#if UNITY_EDITOR
        // Create a dialog ScriptableObject
        Dialog dialog = ScriptableObject.CreateInstance<Dialog>();
        
        // Create a DialogLine
        DialogLine line = new DialogLine();
        var textField = typeof(DialogLine).GetField("text", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        if (textField != null)
        {
            textField.SetValue(line, "This is a test dialog line.");
        }
        
        // Access the lines field and add our line
        var linesField = typeof(Dialog).GetField("lines", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        
        if (linesField != null)
        {
            var linesList = linesField.GetValue(dialog) as List<DialogLine>;
            if (linesList != null)
            {
                linesList.Add(line);
            }
        }
        
        // Save the asset
        string path = "Assets/TestDialog.asset";
        AssetDatabase.CreateAsset(dialog, path);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Created dialog asset at {path}");
#endif
    }
    
    [ContextMenu("Test Dialog")]
    public void TestCurrentDialog()
    {
        if (testDialog == null)
        {
            Debug.LogError("No test dialog assigned!");
            return;
        }
        
        Debug.Log($"Dialog has {testDialog.Lines.Count} lines");
        foreach (var line in testDialog.Lines)
        {
            Debug.Log($"Line text: {line.Text}");
        }
    }
} 