using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCharacter : MonoBehaviour
{
    GameManager gameManager;
    List<Transform> targets;
    Transform nearestTarget;
    float nearestDistance;

    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        nearestDistance = 0;
        targets = new List<Transform>();
    }

    void Update()
    {
        if (!GetComponent<Character>().isFighting)
            return;

        nearestTarget = null;
        nearestDistance = 0;

        //Get list of targets
        if (GetComponent<Character>().isPlayerObject)
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
            float targetDistance = (target.position - transform.position).magnitude;
            if (nearestDistance == 0 || targetDistance <= nearestDistance)
            {
                nearestDistance = targetDistance;
                nearestTarget = target;
            }
        }

        if (nearestTarget != null)
        {
            bool isTouchingEnemy = false;
            if (GetComponent<Character>().isPlayerObject)
            {
                if (GetComponent<Rigidbody2D>().IsTouching(nearestTarget.GetComponent<CapsuleCollider2D>()))
                {
                    isTouchingEnemy = true;
                }
            }
            else
            {
                if (GetComponent<Rigidbody2D>().IsTouching(nearestTarget.GetComponent<CapsuleCollider2D>()))
                {
                    isTouchingEnemy = true;
                }
            }
            //print(name + " isTouching: " + isTouchingEnemy);

            //Check if at minimum distance
            if (nearestDistance > GetComponent<Character>().attackRange && !isTouchingEnemy)
            {
                //Move towards nearest target
                //self.position += (nearestTarget.position - self.position).normalized * character.moveSpeed * Time.deltaTime;
                GetComponent<Rigidbody2D>().MovePosition(transform.position + ((nearestTarget.position - transform.position).normalized * GetComponent<Character>().moveSpeed * Time.deltaTime));
            }
            else
            {
                //Attack
                GetComponent<Character>().Attack(nearestTarget.GetComponent<Character>());
            }
        }
    }
}
