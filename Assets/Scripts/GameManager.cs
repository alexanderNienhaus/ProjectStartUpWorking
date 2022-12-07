using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public System.Random r;
    public bool organSceneActive;
    public List<Transform> organs;
    public List<Transform> viruses;
    public List<Transform> rosterPositions;
    public List<Transform> shopPositions;
    public List<Transform> shopObjetcs;
    public List<Transform> frozenShopOrgans;
    public List<Transform> frozenShopViruses;
    public List<Transform> fightingEnemyObjects;
    public List<Transform> fightingPlayerObjects;
    public Transform roster;
    public Transform freeze;
    public Transform sell;
    public bool isFighting;

    public int gold;
    public int goldGainPerRound;
    public int goldBoostPerWin;
    public int goldCostOfRoll;
    public int goldCostOfBuy;
    public int goldGainOnSell;
    public int startGold;
    public int fieldBorderLeft;
    public int fieldBorderRight;
    public int fieldBorderTop;
    public int fieldBorderBot;

    TextMeshProUGUI goldText;
    int currentGoldBoost;
    TextMeshProUGUI roundText;
    int roundNumber;
    TextMeshProUGUI streakText;
    int currentStreak;

    GameObject organSetting;
    GameObject virusSetting;

    void Awake()
    {
        organSceneActive = true;
        r = new System.Random();
        fightingEnemyObjects = new List<Transform>();
        fightingPlayerObjects = new List<Transform>();
        organs = new List<Transform>();
        rosterPositions = new List<Transform>();
        shopPositions = new List<Transform>();
        shopObjetcs = new List<Transform>();
        frozenShopOrgans = new List<Transform>();
        frozenShopViruses = new List<Transform>();

        organSetting = GameObject.FindGameObjectWithTag("OrganSetting");
        virusSetting = GameObject.FindGameObjectWithTag("VirusSetting");
        virusSetting.SetActive(false);

        isFighting = false;

        roundNumber = 1;
        roundText = GameObject.FindGameObjectWithTag("RoundText").GetComponent<TextMeshProUGUI>();
        roundText.SetText("Next Round: " + roundNumber);

        gold = startGold;
        goldText = GameObject.FindGameObjectWithTag("GoldText").GetComponent<TextMeshProUGUI>();
        goldText.SetText("Gold: " + gold);
        currentGoldBoost = 0;

        currentStreak = 0;
        streakText = GameObject.FindGameObjectWithTag("StreakText").GetComponent<TextMeshProUGUI>();
        streakText.SetText("Current streak: " + currentStreak);

        //Get roster
        roster = GameObject.FindGameObjectWithTag("Roster").transform;

        //Get roster positions
        foreach (GameObject rosterPosition in GameObject.FindGameObjectsWithTag("RosterPosition"))
        {
            rosterPositions.Add(rosterPosition.transform);
        }
        rosterPositions = rosterPositions.OrderBy(go => go.name).ToList();

        //Get shop positions
        foreach (GameObject shopPosition in GameObject.FindGameObjectsWithTag("ShopPosition"))
        {
            shopPositions.Add(shopPosition.transform);
        }

        SpawnShop();

        freeze = GameObject.FindGameObjectWithTag("Freeze").transform;
        sell = GameObject.FindGameObjectWithTag("Sell").transform;

        DontDestroyOnLoad(this.gameObject);
    }

    public void StartRound()
    {
        if (isFighting)
            return;

        fightingEnemyObjects.Clear();
        fightingPlayerObjects.Clear();

        SpawnEnemies();

        //Get fighting organs
        foreach (Transform playerObject in organSceneActive ? organs : viruses)
        {
            if (!(playerObject.position.x > roster.position.x - roster.gameObject.GetComponent<SpriteRenderer>().bounds.size.x / 2
                && playerObject.position.x < roster.position.x + roster.gameObject.GetComponent<SpriteRenderer>().bounds.size.x / 2
                && playerObject.position.y > roster.position.y - roster.gameObject.GetComponent<SpriteRenderer>().bounds.size.y / 2
                && playerObject.position.y < roster.position.y + roster.gameObject.GetComponent<SpriteRenderer>().bounds.size.y / 2))
            {
                playerObject.GetComponent<Character>().isFighting = true;
                fightingPlayerObjects.Add(playerObject);
            }
        }

        isFighting = true;
    }

    void Update()
    {
        //Check if fight is over
        if (isFighting && (fightingEnemyObjects.Count == 0 || fightingPlayerObjects.Count == 0))
        {
            isFighting = false;

            //Reset all fightingPlayerObject
            foreach (Transform fightingPlayerObject in fightingPlayerObjects)
            {
                fightingPlayerObject.GetComponent<Character>().ResetCharacter();
                fightingPlayerObject.GetComponent<Character>().isFighting = false;
            }

            print("organSceneActive: "+organSceneActive + " fightingPlayerObjects.Count: " + fightingPlayerObjects.Count);
            //Put all fighting objects back in roster
            int i = 0;
            foreach (Transform rosterPosition in rosterPositions)
            {
                print("in");
                if (i >= (organSceneActive ? organs.Count : viruses.Count))
                    break;

                bool positionOccupied = false;
                foreach (Transform playerObject in organSceneActive ? organs : viruses)
                {
                    if (playerObject.position == rosterPosition.position)
                    {
                        positionOccupied = true;
                        break;
                    }
                }

                if (positionOccupied)
                    continue;

                print("Resetting position for "+ (organSceneActive ? organs : viruses)[i].name + " at " + rosterPosition.name);
                (organSceneActive ? organs : viruses)[i].position = rosterPosition.position;
                (organSceneActive ? organs : viruses)[i].GetComponent<Character>().lastPos = rosterPosition.position;
                i++;
            }

            if (fightingEnemyObjects.Count == 0 && fightingPlayerObjects.Count > 0)
            {
                //Player won
                print("PLAYER WON");
                if (currentStreak >= 0)
                {
                    currentStreak++;
                }
                else
                {
                    currentStreak = 1;
                }
                streakText.SetText("Current streak: " + currentStreak);
                gold += currentGoldBoost + goldGainPerRound;
                print("Gold won this round: " + (currentGoldBoost + goldGainPerRound) + " with current gold-boost at: " + currentGoldBoost);
                currentGoldBoost += goldBoostPerWin;
            }
            else if (fightingEnemyObjects.Count > 0 && fightingPlayerObjects.Count == 0)
            {
                //Enemy won
                print("PLAYER LOST");
                if (currentStreak <= 0)
                {
                    currentStreak--;
                }
                else
                {
                    currentStreak = -1;
                }
                streakText.SetText("Current streak: " + currentStreak);
                currentGoldBoost = 0;
                gold += goldGainPerRound;
                print("Gold won this round: " + (currentGoldBoost + goldGainPerRound) + " with current gold-boost at: " + currentGoldBoost);
            }
            else if (fightingEnemyObjects.Count == 0 && fightingPlayerObjects.Count == 0)
            {
                //Draw
                print("ITS A DRAW");
                gold += currentGoldBoost + goldGainPerRound;
                print("Gold won this round: " + (currentGoldBoost + goldGainPerRound) + " with current gold-boost at: " + currentGoldBoost);

            }
            UpdateGold(0);
            roundNumber++;
            roundText.SetText("Next Round: " + roundNumber);
            foreach (Transform enemy in fightingEnemyObjects)
            {
                Destroy(enemy.gameObject);
            }

            if (roundNumber % 2 == 0)
            {
                //Virus scene
                //SceneManager.LoadScene("VirusScene");
                print("Loading Virus Scene");
                organSceneActive = false;
                foreach (Transform organ in organs)
                {
                    organ.gameObject.SetActive(false);
                }
                foreach (Transform virus in viruses)
                {
                    virus.gameObject.SetActive(true);
                }

                organSetting.SetActive(false);
                virusSetting.SetActive(true);

                foreach (Transform froozenShopOrgan in frozenShopOrgans)
                {
                    if (froozenShopOrgan != null)
                        froozenShopOrgan.gameObject.SetActive(false);
                }

                foreach (Transform froozenShopVirus in frozenShopViruses)
                {
                    if (froozenShopVirus != null)
                        froozenShopVirus.gameObject.SetActive(true);
                }
            }
            else
            {
                //Organ scene
                //SceneManager.LoadScene("OrganScene");
                print("Loading Organ Scene");
                organSceneActive = true;
                foreach (Transform organ in organs)
                {
                    organ.gameObject.SetActive(true);
                }
                foreach (Transform virus in viruses)
                {
                    virus.gameObject.SetActive(false);
                }

                organSetting.SetActive(true);
                virusSetting.SetActive(false);

                foreach (Transform froozenShopOrgan in frozenShopOrgans)
                {
                    if(froozenShopOrgan != null)
                        froozenShopOrgan.gameObject.SetActive(true);
                }

                foreach (Transform froozenShopVirus in frozenShopViruses)
                {
                    if (froozenShopVirus != null)
                        froozenShopVirus.gameObject.SetActive(false);
                }
            }

            //Free roll??
            DestroyShop();
            SpawnShop();
        }
    }

    void SpawnEnemies()
    {
        for (int i = 0; i < roundNumber; i++)
        {
            string enemyName = "";
            if (organSceneActive)
            {
                switch (r.Next(0, 9))
                {
                    case 0:
                        enemyName = "Mumps";
                        break;
                    case 1:
                        enemyName = "AIDS";
                        break;
                    case 2:
                        enemyName = "Hepatitis";
                        break;
                    case 3:
                        enemyName = "Herpes";
                        break;
                    case 4:
                        enemyName = "Influenza";
                        break;
                    case 5:
                        enemyName = "Junin";
                        break;
                    case 6:
                        enemyName = "Lassa";
                        break;
                    case 7:
                        enemyName = "Measles";
                        break;
                    case 8:
                        enemyName = "Mumps";
                        break;
                    case 9:
                        enemyName = "VZV";
                        break;
                }
            }
            else
            {
                switch (r.Next(0, 4))
                {
                    case 0:
                        enemyName = "RedBloodCell";
                        break;
                    case 1:
                        enemyName = "Kidney";
                        break;
                    case 2:
                        enemyName = "Stomach";
                        break;
                    case 3:
                        enemyName = "Intestines";
                        break;
                }
            }

            GameObject enemy = (GameObject)Instantiate(Resources.Load(enemyName, typeof(GameObject)),
                new Vector3(r.Next(fieldBorderLeft, fieldBorderRight), r.Next(fieldBorderBot, fieldBorderTop), 0), Quaternion.identity);
            fightingEnemyObjects.Add(enemy.transform);
            enemy.GetComponent<Character>().isFighting = true;
            enemy.GetComponent<Character>().isPlayerObject = false;
        }
    }

    void SpawnShop()
    {
        foreach (Transform shopPosition in shopPositions)
        {
            //Put only in open shop position
            bool positionOccupied = false;
            foreach (Transform frozenOrgan in organSceneActive ? frozenShopOrgans : frozenShopViruses)
            {
                if (frozenOrgan == null)
                    continue;

                if (frozenOrgan.position == shopPosition.position)
                {
                    positionOccupied = true;
                    break;
                }
            }

            if (positionOccupied)
                continue;

            string playerObjectName = "";
            if (organSceneActive)
            {
                switch (r.Next(0, 5))
                {
                    case 0:
                        playerObjectName = "RedBloodCell";
                        break;
                    case 1:
                        playerObjectName = "Kidney";
                        break;
                    case 2:
                        playerObjectName = "Stomach";
                        break;
                    case 3:
                        playerObjectName = "Intestines";
                        break;
                    case 4:
                        playerObjectName = "RedBloodCell";
                        break;
                }
            }
            else
            {
                switch (r.Next(0, 9))
                {
                    case 0:
                        playerObjectName = "Mumps";
                        break;
                    case 1:
                        playerObjectName = "AIDS";
                        break;
                    case 2:
                        playerObjectName = "Hepatitis";
                        break;
                    case 3:
                        playerObjectName = "Herpes";
                        break;
                    case 4:
                        playerObjectName = "Influenza";
                        break;
                    case 5:
                        playerObjectName = "Junin";
                        break;
                    case 6:
                        playerObjectName = "Lassa";
                        break;
                    case 7:
                        playerObjectName = "Measles";
                        break;
                    case 8:
                        playerObjectName = "Mumps";
                        break;
                    case 9:
                        playerObjectName = "VZV";
                        break;
                }
            }

            GameObject playerObject = (GameObject)Instantiate(Resources.Load(playerObjectName, typeof(GameObject)), shopPosition.position, Quaternion.identity);
            shopObjetcs.Add(playerObject.transform);
            playerObject.GetComponent<Character>().isInShop = true;
        }
    }

    void UpdateGold(int pAdd)
    {
        gold += pAdd;
        goldText.SetText("Gold: " + gold);
    }

    void DestroyShop()
    {
        foreach (Transform shopObject in shopObjetcs)
        {
            if (!(organSceneActive ? frozenShopOrgans : frozenShopViruses).Contains(shopObject) && shopObject != null)
                Destroy(shopObject.gameObject);
        }
        shopObjetcs.Clear();
        shopObjetcs.AddRange(organSceneActive ? frozenShopOrgans : frozenShopViruses);
    }

    public void Roll()
    {
        if (gold == 0 || isFighting)
            return;
        UpdateGold(-goldCostOfRoll);
        DestroyShop();
        SpawnShop();
    }

    public void BuyShopObject(GameObject pShopObject)
    {
        UpdateGold(-goldCostOfBuy);
        pShopObject.GetComponent<Character>().isInShop = false;
        pShopObject.GetComponent<Character>().isPlayerObject = true;
        shopObjetcs.Remove(pShopObject.transform);
        if (organSceneActive)
        {
            organs.Add(pShopObject.transform);
        }
        else
        {
            viruses.Add(pShopObject.transform);
        }
        if ((organSceneActive ? frozenShopOrgans : frozenShopViruses).Contains(pShopObject.transform))
        {
            pShopObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
        }
        CheckForCombineObjects();
    }

    public void FreezeShopObject(GameObject pShopObject)
    {
        if (!(organSceneActive ? frozenShopOrgans : frozenShopViruses).Contains(pShopObject.transform))
        {
            //Freeze
            (organSceneActive ? frozenShopOrgans : frozenShopViruses).Add(pShopObject.transform);
            pShopObject.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0);
        }
        else
        {
            //Unfreeze
            (organSceneActive ? frozenShopOrgans : frozenShopViruses).Remove(pShopObject.transform);
            pShopObject.GetComponent<SpriteRenderer>().color = new Color(255, 255, 255);
        }

    }

    public void SellObject(GameObject pObject)
    {
        UpdateGold(goldGainOnSell);
        shopObjetcs.Remove(pObject.transform);
        if ((organSceneActive ? frozenShopOrgans : frozenShopViruses).Contains(pObject.transform))
            shopObjetcs.Remove(pObject.transform);
        if (organSceneActive)
        {
            organs.Remove(pObject.transform);
        }
        else
        {
            viruses.Remove(pObject.transform);
        }
        Destroy(pObject);
    }

    void CheckForCombineObjects()
    {
        if (organSceneActive)
        {
            List<Transform> redBloodCellsLevelOne = new List<Transform>();
            List<Transform> kidneysLevelOne = new List<Transform>();
            List<Transform> stomachsLevelOne = new List<Transform>();
            List<Transform> intestinesLevelOne = new List<Transform>();
            List<Transform> redBloodCellsLevelTwo = new List<Transform>();
            List<Transform> kidneysLevelTwo = new List<Transform>();
            List<Transform> stomachsLevelTwo = new List<Transform>();
            List<Transform> intestinesLevelTwo = new List<Transform>();

            foreach (Transform organ in organs)
            {
                if (organ.name.Contains("RedBloodCell"))
                {
                    if (organ.GetComponent<Character>().level == 1)
                    {
                        redBloodCellsLevelOne.Add(organ);
                    }
                    else if (organ.GetComponent<Character>().level == 2)
                    {
                        redBloodCellsLevelTwo.Add(organ);
                    }
                }
                else if (organ.name.Contains("Kidney"))
                {
                    if (organ.GetComponent<Character>().level == 1)
                    {
                        kidneysLevelOne.Add(organ);
                    }
                    else if (organ.GetComponent<Character>().level == 2)
                    {
                        kidneysLevelTwo.Add(organ);
                    }
                }
                else if (organ.name.Contains("Stomach"))
                {
                    if (organ.GetComponent<Character>().level == 1)
                    {
                        stomachsLevelOne.Add(organ);
                    }
                    else if (organ.GetComponent<Character>().level == 2)
                    {
                        stomachsLevelTwo.Add(organ);
                    }
                }
                else if (organ.name.Contains("Intestines"))
                {
                    if (organ.GetComponent<Character>().level == 1)
                    {
                        intestinesLevelOne.Add(organ);
                    }
                    else if (organ.GetComponent<Character>().level == 2)
                    {
                        intestinesLevelTwo.Add(organ);
                    }
                }
            }

            bool combinationHappened = false;
            if (redBloodCellsLevelOne.Count > 2)
            {
                Destroy(redBloodCellsLevelOne[0].gameObject);
                organs.Remove(redBloodCellsLevelOne[0].transform);
                Destroy(redBloodCellsLevelOne[1].gameObject);
                organs.Remove(redBloodCellsLevelOne[1].transform);
                redBloodCellsLevelOne[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }
            if (kidneysLevelOne.Count > 2)
            {
                Destroy(kidneysLevelOne[0].gameObject);
                organs.Remove(kidneysLevelOne[0].transform);
                Destroy(kidneysLevelOne[1].gameObject);
                organs.Remove(kidneysLevelOne[1].transform);
                kidneysLevelOne[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }
            if (stomachsLevelOne.Count > 2)
            {
                Destroy(stomachsLevelOne[0].gameObject);
                organs.Remove(stomachsLevelOne[0].transform);
                Destroy(stomachsLevelOne[1].gameObject);
                organs.Remove(stomachsLevelOne[1].transform);
                stomachsLevelOne[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }
            if (intestinesLevelOne.Count > 2)
            {
                Destroy(intestinesLevelOne[0].gameObject);
                organs.Remove(intestinesLevelOne[0].transform);
                Destroy(intestinesLevelOne[1].gameObject);
                organs.Remove(intestinesLevelOne[1].transform);
                intestinesLevelOne[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }

            if (redBloodCellsLevelTwo.Count > 2)
            {
                Destroy(redBloodCellsLevelTwo[0].gameObject);
                organs.Remove(redBloodCellsLevelTwo[0].transform);
                Destroy(redBloodCellsLevelTwo[1].gameObject);
                organs.Remove(redBloodCellsLevelTwo[1].transform);
                redBloodCellsLevelTwo[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }
            if (kidneysLevelTwo.Count > 2)
            {
                Destroy(kidneysLevelTwo[0].gameObject);
                organs.Remove(kidneysLevelTwo[0].transform);
                Destroy(kidneysLevelTwo[1].gameObject);
                organs.Remove(kidneysLevelTwo[1].transform);
                kidneysLevelTwo[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }
            if (stomachsLevelTwo.Count > 2)
            {
                Destroy(stomachsLevelTwo[0].gameObject);
                organs.Remove(stomachsLevelTwo[0].transform);
                Destroy(stomachsLevelTwo[1].gameObject);
                organs.Remove(stomachsLevelTwo[1].transform);
                stomachsLevelTwo[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }
            if (intestinesLevelTwo.Count > 2)
            {
                Destroy(intestinesLevelTwo[0].gameObject);
                organs.Remove(intestinesLevelTwo[0].transform);
                Destroy(intestinesLevelTwo[1].gameObject);
                organs.Remove(intestinesLevelTwo[1].transform);
                intestinesLevelTwo[2].GetComponent<Character>().UpgradeCharacter();
                combinationHappened = true;
            }

            if (combinationHappened)
                CheckForCombineObjects();
        } else
        {

        }
    }
}
