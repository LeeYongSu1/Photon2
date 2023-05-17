using UnityEngine;
using System.Collections;
using Photon.Pun;

namespace CompleteProject
{
    public class EnemyMovement : MonoBehaviour
    {
        public LayerMask whatIsTarget; // 추적 대상 레이어
        PlayerHealth playerHealth;      // Reference to the player's health.
        EnemyHealth enemyHealth;        // Reference to this enemy's health.
        EnemyAttack enemyAttach;
        public UnityEngine.AI.NavMeshAgent nav;               // Reference to the nav mesh agent.

        // 추적할 대상이 존재하는지 알려주는 프로퍼티
        private bool hasTarget
        {
            get
            {
                // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
                if (playerHealth != null && !playerHealth.dead)
                {
                    return true;
                }

                // 그렇지 않다면 false
                return false;
            }
        }

        void Awake ()
        {
            enemyHealth = GetComponent <EnemyHealth> ();
            enemyAttach = GetComponent<EnemyAttack>();
            nav = GetComponent <UnityEngine.AI.NavMeshAgent> ();
        }

        [PunRPC]
        public void Setup(int newHealth, int newDamage, float newSpeed)
        {
            // 체력 설정
            enemyHealth.startingHealth = newHealth;
            enemyHealth.currentHealth = newHealth;
            // 공격력 설정
            enemyAttach.attackDamage = newDamage;
            // 내비메쉬 에이전트의 이동 속도 설정
            nav.speed = newSpeed;
        }

        private void Start()
        {
            // 호스트가 아니라면 AI의 추적 루틴을 실행하지 않음
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }

            // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
            StartCoroutine(UpdatePath());
        }

        void Update ()
        {
            // 호스트가 아니라면 애니메이션의 파라미터를 직접 갱신하지 않음
            // 호스트가 파라미터를 갱신하면 클라이언트들에게 자동으로 전달되기 때문.
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
        }

        // 주기적으로 추적할 대상의 위치를 찾아 경로를 갱신
        private IEnumerator UpdatePath()
        {
            while (!enemyHealth.dead)
            {
                // 추적 대상 없음 : AI 이동 중지
                // 20 유닛의 반지름을 가진 가상의 구를 그렸을때, 구와 겹치는 모든 콜라이더를 가져옴
                // 단, targetLayers에 해당하는 레이어를 가진 콜라이더만 가져오도록 필터링
                GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

                Transform closetPlayer = GetClosestPlayer(players);

                if (closetPlayer == null) yield return null;
               
                // 콜라이더로부터 LivingEntity 컴포넌트 가져오기
                PlayerHealth player = closetPlayer.GetComponent<PlayerHealth>();
                
                // LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아있다면,
                if (player != null && !player.dead)
                {
                    nav.SetDestination(closetPlayer.position);
                    nav.isStopped = false;
                    // 추적 대상을 해당 LivingEntity로 설정
                    playerHealth = player;
                    enemyAttach.playerHealth = playerHealth;
                    
                }
                else
                {
                    enemyAttach.playerInRange = false;
                    nav.isStopped = true;
                }
                
                // 0.25초 주기로 처리 반복
                yield return new WaitForSeconds(0.25f);
            }
        }
        Transform GetClosestPlayer(GameObject[] players)
        {
            Transform closestPlayer = null;
            float closestDistance = Mathf.Infinity;

            for (int i = 0; i < players.Length; i++)
            {
                Transform player = players[i].transform;
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player;
                }
            }

            return closestPlayer;
        }
    }
}