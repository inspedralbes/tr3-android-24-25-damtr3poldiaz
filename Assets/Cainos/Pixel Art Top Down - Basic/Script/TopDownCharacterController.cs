using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cargar escenas
using UnityEngine.UI;

namespace Cainos.PixelArtTopDown_Basic
{
    public class TopDownCharacterController : MonoBehaviour
    {
        public GameObject bulletPrefab; // Prefab de la bala
        public Transform firePoint; // Punto de disparo
        public float fireRate = 0.5f; // Tiempo entre disparos
        private float nextFireTime = 0f;
        private Vector2 lastDirection = Vector2.right; // Última dirección de movimiento

        public float speed;
        public int health = 100; // Vida del jugador
        private Color originalColor; // Color original del jugador
        private SpriteRenderer spriteRenderer;
        public Slider healthBar;
        private Animator animator;
        private Rigidbody2D rb;
        public int monsterDamage = 10;

        // Referencias a la UI
        public GameObject gameOverPanel; // Panel de Game Over
        private void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            originalColor = spriteRenderer.color;
            gameOverPanel.SetActive(false);

            // Espera un frame antes de cargar el sprite
            StartCoroutine(DelayedSpriteLoad());
        }

        private IEnumerator DelayedSpriteLoad()
        {
            yield return null; // Espera un frame
            LoadSkinSprite();
        }


        private void LoadSkinSprite()
        {
            string spritePath = PlayerPrefs.GetString("SkinSpritePath", "Textures/defaultSprite"); // Ruta relativa sin extensión
            Sprite newSprite = Resources.Load<Sprite>(spritePath);
            
            if (newSprite != null)
            {
                spriteRenderer.sprite = newSprite;
                Debug.Log("Sprite cargado correctamente: " + spritePath);
                
                // Verifica que el sprite realmente se está asignando
                if (spriteRenderer.sprite != null)
                {
                    Debug.Log("Sprite asignado correctamente: " + spriteRenderer.sprite.name);
                }
                else
                {
                    Debug.LogError("No se ha asignado el sprite al SpriteRenderer");
                }
            }
            else
            {
                Debug.LogError("No se pudo cargar el sprite: " + spritePath);
            }
        }




        private void Update()
        {
            // Solo permitir el movimiento si el jugador está vivo
            if (health > 0)
            {
                Vector2 dir = Vector2.zero;

                if (Input.GetKey(KeyCode.A))
                {
                    dir.x = -1;
                    animator.SetInteger("Direction", 3);
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    dir.x = 1;
                    animator.SetInteger("Direction", 2);
                }

                if (Input.GetKey(KeyCode.W))
                {
                    dir.y = 1;
                    animator.SetInteger("Direction", 1);
                }
                else if (Input.GetKey(KeyCode.S))
                {
                    dir.y = -1;
                    animator.SetInteger("Direction", 0);
                }

                // Si el personaje se mueve, guarda la última dirección
                if (dir.magnitude > 0)
                {
                    lastDirection = dir.normalized;
                }

                dir.Normalize();
                animator.SetBool("IsMoving", dir.magnitude > 0);

                rb.linearVelocity = speed * dir;

                // Disparo con espacio
                if (Input.GetKeyDown(KeyCode.Space) && Time.time > nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + fireRate;
                }
            }
        }

        void Shoot()
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            bullet.layer = gameObject.layer; // Asigna la misma capa que el jugador

            // Asegurar que la bala tenga la misma Sorting Layer y Order in Layer
            SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
            SpriteRenderer bulletSprite = bullet.GetComponent<SpriteRenderer>();
            if (playerSprite != null && bulletSprite != null)
            {
                bulletSprite.sortingLayerID = playerSprite.sortingLayerID;
                bulletSprite.sortingOrder = playerSprite.sortingOrder + 1;
            }

            // Ignorar colisión con el jugador
            Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
            Collider2D playerCollider = GetComponent<Collider2D>();
            if (bulletCollider != null && playerCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, playerCollider);
            }

            // Obtener el script Bullet y pasar la dirección
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(lastDirection);
            }
        }

        public void TakeDamage(int damage)
        {
            health -= damage;

            // Actualizar la barra de vida
            if (healthBar != null)
            {
                healthBar.value = health; // Actualiza el valor del slider

                // Cambiar el color del slider según la vida
                if (health > 50)
                {
                    healthBar.fillRect.GetComponent<Image>().color = Color.green;
                }
                else if (health > 20)
                {
                    healthBar.fillRect.GetComponent<Image>().color = Color.yellow;
                }
                else
                {
                    healthBar.fillRect.GetComponent<Image>().color = Color.red;
                }
            }

            // Cambiar el color a rojo cuando el jugador recibe daño
            StartCoroutine(ChangeColorOnHit());

            if (health <= 0)
            {
                Die();
            }
        }


        // Corutina para cambiar el color temporalmente
        private IEnumerator ChangeColorOnHit()
        {
            spriteRenderer.color = Color.red; // Cambiar el color a rojo
            yield return new WaitForSeconds(0.2f); // Esperar 0.2 segundos
            spriteRenderer.color = originalColor; // Restaurar el color original
        }

        void Die()
        {
            Debug.Log("¡El jugador ha muerto!");
            gameOverPanel.SetActive(true); // Mostrar el panel de Game Over
            Time.timeScale = 0f; // Pausar el juego (opcional)
        }

        // Método para reiniciar el juego (se puede llamar desde el botón)
        public void RestartGame()
        {
            Time.timeScale = 1f; // Reanudar el juego
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Recargar la escena actual
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Monster"))
            {
                // Si el jugador choca con el monstruo, recibe daño
                Monster monster = collision.GetComponent<Monster>();
                if (monster != null)
                {
                    // Supongamos que el monstruo hace 10 puntos de daño al jugador
                    TakeDamage(10);
                }
            }
        }
        

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Monster"))
            {
                Monster monster = collision.gameObject.GetComponent<Monster>();
                if (monster != null)
                {
                    TakeDamage(monsterDamage);
                }
            }
        }
    }
}
