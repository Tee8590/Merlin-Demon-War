using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.CompilerServices;

public class GameController : MonoBehaviour
{
    static public GameController instance = null;

    public Deck playerDeck = new Deck();
    public Deck enemyDeck = new Deck();

    public Hand playerHand = new Hand();
    public Hand enemyHand = new Hand();

    public Player player = null;
    public Player enemy = null;

    public List<CardData> cards = new List<CardData>();

    public Sprite[] healthNumbers = new Sprite[10];
    public Sprite[] damageNumbers = new Sprite[10];

    public GameObject cardPreFab = null;
    public Canvas canvas = null;
    public bool isPlayable = false;

    public GameObject effectFromLeftPrefab = null;
    public GameObject effectFromRightPrefab = null;

    public Sprite fireBallImage = null;
    public Sprite iceBallImage = null;
    public Sprite multiFireBallImage = null;
    public Sprite multiIceBallImage = null;
    public Sprite fireAndIceBallImage = null;

    public bool playersTurn = true;
    public Text turnText = null;
    public Image enemySkipTurn = null;

    public Sprite fireDemon = null;     
    public Sprite iceDemon = null;

    public int playerScore = 0; 
    public int playerKills = 0;
    public Text scoreText = null;

    public AudioSource playerDieAudio = null;
    public AudioSource enemyDieAudio = null;

    private void Awake()
    {
        instance = this;
        SetupEnemy();
        playerDeck.Create();
        enemyDeck.Create();

        StartCoroutine(DealHands());
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }


    public void SkipTurn()
    {
        if (playersTurn && isPlayable)
        {
            NextPlayersTurn();
        }
    }
    internal IEnumerator DealHands()
    {
        yield return new WaitForSeconds(1);

        for (int t = 0; t < 3; t++)
        {
            playerDeck.DealCards(playerHand);
            enemyDeck.DealCards(enemyHand);

            yield return new WaitForSeconds(1);
        }
        isPlayable = true;
    }
    internal bool UseCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        //CardValid
        if (!CardValid(card, usingOnPlayer, fromHand))
            return false;
        //CastCard
        isPlayable = false;

        CastCard(card, usingOnPlayer, fromHand);

        player.glowImage.gameObject.SetActive(false);
        enemy.glowImage.gameObject.SetActive(false);

