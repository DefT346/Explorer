using Photon.Pun;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public enum GameState { MainMenu, Play }
    public GameState State { get; private set; } = GameState.MainMenu;

    public Camera menuCamera;

    public Transform spawnPoint;

    public void Play()
    {
        menuCamera.gameObject.SetActive(false);
        SpawnPlayer();
    }

    public void Exit()
    {
        menuCamera.gameObject.SetActive(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void SpawnPlayer()
    {
        PhotonNetwork.Instantiate("Player", spawnPoint.position, Quaternion.identity);
    }

    public void SpawnCar(Vector3 position)
    {
        PhotonNetwork.Instantiate("Car", position, Quaternion.identity);
    }

    public void ExitFromApplication()
    {
        Application.Quit();
    }
}
