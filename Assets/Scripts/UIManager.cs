using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private void OnEnable()
    {
        // Obtener el UIDocument
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Buscar el botón por su nombre en el UXML
        Button loginButton = root.Q<Button>("btnLogin");

        // Asignar evento al botón
        if (loginButton != null)
        {
            loginButton.clicked += () => SceneManager.LoadScene("Login");
        }
        else
        {
            Debug.LogError("⚠️ Botón 'btnLogin' no encontrado en el UI Document.");
        }
    }
}
