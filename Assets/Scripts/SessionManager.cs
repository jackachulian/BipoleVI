using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    private static SessionManager _singleton = null;
    public static SessionManager singleton => _singleton;

    public Character playerCharacterPrefab;

    private void Awake() {
        if (_singleton) Destroy(gameObject);
        _singleton = this;
    }

    private Dictionary<ulong, Character> _characters = new Dictionary<ulong, Character>();

    public void StartServer() {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedServer;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectServer;
        NetworkManager.Singleton.StartServer();
    }

    private void OnClientConnectedServer(ulong clientID) {
        ulong[] target = {clientID};
        ClientRpcParams clientRpcParams = default;
        clientRpcParams.Send.TargetClientIds = target;
        OnClientConnectedClientRpc(clientID);
    }

    private void OnClientDisconnectServer(ulong clientID) {
        _characters.Remove(clientID);
    }

    [ClientRpc]
    public void OnClientConnectedClientRpc(ulong clientID) {
        // Todo: pass the account id
        SpawnCharacterServerRpc(clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnCharacterServerRpc(ulong clientID) {
        SpawnCharacter(clientID);
    }

    public void SpawnCharacter(ulong clientID) {
        Vector2 randomCircle = Random.insideUnitCircle*10f;
        Vector3 spawnPosition = new Vector3(randomCircle.x, 1f, randomCircle.y);
        Character character = Instantiate(playerCharacterPrefab, spawnPosition, Quaternion.identity);

        // If online as server,spawn it with ownership on the network
        if (NetworkManager.Singleton.IsServer) character.GetComponent<NetworkObject>().SpawnWithOwnership(clientID);

        // add client to list of connected characters
        _characters.Add(clientID, character);
        
        // spawn the new characcter in with default data (empty inventory)
        Character.Data data = Character.Data.NewEmptyData();
        
        // spawn on this local server
        character.InitializeServer(clientID, data);

        // stop  here if singleplayer, no client mirroring stuff
        if (!NetworkManager.Singleton.IsServer) return;
    
        // spawn for all clients
        string serializedData = data.Serialize();
        character.InitializeClientRpc(clientID, serializedData);

        Debug.Log("[SPAWN] Player id " + clientID + "\n" + serializedData);

        // initialize each other character in the game on the connecting client
        foreach (var other in _characters) {
            var otherID = other.Key;
            var otherCharacter = other.Value;
            if (otherCharacter != character) {
                ulong[] target = {clientID};
                ClientRpcParams clientRpcParams = default;
                clientRpcParams.Send.TargetClientIds = target;
                character.InitializeClientRpc(otherID, otherCharacter.data.Serialize(), clientRpcParams);
            }
        }
    }

    public void StartClient() {
        NetworkManager.Singleton.StartClient();
    }

    public void StartSingleplayer() {
        SpawnCharacter(0);
    }
}
