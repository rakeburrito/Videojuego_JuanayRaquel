
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerPlatformerController : MonoBehaviour
{
    [Header("Movimiento y Físicas")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Reaparición")]
    public Transform respawnPoint;
    public float respawnDelay = 1f;

    [Header("Puntuación y Coleccionables")]
    public int totalScore = 0;

    public int coinsCollected = 0;
    public int totalCoins = 55;

    public int diamondsCollected = 0;
    public int totalDiamonds = 15;

    public int keysCollected = 0;
    public int totalKeys = 1;

    [Header("Objetivo del Nivel")]
    public Chest chest;

    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textScore;
    public TextMeshProUGUI textCoins;
    public TextMeshProUGUI textDiamonds;
    public TextMeshProUGUI textKeys;
    public GameObject instructionsPanel;
    public GameObject pausePanel;
    public GameObject settingsPanelInGame;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Efectos de Sonido (SFX)")]
    public AudioSource audioSource;
    public AudioClip jumpSFX;
    public AudioClip collectSFX;
    public AudioClip keySFX;
    public AudioClip chestSFX;
    public AudioClip deathSFX;
    public AudioClip winSFX;

    // Estado interno
    private Rigidbody2D rb;
    private Animator animator;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isDead = false;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Crear punto de aparición automático
        if (respawnPoint == null)
        {
            GameObject autoSpawn = new GameObject("AutoRespawnPoint");
            autoSpawn.transform.position = transform.position;
            respawnPoint = autoSpawn.transform;
        }

        // Pausa inicial
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
        }

        UpdateHUD();
    }

    void Update()
    {
        if (isDead || isRespawning) return;

        if (Mathf.Approximately(Time.timeScale, 0f)) return;

        horizontalInput = Input.GetAxis("Horizontal");

        if (animator != null)
        {
            animator.SetBool("IsRunning", horizontalInput != 0);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            PlaySFX(jumpSFX);
        }

        FlipSprite();
    }

    void FixedUpdate()
    {
        if (isDead || isRespawning ||
            Mathf.Approximately(Time.timeScale, 0f)) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (animator != null)
        {
            animator.SetBool("IsJumping", !isGrounded);
        }

        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead || isRespawning) return;

        // Caída al vacío
        if (other.CompareTag("Boundary"))
        {
            StartCoroutine(RespawnAfterFall());
            return;
        }

        // Monedas
        if (other.CompareTag("Coin"))
        {
            coinsCollected++;
            totalScore += 10;

            PlaySFX(collectSFX);
            Destroy(other.gameObject);

            UpdateHUD();
        }

        // Diamantes
        else if (other.CompareTag("Diamond"))
        {
            diamondsCollected++;
            totalScore += 25;

            PlaySFX(collectSFX);
            Destroy(other.gameObject);

            UpdateHUD();
        }

        // Llaves
        else if (other.CompareTag("Key"))
        {
            keysCollected++;
            totalScore += 50;

            PlaySFX(keySFX != null ? keySFX : collectSFX);
            Destroy(other.gameObject);

            UpdateHUD();
        }

        // Cofre
        else if (other.CompareTag("Chest"))
        {
            if (chest != null && !chest.isOpen)
            {
                int bonus = chest.OpenChest(
                    coinsCollected,
                    diamondsCollected,
                    keysCollected
                );

                if (bonus > 0)
                {
                    totalScore += bonus;
                    PlaySFX(chestSFX);
                    UpdateHUD();
                }
            }
        }

        // Bandera: condición de victoria
        else if (other.CompareTag("WinCondition"))
        {
            TryCompleteLevel();
        }

        // Enemigos y trampas
        else if (other.CompareTag("Enemy") ||
                 other.CompareTag("Hazard"))
        {
            StartCoroutine(RespawnAfterFall());
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead || isRespawning) return;

        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.CompareTag("Hazard"))
        {
            StartCoroutine(RespawnAfterFall());
        }
    }

    // Reiniciar escena cuando el jugador muere
    private IEnumerator RespawnAfterFall()
    {
        if (isRespawning) yield break;

        isRespawning = true;

        horizontalInput = 0f;
        rb.linearVelocity = Vector2.zero;

        if (animator != null)
        {
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsJumping", false);
        }

        PlaySFX(deathSFX);

        yield return new WaitForSecondsRealtime(respawnDelay);

        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Comprobar todos los requisitos para ganar
    private void TryCompleteLevel()
    {
        bool allCoinsCollected =
            coinsCollected >= totalCoins;

        bool allDiamondsCollected =
            diamondsCollected >= totalDiamonds;

        bool allKeysCollected =
            keysCollected >= totalKeys;

        bool chestOpened =
            chest != null && chest.isOpen;

        if (allCoinsCollected &&
            allDiamondsCollected &&
            allKeysCollected &&
            chestOpened)
        {
            CompleteLevel();
        }
        else
        {
            Debug.Log(
                "¡Todavía te faltan objetivos para ganar!"
            );
        }
    }

    // Cerrar instrucciones
    public void CloseInstructions()
    {
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }

    // Pausar juego
    public void TogglePause()
    {
        if (pausePanel != null)
        {
            bool isPaused = pausePanel.activeSelf;

            pausePanel.SetActive(!isPaused);

            Time.timeScale = isPaused ? 1f : 0f;
        }
    }

    // Abrir configuración
    public void OpenSettingsInGame(bool open)
    {
        if (settingsPanelInGame != null)
        {
            settingsPanelInGame.SetActive(open);
        }
    }

    // Muerte manual
    public void Die()
    {
        if (isDead || isRespawning) return;

        StartCoroutine(DieAndRespawn());
    }

    private IEnumerator DieAndRespawn()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        yield return RespawnAfterFall();
    }

    // Completar nivel
    private void CompleteLevel()
    {
        if (isDead) return;

        PlaySFX(winSFX);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        isDead = true;
        Time.timeScale = 0f;
    }

    // Reiniciar nivel desde botón
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    // Volver al menú principal
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    // Actualizar interfaz
    private void UpdateHUD()
    {
        if (textScore != null)
        {
            textScore.text = "Puntaje: " + totalScore;
        }

        if (textCoins != null)
        {
            textCoins.text =
                $"Monedas: {coinsCollected}/{totalCoins}";
        }

        if (textDiamonds != null)
        {
            textDiamonds.text =
                $"Diamantes: {diamondsCollected}/{totalDiamonds}";
        }

        if (textKeys != null)
        {
            textKeys.text =
                $"Llaves: {keysCollected}/{totalKeys}";
        }
    }

    // Girar personaje
    private void FlipSprite()
    {
        if (horizontalInput < 0 && isFacingRight)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );

            isFacingRight = false;
        }
        else if (horizontalInput > 0 && !isFacingRight)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );

            isFacingRight = true;
        }
    }

    // Reproducir sonidos
    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Dibujar comprobación de suelo
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}
