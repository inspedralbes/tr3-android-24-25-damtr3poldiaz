using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject[] monsterPrefabs; // Array de prefabs de monstruos
    public Transform[] spawnPoints; // Puntos donde pueden aparecer los monstruos
    public float spawnRate = 3f; // Tiempo entre spawns
    public int maxMonsters = 10; // Máximo de monstruos en la escena

    private List<GameObject> activeMonsters = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnMonsters());
    }

    IEnumerator SpawnMonsters()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnRate);

            // Elimina monstruos destruidos de la lista antes de spawnear nuevos
            CleanMonsterList();

            if (activeMonsters.Count < maxMonsters)
            {
                SpawnMonster();
            }
        }
    }

    void SpawnMonster()
    {
        if (monsterPrefabs.Length == 0 || spawnPoints.Length == 0) return;

        // Selecciona un monstruo y un punto de spawn aleatorio
        GameObject monsterPrefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        // Instancia el monstruo
        GameObject newMonster = Instantiate(monsterPrefab, spawnPoint.position, Quaternion.identity);
        
        // Verifica si tiene el script Monster antes de agregarlo
        Monster monsterScript = newMonster.GetComponent<Monster>();
        if (monsterScript != null)
        {
            monsterScript.OnMonsterDestroyed += () => RemoveMonster(newMonster);
            activeMonsters.Add(newMonster);
        }
        else
        {
            Debug.LogWarning("El monstruo instanciado no tiene el script 'Monster'.");
            Destroy(newMonster);
        }
    }

    void RemoveMonster(GameObject monster)
    {
        if (activeMonsters.Contains(monster))
        {
            activeMonsters.Remove(monster);
        }
    }

    void CleanMonsterList()
    {
        activeMonsters.RemoveAll(monster => monster == null);
    }
}
