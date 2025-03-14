using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestItem : MonoBehaviour
{
    public string itemID; // matchar med quest objective
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectItem();
        }
    }
    
    public void CollectItem()
    {
        // säger till questmanager
        QuestManager.Instance.UpdateObjective(itemID);
        

        

        gameObject.SetActive(false);
    }
}