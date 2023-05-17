using UnityEngine;
using System.Collections;
using Photon.Pun;

namespace CompleteProject
{
    public class EnemyAttack : MonoBehaviour
    {
        public float timeBetweenAttacks = 0.5f;     
        public int attackDamage = 10;               


        Animator anim;                              
        public PlayerHealth playerHealth;
        public bool playerInRange;   
        EnemyHealth enemyHealth;                    
        float timer;                                


        void Awake ()
        {
            // Setting up the references.
            enemyHealth = GetComponent<EnemyHealth>();
            anim = GetComponent <Animator> ();
        }


        void OnTriggerEnter (Collider other)
        {
            // If the entering collider is the player...
            if(playerHealth != null && other.GetComponent<PlayerHealth>() == playerHealth)
            {
                // ... the player is in range.
                playerInRange = true;
            }
        }


        void OnTriggerExit (Collider other)
        {
            // If the exiting collider is the player...
            if(playerHealth != null && other.GetComponent<PlayerHealth>() == playerHealth)
            {
                // ... the player is no longer in range.
                playerInRange = false;
            }
        }


        void Update ()
        {
            if (playerHealth == null) return;
            // Add the time since Update was last called to the timer.
            timer += Time.deltaTime;

            // If the timer exceeds the time between attacks, the player is in range and this enemy is alive...
            if(timer >= timeBetweenAttacks && playerInRange && enemyHealth.currentHealth > 0)
            {
                // ... attack.
                Attack ();
                playerHealth.TakeDamage();
            }

            // If the player has zero or less health...
            if(playerHealth.currentHealth <= 0)
            {
                // ... tell the animator the player is dead.
                anim.SetTrigger ("PlayerDead");
            }
        }


        void Attack ()
        {
            // 호스트가 아니라면 공격 실행 불가
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            // Reset the timer.
            timer = 0f;

            // If the player has health to lose...
            if(playerHealth.currentHealth > 0)
            {
                // ... damage the player.
                playerHealth.TakeDamageRPC (attackDamage);
            }
        }
    }
}