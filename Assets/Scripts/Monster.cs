using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public float speed = 2f; // Velocidad del monstruo
    public int health = 50; // Vida del monstruo
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 movementDirection;
    private float changeDirectionTime = 2f;
    private float timer;

    public event System.Action OnMonsterDestroyed;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // Comenzar con un movimiento aleatorio
        ChangeDirection();
    }

    void Update()
    {
        if (player != null)
        {
            // Si hay un jugador, moverse hacia él
            movementDirection = (player.position - transform.position).normalized;
        }
        else
        {
            // Patrullaje aleatorio
            timer += Time.deltaTime;
            if (timer >= changeDirectionTime)
            {
                ChangeDirection();
            }
        }

        // Activar animación de movimiento
        animator.SetBool("IsMoving", movementDirection.magnitude > 0);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = speed * movementDirection;
    }

    void ChangeDirection()
    {
        // Generar un nuevo movimiento aleatorio
        movementDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        timer = 0f;
    }

    private bool isDead = false;

    public void TakeDamage(int damage)
    {
        if (damage < 0) damage = 0; // Asegurarse de que el daño no sea negativo

        if (isDead) return; // Si ya está muerto, no hacer nada

        health -= damage;

        // Activar animación de impacto
        animator.SetTrigger("IsHit");

        if (health <= 0)
        {
            isDead = true; // Marcar como muerto
            Die();
        }
    }



    void Die()
    {
        Debug.Log("¡El monstruo ha muerto!");

        // Activar animación de muerte
        animator.SetTrigger("Death");

        // Desactivar colisiones y movimiento
        Collider2D monsterCollider = GetComponent<Collider2D>();
        if (monsterCollider != null)
        {
            monsterCollider.enabled = false;
        }

        rb.linearVelocity = Vector2.zero;

        // Destruir el monstruo después de la animación
        StartCoroutine(DestroyAfterAnimation());
    }


    IEnumerator DestroyAfterAnimation()
    {
        // Esperar el tiempo que dure la animación de muerte
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Invocar el evento antes de destruir el objeto
        OnMonsterDestroyed?.Invoke();
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage); // Aplica el daño
            }
            Destroy(collision.gameObject); // Destruir la bala al impactar
        }
    }


}