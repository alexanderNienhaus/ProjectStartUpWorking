using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragAndDrop : MonoBehaviour
{
    List<Transform> targets;
    List<Transform> rosterPositions;
    Transform roster;
    GameManager gameManager;

    void Start()
    {
        targets = new List<Transform>();
        rosterPositions = new List<Transform>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        rosterPositions.AddRange(gameManager.rosterPositions);
        roster = gameManager.roster;
    }

    void Update()
    {
        if (gameManager.isFighting)
            return;

        targets.Clear();
        targets.AddRange(gameManager.organSceneActive ? gameManager.organs : gameManager.viruses);
        targets.AddRange(gameManager.shopObjetcs);

        foreach (Transform target in targets)
        {
            if (target == null)
                continue;

            if (Input.GetMouseButtonUp(0) && target.GetComponent<Character>().isDragging == true)
            {
                //Dropping a target
                print("dropping");
                target.GetComponent<Character>().isDragging = false;
                target.GetComponent<SpriteRenderer>().sortingLayerName = "Organs";
                target.GetComponent<CapsuleCollider2D>().enabled = true;

                if (roster.GetComponent<BoxCollider2D>().OverlapPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)))))
                {
                    //In roster
                    print("in roster");
                    Transform droppedOnRosterPos = null;
                    foreach (Transform rosterPos in rosterPositions)
                    {
                        if (rosterPos.GetComponent<BoxCollider2D>().OverlapPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)))))
                        {
                            droppedOnRosterPos = rosterPos;
                            break;
                        }
                    }

                    if (droppedOnRosterPos != null
                        && (!target.GetComponent<Character>().isInShop || (gameManager.gold >= gameManager.goldCostOfBuy
                        && (gameManager.organSceneActive ? gameManager.organs : gameManager.viruses).Count < gameManager.rosterPositions.Count)))
                    {
                        //On roster position
                        print("on roster pos");
                        Transform onOrganDrop = null;
                        foreach (Transform otherTarget in targets)
                        {
                            if (otherTarget == null)
                                continue;

                            if (otherTarget.position == droppedOnRosterPos.position)
                            {
                                onOrganDrop = otherTarget;
                                break;
                            }
                        }

                        if (onOrganDrop != null)
                        {
                            //On another target
                            print("on another target");
                            if (!target.GetComponent<Character>().isInShop)
                            {
                                //Swap with other organ, if not from shop
                                print("so swap");
                                target.position = droppedOnRosterPos.position;
                                onOrganDrop.position = target.GetComponent<Character>().lastPos;
                                target.GetComponent<Character>().lastPos = target.position;
                                onOrganDrop.GetComponent<Character>().lastPos = onOrganDrop.position;
                                break;
                            }
                            else
                            {
                                //Reset if from shop
                                print("so reset, because from shop");
                                target.position = target.GetComponent<Character>().lastPos;
                                break;
                            }
                        }
                        else
                        {
                            //On empty roster position
                            print("on empty roster pos");
                            target.position = droppedOnRosterPos.position;
                            target.GetComponent<Character>().lastPos = target.position;
                            if (target.GetComponent<Character>().isInShop)
                            {
                                gameManager.BuyShopObject(target.gameObject);
                            }
                            break;
                        }
                    }
                    else if (gameManager.freeze.GetComponent<BoxCollider2D>().OverlapPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z))))
                        && target.GetComponent<Character>().isInShop)
                    {
                        //On freeze
                        print("on freeze");
                        target.position = target.GetComponent<Character>().lastPos;
                        gameManager.FreezeShopObject(target.gameObject);
                        break;
                    }
                    else if (gameManager.sell.GetComponent<BoxCollider2D>().OverlapPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z))))
                        && !target.GetComponent<Character>().isInShop)
                    {
                        //On sell
                        print("on sell");
                        gameManager.SellObject(target.gameObject);
                        break;
                    }
                    else
                    {
                        //Outside field position, so reset
                        print("outside field pos, so reset");
                        target.position = target.GetComponent<Character>().lastPos;
                        break;
                    }
                }
                else if (!target.GetComponent<Character>().isInShop || (gameManager.gold >= gameManager.goldCostOfBuy
                    && (gameManager.organSceneActive ? gameManager.organs : gameManager.viruses).Count < gameManager.rosterPositions.Count))
                {
                    //Outside roster
                    print("outside roster");
                    Transform onOrganDrop = null;
                    foreach (Transform otherTarget in targets)
                    {
                        if (otherTarget == target || otherTarget == null)
                            continue;

                        if (otherTarget.GetComponent<CapsuleCollider2D>().OverlapPoint(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)))))
                        {
                            onOrganDrop = otherTarget;
                            break;
                        }
                    }

                    if (onOrganDrop == null)
                    {
                        //Not on another target
                        print("not on another target");
                        target.GetComponent<Character>().lastPos = target.position;
                        if (target.GetComponent<Character>().isInShop)
                        {
                            gameManager.BuyShopObject(target.gameObject);
                        }
                        break;
                    }
                    else
                    {
                        //On another target
                        print("on another organ");
                        if (target.GetComponent<Character>().isInShop)
                        {
                            //Target is from shop, so reset
                            print("Target is from shop, so reset");
                            target.position = target.GetComponent<Character>().lastPos;
                            break;
                        }
                        else
                        {
                            //Target is not from shop, so swap
                            print("Target is not from shop, so swap");
                            target.position = onOrganDrop.position;
                            onOrganDrop.position = target.GetComponent<Character>().lastPos;
                            target.GetComponent<Character>().lastPos = target.position;
                            onOrganDrop.GetComponent<Character>().lastPos = onOrganDrop.position;
                            break;
                        }
                    }
                }
                else
                {
                    //Reset
                    print("Reset");
                    target.position = target.GetComponent<Character>().lastPos;
                }
            }
            else if (Input.GetMouseButtonDown(0)
                      && Input.mousePosition.x > Camera.main.WorldToScreenPoint(target.position).x - target.gameObject.GetComponent<SpriteRenderer>().bounds.size.x / 2
                      && Input.mousePosition.x < Camera.main.WorldToScreenPoint(target.position).x + target.gameObject.GetComponent<SpriteRenderer>().bounds.size.x / 2
                      && Input.mousePosition.y > Camera.main.WorldToScreenPoint(target.position).y - target.gameObject.GetComponent<SpriteRenderer>().bounds.size.y / 2
                      && Input.mousePosition.y < Camera.main.WorldToScreenPoint(target.position).y + target.gameObject.GetComponent<SpriteRenderer>().bounds.size.y / 2)
            {
                //Picking up target
                print("Picking up target");
                target.GetComponent<CapsuleCollider2D>().enabled = false;
                target.GetComponent<Character>().isDragging = true;
                target.GetComponent<SpriteRenderer>().sortingLayerName = "DragOrgan";
            }

            if (target.GetComponent<Character>().isDragging)
            {
                //Dragging target
                target.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Mathf.Abs(Camera.main.transform.position.z)));
            }
        }
    }
}
