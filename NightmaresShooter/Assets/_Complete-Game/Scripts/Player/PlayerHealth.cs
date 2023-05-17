using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace CompleteProject
{
    public class PlayerHealth : MonoBehaviourPun
    {
        public int startingHealth = 100;                            // The amount of health the player starts the game with.
        public int currentHealth;                                   // The current health the player has.
        public Slider healthSlider;                                 // Reference to the UI's health bar.
        public Image damageImage;                                   // Reference to an image to flash on the screen on being hurt.
        public AudioClip deathClip;                                 // The audio clip to play when the player dies.
        public float flashSpeed = 5f;                               // The speed the damageImage will fade at.
        public Color flashColour = new Color(1f, 0f, 0f, 0.1f);     // The colour the damageImage is set to, to flash.


        Animator anim;                                              // Reference to the Animator component.
        AudioSource playerAudio;                                    // Reference to the AudioSource component.
        PlayerMovement playerMovement;                              // Reference to the player's movement.
        PlayerShooting playerShooting;                              // Reference to the PlayerShooting script.
        bool isDead;                                                // Whether the player is dead.
        bool damaged;                                               // True when the player gets damaged.

        public bool dead { 
            get 
            {
                return isDead;
            }
            protected set { } } // 사망 상태
        void Awake ()
        {
            // Setting up the references.
            anim = GetComponent <Animator> ();
            playerAudio = GetComponent <AudioSource> ();
            playerMovement = GetComponent <PlayerMovement> ();
            playerShooting = GetComponentInChildren <PlayerShooting> ();

            // Set the initial health of the player.
            currentHealth = startingHealth;
        }

        void OnEnable()
        {
            currentHealth = startingHealth;

            healthSlider = GameManager.instance.uiManager.transform.GetChild(0).GetChild(0).GetComponent<Slider>();
            damageImage = GameManager.instance.uiManager.transform.GetChild(4).GetComponent<Image>();
            // 체력 슬라이더 활성화
            healthSlider.gameObject.SetActive(true);
            // 체력 슬라이더의 최대값을 기본 체력값으로 변경
            healthSlider.maxValue = startingHealth;
            // 체력 슬라이더의 값을 현재 체력값으로 변경
            healthSlider.value = currentHealth;

            // 플레이어 조작을 받는 컴포넌트들 활성화
            playerMovement.enabled = true;
            playerShooting.enabled = true;
        }

        void Update ()
        {
            // If the player has just been damaged...
            if(damaged)
            {
                // ... set the colour of the damageImage to the flash colour.
                damageImage.color = flashColour;
            }
            // Otherwise...
            else
            {
                // ... transition the colour back to clear.
                damageImage.color = Color.Lerp (damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }

            // Reset the damaged flag.
            damaged = false;
        }

        [PunRPC]
        public void ApplyUpdatedHealth(int newHealth, bool newDead)
        {
            currentHealth = newHealth;
            dead = newDead;
        }
        public void TakeDamage()
        {
            damaged = true;
            healthSlider.value = currentHealth;
            playerAudio.Play();
        }

        [PunRPC]
        public void TakeDamageRPC (int amount)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Reduce the current health by the damage amount.
                currentHealth -= amount;

                // 호스트에서 클라이언트로 동기화
                photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, currentHealth, dead);

                // 다른 클라이언트들도 OnDamage를 실행하도록 함
                photonView.RPC("TakeDamageRPC", RpcTarget.Others, amount);

            }
           
            if (currentHealth <= 0 && !isDead)
            {
                // ... it should die.
                Death();
            }
        }


        void Death ()
        {
            // Set the death flag so this function won't be called again.
            isDead = true;

            // Turn off any remaining shooting effects.
            playerShooting.DisableEffects ();

            // Tell the animator that the player is dead.
            anim.SetTrigger ("Die");

            // Set the audiosource to play the death clip and play it (this will stop the hurt sound from playing).
            playerAudio.clip = deathClip;
            playerAudio.Play ();

            // Turn off the movement and shooting scripts.
            playerMovement.enabled = false;
            playerShooting.enabled = false;

            GameManager.instance.EndGame(this.gameObject);

            // 5초 뒤에 리스폰
            Invoke("Respawn", 3f);

        }


        public void RestartLevel ()
        {
            // Reload the level that is currently loaded.
            SceneManager.LoadScene (0);
        }

        // 부활 처리
        public void Respawn()
        {
            // 로컬 플레이어만 직접 위치를 변경 가능
            if (photonView.IsMine)
            {
                // 원점에서 반경 5유닛 내부의 랜덤한 위치 지정
                Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
                // 랜덤 위치의 y값을 0으로 변경
                randomSpawnPos.y = 0f;

                // 지정된 랜덤 위치로 이동
                transform.position = randomSpawnPos;

                GameManager.instance.isGameover = false;
            }
            isDead = false;

            // 컴포넌트들을 리셋하기 위해 게임 오브젝트를 잠시 껐다가 다시 켜기
            // 컴포넌트들의 OnDisable(), OnEnable() 메서드가 실행됨
            gameObject.SetActive(false);
            gameObject.SetActive(true);
        }
    }
}