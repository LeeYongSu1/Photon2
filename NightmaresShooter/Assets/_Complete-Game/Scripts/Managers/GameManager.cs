using CompleteProject;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ������ ���� ���� ����, ���� UI�� �����ϴ� ���� �Ŵ���
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // �ܺο��� �̱��� ������Ʈ�� �����ö� ����� ������Ƽ
    public static GameManager instance
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (m_instance == null)
            {
                // ������ GameManager ������Ʈ�� ã�� �Ҵ�
                m_instance = FindObjectOfType<GameManager>();
            }

            // �̱��� ������Ʈ�� ��ȯ
            return m_instance;
        }
    }

    private static GameManager m_instance; // �̱����� �Ҵ�� static ����
    public UIManager UI { get { return instance.uiManager; } }
    public GameObject playerPrefab; // ������ �÷��̾� ĳ���� ������
    public UIManager uiManager;
    public Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    private int score = 0; // ���� ���� ����
    
    public bool isGameover { get;  set; } // ���� ���� ����

    // �ֱ������� �ڵ� ����Ǵ�, ����ȭ �޼���
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // ���� ������Ʈ��� ���� �κ��� �����
        if (stream.IsWriting)
        {
            // ��Ʈ��ũ�� ���� score ���� ������
            stream.SendNext(score);
        }
        else
        {
            // ����Ʈ ������Ʈ��� �б� �κ��� �����         

            // ��Ʈ��ũ�� ���� score �� �ޱ�
            score = (int)stream.ReceiveNext();
            // ����ȭ�Ͽ� ���� ������ UI�� ǥ��
            //UIManager.instance.UpdateScoreText(score);
        }
    }

    private void Awake()
    {
        // ���� �̱��� ������Ʈ�� �� �ٸ� GameManager ������Ʈ�� �ִٸ�
        if (instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }
    }

    // ���� ���۰� ���ÿ� �÷��̾ �� ���� ������Ʈ�� ����
    private void Start()
    {
        // ������ ���� ��ġ ����
        Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
        // ��ġ y���� 0���� ����
        randomSpawnPos.y = 0f;

        // ��Ʈ��ũ ���� ��� Ŭ���̾�Ʈ�鿡�� ���� ����
        // ��, �ش� ���� ������Ʈ�� �ֵ�����, ���� �޼��带 ���� ������ Ŭ���̾�Ʈ���� ����
        GameObject go = PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);

        if (go.GetPhotonView().IsMine)
        {
            CameraFollow camera = Camera.main.transform.GetComponent<CameraFollow>();
            camera.target = go.transform;
            camera.offset = new Vector3(0, 15, -22);
        }
    }

    // ������ �߰��ϰ� UI ����
    public void AddScore(int newScore)
    {
        // ���� ������ �ƴ� ���¿����� ���� ���� ����
        if (!isGameover)
        {
            // ���� �߰�
            score += newScore;
            // ���� UI �ؽ�Ʈ ����
            uiManager.UpdateScoreText(score);
        }
    }

    // ���� ���� ó��
    public void EndGame(GameObject player)
    {
        // ���� ���� ���¸� ������ ����
        isGameover = true;
        // ���� ���� UI�� Ȱ��ȭ
        //uiManager.SetActiveGameoverUI();

    }

    // Ű���� �Է��� �����ϰ� ���� ������ ��
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    // ���� ������ �ڵ� ����Ǵ� �޼���
    public override void OnLeftRoom()
    {
        // ���� ������ �κ� ������ ���ư�
        SceneManager.LoadScene("Lobby");
    }
}