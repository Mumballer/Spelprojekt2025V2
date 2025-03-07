using UnityEngine;
using System;
using TMPro;

public class Nametag : MonoBehaviour
{
    [Header("Nametag Settings")]
    [SerializeField] private string nametagID;
    [SerializeField] private bool interactable = true;
    [SerializeField] private float pickupDistance = 2.0f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E; // nyckel f�r att plocka upp namnskylten

    [Header("Visual Effects")]
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private GameObject visualsContainer;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private TextMeshPro nameText;
    [SerializeField] private float fadeSpeed = 5f;
    [SerializeField] private GameObject pickupPrompt; // valfri "Press E" prompt

    [Header("Sound Effects")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip placeSound;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    // events f�r interaktioner
    public event Action<TableSpot, string> OnNametagPlaced;
    public event Action OnNametagPickedUp;

    // interna variabler
    private bool pickedUp = false;
    private bool placed = false;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private TableSpot currentSpot;
    private float currentAlpha = 1.0f;
    private float targetAlpha = 1.0f;
    private Transform playerTransform;
    private bool playerInRange = false;
    private AudioSource audioSource;
    private GameObject playerObject; // referens till hela spelarobjektet

    void Awake()
    {
        // leta reda p� och spara alla renderare om ingen �r satt
        if (renderers == null || renderers.Length == 0)
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        // spara originalposition f�r att kunna �terst�lla senare
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        // s�tt upp ljudk�lla
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (pickupSound != null || placeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D ljud
            audioSource.volume = 0.8f;
        }
    }

    void Start()
    {
        // hitta spelaren, kan vara bra att ha
        playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        // d�lj highlight och prompts vid start
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }

        // s�tt namn om textkomponent finns
        if (nameText != null && !string.IsNullOrEmpty(nametagID))
        {
            nameText.text = nametagID;
        }
        else if (string.IsNullOrEmpty(nametagID))
        {
            // anv�nd gameobject-namn om inget ID �r satt
            nametagID = gameObject.name;
        }
    }

    void Update()
    {
        // kolla om spelaren �r n�ra (f�r highlight etc)
        CheckPlayerDistance();

        // hantera pickup-input n�r spelaren �r inom r�ckvidd
        if (playerInRange && interactable && !pickedUp && !placed && Input.GetKeyDown(pickupKey))
        {
            HandlePickup();
        }

        // hantera alpha-fade
        UpdateVisibility();
    }

