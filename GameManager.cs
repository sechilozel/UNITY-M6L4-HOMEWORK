using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{

     [SerializeField] List<Transform> spawns = new List<Transform>();
     [SerializeField] List<Transform> spawnsWE = new List<Transform>();
     [SerializeField] List<Transform> spawnsTurret = new List<Transform>();
     [SerializeField] public TMP_Text playersText;
     GameObject[] players;
     List<string> activePlayers = new List<string>();
     int checkPlayers = 0;
     int previousPlayerCount;

     int randomSpawn;
    // Start is called before the first frame update
    void Start()
    {
        randomSpawn = Random.Range(0, spawns.Count);
        PhotonNetwork.Instantiate("Player", spawns[randomSpawn].position,
                                 spawns[randomSpawn].rotation);
        
        Invoke(nameof(SpawnEnemy), 5f);
        previousPlayerCount = PhotonNetwork.PlayerList.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.PlayerList.Length < previousPlayerCount)
        {
            // birileri oyundan çıkmış
            ChangePlayersList();
        }

        previousPlayerCount = PhotonNetwork.PlayerList.Length;
    }

    public void SpawnEnemy()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            for (int i = 0; i < spawnsWE.Count; i++)
            {
                PhotonNetwork.Instantiate("WalkEnemy", spawnsWE[i].position, spawnsWE[i].rotation);
            }
            for (int i = 0; i < spawnsTurret.Count; i++)
            {
                PhotonNetwork.Instantiate("Turret", spawnsTurret[i].position, spawnsTurret[i].rotation);
            }
        }
    }

    [PunRPC]
    public void PlayerList()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        activePlayers.Clear();

        foreach (GameObject player in players)
        {
            if (player.GetComponent<PlayerController>().isDead == false)
            {
                activePlayers.Add(player.GetComponent<PhotonView>().Owner.NickName);
            }
        }

        playersText.text = "PLAYERS ALIVE IN GAME: " + activePlayers.Count.ToString();

        if (activePlayers.Count <= 1 && checkPlayers > 0)
        {
            PlayerPrefs.SetString("Winner", activePlayers[0]);
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("enemy");

            foreach (GameObject enemy in enemies)
            {
                enemy.GetComponent<Enemy>().ChangeHealth(100);
            }
        }

        checkPlayers++;
    }

    public void ChangePlayersList()
    {
        photonView.RPC("PlayerList", RpcTarget.All);
    }

    void EndGame()
    {
        PhotonNetwork.LoadLevel("Lobby");
    }

    public void ExitGame()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(1);      // menüye ışınlar
        ChangePlayersList();
    }
}