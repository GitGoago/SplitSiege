using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SpawningManager : MonoBehaviour
{

    #region Singleton

    public static SpawningManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }


    #endregion Singleton

    [SerializeField]
    int credits;


    [SerializeField]
    List<SpawningEvent> spawns;

    List<float> cooldowns = new List<float>();

    [SerializeField]
    float globalCD = 10.0f;

    [SerializeField]
    Transform spawnLocation;


    private void Start()
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            cooldowns.Add(0.0f);
        }
    }


    private void Update()
    {
        if (GameStateManager.instance.GetState() == GameStateManager.GameState.Fighting)
        {
            HandleCooldowns();
            AttemptEvents();

            if (LevelIsDone())
            {
                GameStateManager.instance.SetState(GameStateManager.GameState.Building);
                ClearCooldowns();
            }
        }
        
    }

    bool LevelIsDone()
    {
        return DoneSpawning() && UnitBehavior.allEnemies.Count == 0;
    }

    public bool DoneSpawning()
    {
        int smallest = 10000;
        foreach (SpawningEvent se in spawns)
        {
            if (se.GetCost() < smallest) smallest = se.GetCost();
        }
        return smallest > credits;
    }

    void AttemptEvents()
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            if (OffCooldown(i) && credits > spawns[i].GetCost())
            {
                InvokeEventCooldown(i);
                if (spawns[i].Success())
                {
                    credits -= spawns[i].GetCost();
                    StartCoroutine(SpawnEvent(spawns[i]));
                    InvokeGlobalCooldown();
                    return;
                }
            }
        }
    }


    IEnumerator SpawnEvent(SpawningEvent spawningEvent)
    {
        foreach (GameObject unit in spawningEvent.GetSpawns())
        {
            Instantiate(unit, spawnLocation.position, Quaternion.identity, null);

            float randomWait = UnityEngine.Random.Range(0.5f, 2.0f);
            yield return new WaitForSeconds(randomWait);
        }
    }

    void ClearCooldowns()
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            if (cooldowns[i] > 0)
            {
                cooldowns[i] = 0;
            }
        }
    }

    void HandleCooldowns()
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            if (cooldowns[i] > 0)
            {
                cooldowns[i] -= Time.deltaTime;
            }
        }
    }

    void InvokeGlobalCooldown()
    {
        for (int i = 0; i < spawns.Count; i++)
        {
            cooldowns[i] += globalCD;
        }
    }

    void InvokeEventCooldown(int index)
    {
        cooldowns[index] += spawns[index].GetCooldown();
    }

    bool OffCooldown(int index)
    {
        return cooldowns[index] <= 0;
    }






}