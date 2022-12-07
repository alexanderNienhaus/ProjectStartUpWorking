using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCharacter : MonoBehaviour
{
    GameManager gameManager;
    List<Transform> targets;
    Transform self;
    Transform nearestTarget;
    Character character;
    float nearestDistance;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        nearestDistance = 0;
        targets = new List<Transform>();
        self = GetComponent<Transform>();
        character = GetComponent<Character>();
    }

    void Update()
    {
        if (!GetComponent<Character>().isFighting)
            return;

        nearestTarget = null;
        nearestDistance = 0;

        //Get list of targets
        if (character.isPlayerObject)
        {
            targets.Clear();
            targets.AddRange(gameManager.fightingEnemyObjects);
        }
        else
        {
            targets.Clear();
            targets.AddRange(gameManager.fightingPlayerObjects);
        }

        //Get nearest target from all targets
        foreach (Transform target in targets)
        {
            float targetDistance = (target.position - self.position).magnitude;
            if (nearestDistance == 0 || targetDistance <= nearestDistance)
            {
                nearestDistance = targetDistance;
                nearestTarget = target;
            }
        }

        bool isTouchingEnemy = false;
        if (GetComponent<Character>().isPlayerObject)
        {
            foreach (Transform enemy in gameManager.fightingEnemyObjects)
            {
                if (GetComponent<Rigidbody2D>().IsTouching(enemy.GetComponent<CapsuleCollider2D>()))
                {
                    isTouchingEnemy = true;
                }
            }
        }
        else
        {
            foreach (Transform organ in gameManager.fightingPlayerObjects)
            {
                if (GetComponent<Rigidbody2D>().IsTouching(organ.GetComponent<CapsuleCollider2D>()))
                {
                    isTouchingEnemy = true;
                }
            }
        }

        if (nearestTarget != null)
        {
            //Check if at minimum distance
            if (nearestDistance > GetComponent<Character>().attackRange && !isTouchingEnemy)
            {
                //Move towards nearest target
                self.position += (nearestTarget.position - self.position).normalized * character.moveSpeed * Time.deltaTime;
                //self.GetComponent<Rigidbody2D>().MovePosition(self.position + (nearestTarget.position - self.position).normalized * character.moveSpeed * Time.deltaTime);
            }
            else
            {
                //Attack
                self.GetComponent<Character>().Attack(nearestTarget.GetComponent<Character>());
            }
        }
    }
}
