using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestItem : MonoBehaviour
{
    public string itemID; // Should match the itemID in QuestObjective
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }
    
    public void CollectItem()
    {
        // Notify QuestManager that this item was collected
        QuestManager.Instance.UpdateObjective(itemID);
        
        // Optionally play sound, particle effect, etc.
        
        // Remove the item from the scene
        gameObject.SetActive(false);
    }
}