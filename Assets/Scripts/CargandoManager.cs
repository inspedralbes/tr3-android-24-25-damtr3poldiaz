using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using Cainos.PixelArtTopDown_Basic;
using UnityEngine.SceneManagement;

public class CargandoManager : MonoBehaviour
{
    private string gameConfigUrl = "http://localhost:3000/game-config/debug";

    // Variables para almacenar la configuración obtenida
    private MonsterConfig cachedMonsterConfig;
    private int cachedMonsterStrength;

    // Prefabs (opcional, en caso de que necesites instanciarlos)
    public GameObject monsterPrefab;
    public GameObject playerPrefab;

    private void Start()
    {
        StartCoroutine(GetGameConfig());
    }
    void Awake()
    {
        DontDestroyOnLoad(gameObject); // Hace que el objeto persista entre escenas
    }


    private IEnumerator GetGameConfig()
    {
        UnityWebRequest request = UnityWebRequest.Get(gameConfigUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string jsonResponse = request.downloadHandler.text;
            GameConfig config = JsonUtility.FromJson<GameConfig>(jsonResponse);

            // Guardar la configuración
            cachedMonsterConfig = config.monster;
            cachedMonsterStrength = config.monster.strength;

            // Cargar la escena Main
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Main");
        }
        else
        {
            Debug.LogError("Error al obtener la configuración del juego: " + request.error);
        }
    }
    private IEnumerator WaitForMonsters()
    {
        // Espera hasta que haya monstruos activos en la escena
        yield return new WaitUntil(() => FindObjectsOfType<Monster>().Length > 0);

        // Busca los monstruos
        Monster[] monsters = FindObjectsOfType<Monster>();
        if (monsters.Length > 0)
        {
            // Configura los monstruos
            foreach (Monster monster in monsters)
            {
                ConfigureMonster(monster, cachedMonsterConfig);
            }
        }
        else
        {
            Debug.LogError("No se encontraron monstruos generados en la escena.");
        }
    }


    // Este método se llamará cuando la escena "Main" se haya cargado
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(scene.name == "Main")
        {
            // Verificar si CargandoManager aún existe
            if (this != null)
            {
                // Llamar a la corutina para esperar los monstruos
                StartCoroutine(WaitForMonsters());

                // Configurar al jugador
                ConfigurePlayer(cachedMonsterStrength);
            }
            else
            {
                Debug.LogWarning("CargandoManager no está disponible en la escena.");
            }

            // Desuscribirse del evento para evitar llamadas futuras
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }



    private void ConfigureMonster(Monster monster, MonsterConfig monsterConfig)
    {
        if (monster != null)
        {
            monster.speed = monsterConfig.speed;
            monster.health = monsterConfig.health;
            Debug.Log("Monster configurado: Speed=" + monster.speed + ", Health=" + monster.health);
        }
        else
        {
            Debug.LogError("No se pudo configurar el monstruo.");
        }
    }


    private void ConfigurePlayer(int monsterStrength)
    {
        TopDownCharacterController player = FindObjectOfType<TopDownCharacterController>();
        if (player == null && playerPrefab != null)
        {
            GameObject playerObj = Instantiate(playerPrefab);
            player = playerObj.GetComponent<TopDownCharacterController>();
        }
        
        if (player != null)
        {
            player.monsterDamage = monsterStrength;
            Debug.Log("Daño del monster para el jugador configurado a: " + player.monsterDamage);
        }
        else
        {
            Debug.LogError("No se encontró un objeto TopDownCharacterController en la escena ni se pudo instanciar desde el prefab.");
        }
    }
}

[System.Serializable]
public class GameConfig
{
    public MonsterConfig monster;
    // Se ignora "skin" ya que no se va a usar
    public string timestamp;
}

[System.Serializable]
public class MonsterConfig
{
    public int id;
    public string name;
    public int strength;
    public float speed;
    public int health;
    public string sprite;
}