        //RemoveCard
        fromHand.RemoveCard(card);
        //Deal Replacement Card
        return false;
    }
    internal bool CardValid(Card cardBeingPlayed, Player usingOnPlayer, Hand fromHand)
    {
        bool valid = false;
        if (cardBeingPlayed == null)
            return false;

        if (fromHand.isPlayers)
        {
            if (cardBeingPlayed.cardData.cost <= player.mana)
            {
                if (usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenceCard)
                    valid = true;
                if (!usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenceCard)
                    valid = true;
            }
        }
        else// from enemy
        {
            if (cardBeingPlayed.cardData.cost <= enemy.mana)
            {
                if (!usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenceCard)
                    valid = true;
                if (usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenceCard)
                    valid = true;
            }
        }
        return valid;
    }
    internal void CastCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
       
        if (card.cardData.isMirrorCard)
        {
            usingOnPlayer.SetMirror(true);
            usingOnPlayer.PlayMirrorSound();
            NextPlayersTurn();
            isPlayable = true;
        }
        else
        {
            if (card.cardData.isDefenceCard)//Health cards
            {
                usingOnPlayer.health += card.cardData.damage;
                usingOnPlayer.PlayHealSound();

                if(usingOnPlayer.health>usingOnPlayer.maxHealth)
                    usingOnPlayer.health = usingOnPlayer.maxHealth;  
                
                UpdateHealth();

                StartCoroutine(CastHealEffect(usingOnPlayer));
            }
            else//Attack Card
            {
                CastAttackEffect(card, usingOnPlayer);
            }
            //Todo Update score
            if (fromHand.isPlayers)
                playerScore += card.cardData.damage;
         UpdateScore();

        }
        if (fromHand.isPlayers)
        {
            GameController.instance.player.mana -= card.cardData.cost;
            GameController.instance.player.UpdateManaBalls();
        }
        else
        {
            GameController.instance.enemy.mana -= card.cardData.cost;
            GameController.instance.enemy.UpdateManaBalls();
        }
     
    }

   
    private IEnumerator CastHealEffect(Player unsingOnPlayer)
    {
        yield return new WaitForSeconds(0.5f);
        NextPlayersTurn();
        isPlayable = true;
    }
    internal void CastAttackEffect(Card card, Player usingOnPlayer)
     {
        GameObject effectGO = null;
        if (usingOnPlayer.isPlayer)
            effectGO = Instantiate(effectFromRightPrefab, canvas.gameObject.transform);
        else
            effectGO = Instantiate(effectFromLeftPrefab, canvas.gameObject.transform);

        Effect effect = effectGO.GetComponent<Effect>();
        if (effect)
        {
            effect.targetPlayer = usingOnPlayer;
            effect.sourceCard = card;

            switch (card.cardData.damageType)
            {
                case CardData.DamageType.fire:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiFireBallImage;
                    else
                        effect.effectImage.sprite = fireBallImage;
                    effect.PlayFireSound();
                break;
                case CardData.DamageType.ice:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiIceBallImage;
                    else
                        effect.effectImage.sprite = iceBallImage;
                    effect.PlayIceSound();
                break;
                case CardData.DamageType.both:
                    effect.effectImage.sprite = fireAndIceBallImage;
                    effect.PlayFireSound();
                    effect.PlayIceSound();
                    break;
            }
        }
    }
    internal void UpdateHealth()
    {
        player.HealthCheck();
        enemy.HealthCheck();

        if(player.health <= 0)
        {
          StartCoroutine(GameOver());

        }
        if(enemy.health <= 0)
        {
            playerKills++;
            playerScore += 100;
            UpdateScore();
           StartCoroutine(NewEnemy());
        }

    }

    private IEnumerator NewEnemy()
    {
        enemy.gameObject.SetActive(false);
        enemyHand.ClearHand();
        yield return new WaitForSeconds(1);
        SetupEnemy();
        enemy.gameObject.SetActive(true);
        StartCoroutine(DealHands());

    }
    private void SetupEnemy()
    {
        enemy.health = 5;
        enemy.mana = 0;
        enemy.HealthCheck();
        enemy.isFire = true;
        if (UnityEngine.Random.Range(0, 2) == 1)
            enemy.isFire = false;
        if (enemy.isFire)
            enemy.playerImage.sprite = fireDemon;
        else
            enemy.playerImage.sprite = iceDemon;
    }
    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(2);
    }

    internal void NextPlayersTurn()
    {
        playersTurn = !playersTurn;
        bool enemyIsDead = false;
        if (playersTurn)
        {
            if (player.mana < 5)
            {
                player.mana++;
            }
        }
        else //Enemy
        {
            if (enemy.health > 0)
            {

                if (enemy.mana < 5)
                {
                    enemy.mana++;
                }
            }
            else
                enemyIsDead=true;
        }

        
        if (enemyIsDead)
        {
            playersTurn = !playersTurn;

            if (player.mana < 5)
                player.mana++;
        }
        else
        {
            SetTurnText();
            if (!playersTurn)
            {
                MonstersTurn();
            }
        }
        player.UpdateManaBalls();
        enemy.UpdateManaBalls();
    }
    internal void SetTurnText()
    {
        if (playersTurn)
        {
            turnText.text = "Merlin's Turn";
        }
        else
        {
            turnText.text = "Enemys Turn";
        }
    }
    private void MonstersTurn()
    {
       Card card =  MonsterAI();
        StartCoroutine(MonsterCastCard(card));
    }

    private Card MonsterAI()
    {
       List<Card> available = new List<Card>();
        for (int i = 0; i < 3; i++)
        {
            if (CardValid(enemyHand.cards[i], enemy, enemyHand))
                available.Add(enemyHand.cards[i]);
            else if (CardValid(enemyHand.cards[i], player, enemyHand))
                available.Add(enemyHand.cards[i]);
        }
        if (available.Count < 0) //none available
        {
            NextPlayersTurn();
            return null;
        }
        
        int choice = UnityEngine.Random.Range(0, available.Count);
        return available[choice];
    }
    private IEnumerator MonsterCastCard(Card card)
    {
        yield return new WaitForSeconds(0.5f);
        if(card)
        {
           TurnCard(card);

            yield return new WaitForSeconds(1);

            if(card.cardData.isDefenceCard)
            {
                UseCard(card, enemy, enemyHand); 
            }
            else
            {
                UseCard(card, player, enemyHand); 
            }

            yield return new WaitForSeconds(2);

            //Deal new card
            enemyDeck.DealCards(enemyHand);
        }else
        {
            enemySkipTurn.gameObject.SetActive(true);

            yield return new WaitForSeconds(1);

            enemySkipTurn.gameObject.SetActive(false);
        }
        
    }
    internal void TurnCard(Card card)
    {
        Animator animator = card.GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.SetTrigger("Flip");
        }
        else
            Debug.Log("No animator found");
    }
    private void UpdateScore()
    {
        scoreText.text = "Demon Killed: "+ playerKills.ToString()+ "  Score: "+ playerScore.ToString();
    }
    internal void PlayPlayerDieSound()
    {
        playerDieAudio.Play();
    }
    internal void PlayEnemyDieSound()
    {
        enemyDieAudio.Play();
    }


}
