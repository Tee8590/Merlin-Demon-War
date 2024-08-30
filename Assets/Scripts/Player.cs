using System.Collections;
using System.Collections.Generic;
/*using UnityEditor.PackageManager;*/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDropHandler
{

    public Image playerImage = null;
    public Image mirrorImage = null;
    public Image healthImage = null;
    public Image glowImage = null;

    public int maxHealth = 5;
    public int health = 5;//current health
    public int mana = 1;

    public bool isPlayer;
    public bool isFire; //Whether an enemy is a fire monster or not.

    public GameObject[] manaBolls = new GameObject[5];

    private Animator animator = null;

    public AudioSource dealAudio = null;
    public AudioSource healAudio = null;
    public AudioSource mirrorAudio = null;
    public AudioSource smashAudio = null;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        HealthCheck();
        UpdateManaBalls();
    }
    internal void PlayHitAnim()
    {
        if (animator != null)
            animator.SetTrigger("Hit");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!GameController.instance.isPlayable)
            return;
        GameObject obj = eventData.pointerDrag;
        if (obj != null)
        {
            Card card = obj.GetComponent<Card>();
            if (card != null)
            {
                GameController.instance.UseCard(card, this, GameController.instance.playerHand);
            }
        }
    }
    internal void HealthCheck()
    {
        if (health >= 0 && health < GameController.instance.healthNumbers.Length)
        {
            healthImage.sprite = GameController.instance.healthNumbers[health];
        }
        else
        {
            Debug.LogWarning("health is notValid" + health.ToString());
        }
    }
    internal void SetMirror(bool on)
    {
        mirrorImage.gameObject.SetActive(on);
    }
    internal bool HasMirror()
    {
        return mirrorImage.gameObject.activeInHierarchy;
    }
    internal void UpdateManaBalls()
    {
        for (int m = 0; m < 5; m++)
        {
            if (m < mana)
            {
                manaBolls[m].SetActive(true);
            }
            else
            {
                manaBolls[m].SetActive(false);
            }
        }
    }
    internal void PlayMirrorSound()
    {
        mirrorAudio.Play();
    }
    internal void PlaySmashSound()
    {
        smashAudio.Play();
    }
    internal void PlayDealSound()
    {
        dealAudio.Play();
    }
    internal void PlayHealSound()
    {
        healAudio.Play();
    }
    
}