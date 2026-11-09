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

    [Header("Puntuación y Coleccionables")]
    public int totalScore = 0;
    public int typeACollected = 0;
    public int totalTypeA = 10;
    public int typeBCollected = 0;
    public int totalTypeB = 5;

    [Header("Interfaz (UI)")]
    public TextMeshProUGUI textScore;
    public TextMeshProUGUI textCollectiblesA;
    public TextMeshProUGUI textCollectiblesB;
    public GameObject instructionsPanel;
    public GameObject pausePanel;
    public GameObject settingsPanelInGame;
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Efectos de Sonido (SFX)")]
    public AudioSource audioSource;
    public AudioClip jumpSFX;
    public AudioClip collectSFX;
    public AudioClip chestSFX;
    public AudioClip deathSFX;
    public AudioClip winSFX;

    // Estado Interno
    private Rigidbody2D rb;
    private Animator animator;
    private float horizontalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Pausa inicial para el panel de instrucciones
        if (instructionsPanel != null)
        {
            instructionsPanel.SetActive(true);
            Time.timeScale = 0f;
        }

        UpdateHUD();
    }

    void Update()
    {
        if (isDead) return;

        // Si el juego está en pausa, no procesar movimiento ni salto
        if (Time.timeScale == 0f) return;

        horizontalInput = Input.GetAxis("Horizontal");

        // Control de Animaciones
        if (animator != null)
        {
            animator.SetBool("isruning", horizontalInput != 0);
        }

        // Salto
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            PlaySFX(jumpSFX);
        }

        FlipSprite();
    }

    void FixedUpdate()
    {
        if (isDead || Time.timeScale == 0f) return;

        // Comprobación de suelo mediante overlap
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (animator != null)
        {
            animator.SetBool("isjumping", !isGrounded);
        }

        // Aplicar velocidad al Rigidbody2D
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        // Coleccionable Tipo A (ej. Moneda = 10 pts)
        if (other.CompareTag("CollectibleA"))
        {
            typeACollected++;
            totalScore += 10;
            PlaySFX(collectSFX);
            Destroy(other.gameObject);
            UpdateHUD();
        }
        // Coleccionable Tipo B (ej. Gema = 25 pts)
        else if (other.CompareTag("CollectibleB"))
        {
            typeBCollected++;
            totalScore += 25;
            PlaySFX(collectSFX);
            Destroy(other.gameObject);
            UpdateHUD();
        }
        // Característica 1: Cofre interactuable
        else if (other.CompareTag("Chest"))
        {
            Chest chest = other.GetComponent<Chest>();
            if (chest != null && !chest.isOpen)
            {
                int bonus = chest.OpenChest();
                totalScore += bonus;
                PlaySFX(chestSFX);
                UpdateHUD();
            }
        }
        // Condición de Victoria
        else if (other.CompareTag("WinCondition"))
        {
            CompleteLevel();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Característica 2 y Derrota: Enemigos o Trampas
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.CompareTag("Hazard"))
        {
            Die();
        }
    }

    public void CloseInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TogglePause()
    {
        if (pausePanel != null)
        {
            bool isPaused = pausePanel.activeSelf;
            pausePanel.SetActive(!isPaused);
            Time.timeScale = isPaused ? 1f : 0f;
        }
    }

    public void OpenSettingsInGame(bool open)
    {
        if (settingsPanelInGame != null)
        {
            settingsPanelInGame.SetActive(open);
        }
    }

    public void Die()
    {
        isDead = true;
        PlaySFX(deathSFX);
        rb.linearVelocity = Vector2.zero;
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void CompleteLevel()
    {
        PlaySFX(winSFX);
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }

    private void UpdateHUD()
    {
        if (textScore != null) textScore.text = "Puntaje: " + totalScore;
        if (textCollectiblesA != null) textCollectiblesA.text = $"Monedas: {typeACollected}/{totalTypeA}";
        if (textCollectiblesB != null) textCollectiblesB.text = $"Gemas: {typeBCollected}/{totalTypeB}";
    }

    private void FlipSprite()
    {
        if (horizontalInput < 0 && isFacingRight)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            isFacingRight = false;
        }
        else if (horizontalInput > 0 && !isFacingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            isFacingRight = true;
        }
    }

    private void PlaySFX(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}