    // kollar avst�ndet till spelaren
    private void CheckPlayerDistance()
    {
        if (playerTransform == null || !interactable) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool inRange = distance <= pickupDistance;

        // uppdatera bara om statusen �ndrats
        if (inRange != playerInRange)
        {
            playerInRange = inRange;

            // visa highlight om spelaren �r i n�rheten och namnskylten kan plockas upp
            if (highlightObject != null)
            {
                highlightObject.SetActive(playerInRange && !pickedUp && !placed);
            }

            // visa E-prompt om spelaren �r i n�rheten
            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(playerInRange && !pickedUp && !placed);
            }

            // anropa event-metoder f�r att andra skript ska kunna reagera
            if (playerInRange)
            {
                OnPlayerEnterRange();
            }
            else
            {
                OnPlayerExitRange();
            }
        }
    }

    // hanterar upplockande av namnskylten
    private void HandlePickup()
    {
        if (showDebug)
        {
            Debug.Log($"Player pressed {pickupKey} to pick up {nametagID}");
        }

        // plocka upp namnskylten
        PickUp();

        // parenta till spelaren (s� den f�ljer med)
        Transform playerHand = FindPlayerHand();
        if (playerHand != null)
        {
            transform.SetParent(playerHand);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            // placera i spelarens n�rhet om ingen hand hittas
            transform.position = playerTransform.position + playerTransform.forward * 0.5f + Vector3.up * 0.5f;
        }
    }

    // hitta spelarens hand eller annan plats att s�tta namnskylten
    private Transform FindPlayerHand()
    {
        if (playerObject == null) return null;

        // leta efter specifikt hand-objekt
        Transform hand = playerObject.transform.Find("Hand");
        if (hand != null) return hand;

        // leta efter namngivet hand-objekt
        Transform[] allChildren = playerObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in allChildren)
        {
            if (child.name.Contains("Hand") || child.name.Contains("hand") ||
                child.name.Contains("Hold") || child.name.Contains("NametagHolder"))
            {
                return child;
            }
        }

        // om inget hittas, anv�nd spelaren sj�lv
        return playerTransform;
    }

    // n�r spelaren kommer inom r�ckh�ll
    private void OnPlayerEnterRange()
    {
        try
        {
            if (showDebug)
            {
                Debug.Log($"Player entered range of nametag: {gameObject.name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnPlayerEnterRange: {e.Message}");
        }
    }

    // n�r spelaren l�mnar omr�det
    private void OnPlayerExitRange()
    {
        try
        {
            if (showDebug)
            {
                Debug.Log($"Player exited range of nametag: {gameObject.name}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in OnPlayerExitRange: {e.Message}");
        }
    }

    // s�tter genomskinlighet p� namnbrickan (anropas fr�n TableSpot)
    public void SetAlpha(float alpha)
    {
        targetAlpha = Mathf.Clamp01(alpha);

        // direkt uppdatering p� alla material
        if (renderers != null && renderers.Length > 0)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        // kolla om materialet har en f�rgegenskap
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = targetAlpha;
                            mat.color = color;
                        }
                    }
                }
            }
        }

        currentAlpha = targetAlpha;

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' alpha set to {targetAlpha}");
        }
    }

    // gradvis uppdatering av genomskinlighet
    private void UpdateVisibility()
    {
        if (Mathf.Approximately(currentAlpha, targetAlpha)) return;

        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        if (renderers != null)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer != null)
                {
                    foreach (Material mat in renderer.materials)
                    {
                        if (mat.HasProperty("_Color"))
                        {
                            Color color = mat.color;
                            color.a = currentAlpha;
                            mat.color = color;
                        }
                    }
                }
            }
        }
    }

    // markerar att namnbrickan �r placerad (kallas fr�n TableSpot)
    public void PlaceNametag()
    {
        placed = true;
        pickedUp = false;

        // s�tt tillbaka parent
        transform.SetParent(null);

        // visa namnbrickan
        SetAlpha(1.0f);

        // spela ljud om det finns
        PlaySound(placeSound);

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' placed at spot: {(currentSpot != null ? currentSpot.name : "unknown")}");
        }

        // notifiera att namnbrickan har placerats
        if (currentSpot != null)
        {
            OnNametagPlaced?.Invoke(currentSpot, nametagID);
        }
    }

    // fysiskt placerar namnbrickan p� en plats (kallas fr�n TableSpot)
    public void PlaceAtSpot(TableSpot spot)
    {
        if (spot == null)
        {
            Debug.LogError("Cannot place nametag - TableSpot is null");
            return;
        }

        // spara referensen till platsen
        currentSpot = spot;

        // s�tt tillbaka parent
        transform.SetParent(null);

        // flytta till platsens position/rotation
        transform.position = spot.transform.position;
        transform.rotation = spot.transform.rotation;

        // leta efter en specifik platsmark�r om s�dan finns
        Transform nametagPosition = spot.transform.Find("NametagPosition");
        if (nametagPosition != null)
        {
            transform.position = nametagPosition.position;
            transform.rotation = nametagPosition.rotation;
        }

        // uppdatera status
        placed = true;
        pickedUp = false;

        // visa namnbrickan
        SetAlpha(1.0f);

        // spela ljud om det finns
        PlaySound(placeSound);

        if (showDebug)
        {
            Debug.Log($"Positioned nametag '{nametagID}' at spot: {spot.name}");
        }
    }

    // markerar att namnbrickan �r upplockad
    public void PickUp()
    {
        pickedUp = true;
        placed = false;
        currentSpot = null;

        // spela ljud om det finns
        PlaySound(pickupSound);

        // notifiera att namnbrickan har plockats upp
        OnNametagPickedUp?.Invoke();

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' picked up");
        }
    }

    // l�gger ner namnbrickan (inte p� en specifik plats)
    public void PutDown()
    {
        pickedUp = false;
        placed = false;
        currentSpot = null;

        // s�tt tillbaka parent
        transform.SetParent(null);

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' put down");
        }
    }

    // kontrollera om namnbrickan �r upplockad - kr�vs av TableSpot
    public bool IsPickedUp()
    {
        return pickedUp;
    }

    // �terst�ll namnbrickan till sin originalposition
    public void ResetPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // s�tt tillbaka parent
        transform.SetParent(null);

        pickedUp = false;
        placed = false;
        currentSpot = null;

        if (showDebug)
        {
            Debug.Log($"Nametag '{nametagID}' reset to original position");
        }
    }

    // hj�lpmetod f�r att spela ljud
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // f�r visualisering i editorn
    private void OnDrawGizmosSelected()
    {
        // visa interaktionsradie
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupDistance);

        // visa status
        if (placed)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(0.1f, 0.1f, 0.1f));
        }
        else if (pickedUp)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.2f, new Vector3(0.1f, 0.1f, 0.1f));
        }
    }
}