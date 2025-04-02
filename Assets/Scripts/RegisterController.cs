using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement; // Importar para manejar las escenas

public class RegisterController : MonoBehaviour
{
    private TextField usernameField;
    private TextField emailField;
    private TextField passwordField;
    private Button registerButton;
    private VisualElement root;

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
        usernameField = root.Q<TextField>("usernameField");
        emailField = root.Q<TextField>("emailField");
        passwordField = root.Q<TextField>("passwordField");
        registerButton = root.Q<Button>("registerButton");

        if (registerButton != null)
        {
            registerButton.clicked += OnRegisterButtonClicked;
        }
        else
        {
            Debug.LogError("Register button not found in UI");
        }
    }

    private void OnRegisterButtonClicked()
    {
        if (usernameField == null || emailField == null || passwordField == null)
        {
            Debug.LogError("One or more input fields are missing");
            return;
        }
        
        string username = usernameField.value.Trim();
        string email = emailField.value.Trim();
        string password = passwordField.value;
        
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogError("All fields must be filled out");
            return;
        }

        StartCoroutine(RegisterUser(username, email, password));
    }

    private IEnumerator RegisterUser(string username, string email, string password)
    {
        string jsonData = JsonUtility.ToJson(new UserData(username, email, password));
        
        using (UnityWebRequest request = new UnityWebRequest("http://localhost:3000/auth/register", "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Usuario registrado exitosamente");

                // Cargar la escena de Login
                SceneManager.LoadScene("Login"); // Asegúrate de que "Login" sea el nombre de la escena que quieres cargar
            }
            else
            {
                Debug.LogError("Error en el registro: " + request.error);
            }
        }
    }
}

[System.Serializable]
public class UserData
{
    public string username;
    public string email;
    public string password;

    public UserData(string username, string email, string password)
    {
        this.username = username;
        this.email = email;
        this.password = password;
    }
}
