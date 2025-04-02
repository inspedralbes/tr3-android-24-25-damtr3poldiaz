using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement; // Importar SceneManager

public class LoginManager : MonoBehaviour
{
    private string loginUrl = "http://localhost:3000/auth/login";

    private void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        TextField emailField = root.Q<TextField>("emailField");
        TextField passwordField = root.Q<TextField>("passwordField");
        Button loginButton = root.Q<Button>("loginButton");
        Button registerButton = root.Q<Button>("registerButton"); // Obtener el botón de registro
        Label errorLabel = root.Q<Label>("errorLabel");

        errorLabel.text = "";
        
        loginButton.clicked += () =>
        {
            StartCoroutine(Login(emailField.text, passwordField.text, errorLabel));
        };

        registerButton.clicked += () => // Agregar evento al botón de registro
        {
            SceneManager.LoadScene("Register"); // Cargar la escena de registro
        };
    }

    private IEnumerator Login(string email, string password, Label errorLabel)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            errorLabel.text = "Los campos no pueden estar vacíos.";
            yield break;
        }

        string jsonPayload = $"{{\"email\": \"{email}\", \"password\": \"{password}\"}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        using (UnityWebRequest request = new UnityWebRequest(loginUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login exitoso: " + request.downloadHandler.text);
                errorLabel.text = "Login exitoso.";

                // Parsear la respuesta JSON
                string jsonResponse = request.downloadHandler.text;
                var json = JsonUtility.FromJson<LoginResponse>(jsonResponse);

                // Guardar id y username en PlayerPrefs
                PlayerPrefs.SetInt("UserID", json.user.id);
                PlayerPrefs.SetString("Username", json.user.username);

                // Asegurarse de guardar los datos
                PlayerPrefs.Save();

                // Cargar la escena "Cargando"
                SceneManager.LoadScene("Cargando");
            }
            else
            {
                Debug.LogError("Error en login: " + request.error);
                errorLabel.text = "Error en login. Verifique sus credenciales.";
            }
        }
    }

    // Clase para deserializar la respuesta JSON
    [System.Serializable]
    public class LoginResponse
    {
        public string message;
        public User user;
    }

    [System.Serializable]
    public class User
    {
        public int id;
        public string username;
    }
}
