using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Character : MonoBehaviour
{
    public bool isDragging;
    public Vector2 lastPos;
    public bool isPlayerObject;
    public bool isFighting;
    public bool isInShop;
    public int level;

    //Stats
    public int atk;
    public int def;
    public int hp;
    public float moveSpeed;
    public float attackSpeed;
    public float attackRange;
    //Reset stats
    int maxAtk;
    int maxDef;
    int maxHp;
    float maxMoveSpeed;
    float maxAttackSpeed;
    float maxAttackRange;
    //Upgrade stats
    int upgradeMaxAtk;
    int upgradeMaxDef;
    int upgradeMaxHp;
    float upgradeMaxMoveSpeed;
    float upgradeMaxAttackSpeed;
    float upgradeMaxAttackRange;

    GameManager gameManager;
    Transform transform;
    float timeSinceLastAttack;

    List<AudioSource> audioSources;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        transform = GetComponent<Transform>();
        lastPos = transform.position;
        timeSinceLastAttack = 0;
        isDragging = false;
        isInShop = true;
        level = 1;
        audioSources = new List<AudioSource>();
        foreach (AudioSource audioSource in GetComponents<AudioSource>())
        {
            audioSources.Add(audioSource);
        }

        maxDef = def;
        maxAtk = atk;
        maxHp = hp;
        maxMoveSpeed = moveSpeed;
        maxAttackSpeed = attackSpeed;
        maxAttackRange = attackRange;
    }

    void Update()
    {
    }

    public void Attack(Character pTarget)
    {
        timeSinceLastAttack += Time.deltaTime;
        if (timeSinceLastAttack >= attackSpeed)
        {
            timeSinceLastAttack = 0;

            int dmg = atk - pTarget.def;
            pTarget.DoDmg(dmg);

            audioSources[0].Play();
            print(transform.name + " attacked " + pTarget.name + " with " + dmg + " Dmg!");
        }
    }

    public void DoDmg(int pDmg)
    {
        hp -= pDmg;
        if (hp < 0)
            hp = 0;
        if (hp == 0)
        {
            Camera.main.GetComponent<AudioSource>().Play();
            isFighting = false;
            if (isPlayerObject)
            {
                gameManager.fightingPlayerObjects.Remove(transform);
            }
            else
            {
                gameManager.fightingEnemyObjects.Remove(transform);
                Destroy(gameObject);
            }
            gameObject.SetActive(false);
        }
    }

    public void ResetCharacter()
    {
        gameObject.SetActive(true);
        atk = maxAtk;
        def = maxDef;
        hp = maxHp;
        attackSpeed = maxAttackSpeed;
        moveSpeed = maxMoveSpeed;
        attackRange = maxAttackRange;
        transform.position = lastPos;
    }

    public void UpgradeCharacter()
    {
        transform.localScale *= 1.1f;

        atk += upgradeMaxAtk;
        maxAtk = atk;

        def += upgradeMaxDef;
        maxDef = def;

        hp += upgradeMaxHp;
        maxHp = hp;

        attackRange += upgradeMaxAttackRange;
        maxAttackRange = attackRange;

        attackSpeed += upgradeMaxAttackSpeed;
        maxAttackSpeed = attackSpeed;

        moveSpeed += upgradeMaxMoveSpeed;
        maxMoveSpeed = moveSpeed;

        level++;
    }
}
