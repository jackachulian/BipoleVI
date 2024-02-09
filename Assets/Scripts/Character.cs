using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System;
using StarterAssets;
using Unity.Collections;

public class Character : NetworkBehaviour {
    ulong _clientID = 0;
    bool _initialized = false;

    [Serializable]
    public struct ItemStack {
        string id;
        string itemID;
        int count;
    }

    [Serializable]
    public struct Data {
        public ItemStack[] inventory;
        public List<string> equippedIds;

        
        public static Data NewEmptyData() {
            return new Data
            {
                inventory = new ItemStack[10],
                equippedIds = new List<string>()
            };
        }

        public string Serialize() {
            return JsonUtility.ToJson(this);
        }

        public static Data Deserialize(string serializedData) {
            return JsonUtility.FromJson<Data>(serializedData);
        }
    }

    private Data _data;
    public Data data => _data;


    public void InitializeServer(ulong clientID, Data data) {
        if (_initialized) return;
        _initialized = true;
        _clientID = clientID;
        _data = data;

    }

    [ClientRpc]
    public void InitializeClientRpc(ulong clientID, string serializedData, ClientRpcParams rpcParams = default) {
        if (_initialized) return;
        _initialized = true;
        _clientID = clientID;
        _data = Data.Deserialize(serializedData);

        Debug.Log("[JOIN] Player id " + clientID + "\n" + serializedData);
    }
}