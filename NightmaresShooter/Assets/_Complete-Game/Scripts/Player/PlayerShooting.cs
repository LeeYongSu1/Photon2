using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnitySampleAssets.CrossPlatformInput;

namespace CompleteProject
{
    public class PlayerShooting : MonoBehaviourPun,IPunObservable
    {
        public int damagePerShot = 20;                  // The damage inflicted by each bullet.
        public float timeBetweenBullets = 0.15f;        // The time between each shot.
        public float range = 100f;                      // The distance the gun can fire.


        float timer;                                    // A timer to determine when to fire.
        Ray shootRay = new Ray();                       // A ray from the gun end forwards.
        RaycastHit shootHit;                            // A raycast hit to get information about what was hit.
        int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.
        ParticleSystem gunParticles;                    // Reference to the particle system.
        LineRenderer gunLine;                           // Reference to the line renderer.
        AudioSource gunAudio;                           // Reference to the audio source.
        Light gunLight;                                 // Reference to the light component.
		public Light faceLight;								// Duh
        float effectsDisplayTime = 0.2f;                // The proportion of the timeBetweenBullets that the effects will display for.


        void Awake ()
        {
            // Create a layer mask for the Shootable layer.
            shootableMask = LayerMask.GetMask ("Shootable");

            // Set up the references.
            gunParticles = GetComponent<ParticleSystem> ();
            gunLine = GetComponent <LineRenderer> ();
            gunAudio = GetComponent<AudioSource> ();
            gunLight = GetComponent<Light> ();
			faceLight = GetComponentInChildren<Light> ();
        }


        void Update ()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            // Add the time since Update was last called to the timer.
            timer += Time.deltaTime;

            // If the Fire1 button is being press and it's time to fire...
			if(Input.GetButton ("Fire1") && timer >= timeBetweenBullets && Time.timeScale != 0)
            {
                // ... shoot the gun.
                Shoot ();
            }

        }


        public void DisableEffects ()
        {
            // Disable the line renderer and the light.
            gunLine.enabled = false;
			faceLight.enabled = false;
            gunLight.enabled = false;
        }


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // 로컬 오브젝트라면 쓰기 부분이 실행됨
            //if (stream.IsWriting)
            //{
            //    // 남은 탄약수를 네트워크를 통해 보내기
            //    stream.SendNext(ammoRemain);
            //    // 탄창의 탄약수를 네트워크를 통해 보내기
            //    stream.SendNext(magAmmo);
            //    // 현재 총의 상태를 네트워크를 통해 보내기
            //    stream.SendNext(state);
            //}
            //else
            //{
            //    // 리모트 오브젝트라면 읽기 부분이 실행됨
            //    // 남은 탄약수를 네트워크를 통해 받기
            //    ammoRemain = (int)stream.ReceiveNext();
            //    // 탄창의 탄약수를 네트워크를 통해 받기
            //    magAmmo = (int)stream.ReceiveNext();
            //    // 현재 총의 상태를 네트워크를 통해 받기
            //    state = (State)stream.ReceiveNext();
            //}
        }

        void Shoot ()
        {
            // 실제 발사 처리는 호스트에게 대리
            photonView.RPC("ShotProcessOnServer", RpcTarget.MasterClient);
            
        }

        [PunRPC]
        private void ShotProcessOnServer()
        {

            // 총알이 맞은 곳을 저장할 변수
            Vector3 hitPosition = Vector3.zero;

            // Reset the timer.
            timer = 0f;

            // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.
            shootRay.origin = transform.position;
            shootRay.direction = transform.forward;

            // Perform the raycast against gameobjects on the shootable layer and if it hits something...
            if (Physics.Raycast(shootRay, out shootHit, range, shootableMask))
            {
                // Try and find an EnemyHealth script on the gameobject hit.
                EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

                // If the EnemyHealth component exist...
                if (enemyHealth != null)
                {
                    // ... the enemy should take damage.
                    enemyHealth.TakeDamage(damagePerShot, shootHit.point);
                }

                // 레이가 충돌한 위치 저장
                hitPosition = shootHit.point;
            }
            // If the raycast didn't hit anything on the shootable layer...
            else
            {
                // ... set the second position of the line renderer to the fullest extent of the gun's range.
                // 총알이 최대 사정거리까지 날아갔을때의 위치를 충돌 위치로 사용
                hitPosition = transform.position +
                              transform.forward * range;
            }

            // 발사 이펙트 재생, 이펙트 재생은 모든 클라이언트들에서 실행
            photonView.RPC("ShotEffectProcessOnClients", RpcTarget.All, hitPosition);
        }

        // 이펙트 재생 코루틴을 랩핑하는 메서드
        [PunRPC]
        private void ShotEffectProcessOnClients(Vector3 hitPosition)
        {
            StartCoroutine(ShotEffect(hitPosition));
        }

        // 발사 이펙트와 소리를 재생하고 총알 궤적을 그린다
        private IEnumerator ShotEffect(Vector3 hitPosition)
        {
            gunAudio.Play();

            // Enable the lights.
            gunLight.enabled = true;
            faceLight.enabled = true;
            gunParticles.Play();
            // Stop the particles from playing if they were, then start the particles.
            
            

            // Enable the line renderer and set it's first position to be the end of the gun.
           
            gunLine.SetPosition(0, transform.position);
            gunLine.SetPosition(1, hitPosition);
            gunLine.enabled = true;

            yield return new WaitForSeconds(0.03f);

            gunLight.enabled = false;
            faceLight.enabled = false;
            gunLine.enabled = false;
            gunParticles.Stop();
        }
    }
}