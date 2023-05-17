using Photon.Pun;
using System.Collections;
using UnityEngine;

namespace CompleteProject
{
    public class EnemyManager : MonoBehaviourPun, IPunObservable
    {
        public PlayerHealth playerHealth;       // Reference to the player's heatlh.
        public GameObject enemy;                // The enemy prefab to be spawned.
        public float spawnTime = 3f;            // How long between each spawn.
        public Transform[] spawnPoints;         // An array of the spawn points this enemy can spawn from.


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //if (stream.IsWriting)
            //{
            //    // 적의 남은 수를 네트워크를 통해 보내기
            //    stream.SendNext(enemies.Count);
            //    // 현재 웨이브를 네트워크를 통해 보내기
            //    stream.SendNext(wave);
            //}
            //else
            //{
            //    // 리모트 오브젝트라면 읽기 부분이 실행됨
            //    // 적의 남은 수를 네트워크를 통해 받기
            //    enemyCount = (int)stream.ReceiveNext();
            //    // 현재 웨이브를 네트워크를 통해 받기 
            //    wave = (int)stream.ReceiveNext();
            //}
        }

        void Start ()
        {
            // Call the Spawn function after a delay of the spawnTime and then continue to call after the same amount of time.
            InvokeRepeating ("Spawn", spawnTime, spawnTime);
        }


        void Spawn ()
        {
            // 호스트만 적을 직접 생성할 수 있음
            // 다른 클라이언트들은 호스트가 생성한 적을 동기화를 통해 받아옴
            if (PhotonNetwork.IsMasterClient)
            {
                // 게임 오버 상태일때는 생성하지 않음
                if (GameManager.instance != null && GameManager.instance.isGameover)
                {
                    return;
                }
            }

            // Find a random index between zero and one less than the number of spawn points.
            int spawnPointIndex = Random.Range (0, spawnPoints.Length);

            // Create an instance of the enemy prefab at the randomly selected spawn point's position and rotation.
            GameObject createdEnemy = PhotonNetwork.Instantiate(enemy.name, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);

            EnemyHealth enemyHealth = createdEnemy.GetComponent<EnemyHealth>();

            enemyHealth.onDeath += () => StartCoroutine(DestroyAfter(enemyHealth.gameObject, 10f));
            enemyHealth.onDeath += () => GameManager.instance.AddScore(100);
        }


        // 포톤의 Network.Destroy()는 지연 파괴를 지원하지 않으므로 지연 파괴를 직접 구현함
        IEnumerator DestroyAfter(GameObject target, float delay)
        {
            // delay 만큼 쉬고
            yield return new WaitForSeconds(delay);

            // target이 아직 파괴되지 않았다면
            if (target != null)
            {
                // target을 모든 네트워크 상에서 파괴
                PhotonNetwork.Destroy(target);
            }
        }

    }
}