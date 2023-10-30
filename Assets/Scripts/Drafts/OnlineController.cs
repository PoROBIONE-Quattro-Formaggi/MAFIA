using System.Diagnostics.CodeAnalysis;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Drafts
{
    public class OnlineController : NetworkBehaviour
    {
        [SerializeField] private GameObject objToSpawn;

        private readonly NetworkVariable<int> _someNumber = new(0, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private readonly NetworkVariable<MyCustomData> _someData = new(new MyCustomData
        {
            Int = 1,
            Bool = true,
            String = "String"
        });

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
        private struct MyCustomData : INetworkSerializable
        {
            public int Int;
            public bool Bool;
            public FixedString128Bytes String; // Cannot use normal string because of the memory allocation

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Int);
                serializer.SerializeValue(ref Bool);
                serializer.SerializeValue(ref String);
            }
        }

        public override void OnNetworkSpawn()
        {
            _someNumber.OnValueChanged += HandleValueChange;
            _someData.OnValueChanged += (_, newValue) =>
            {
                Debug.Log($"{newValue.Int}, {newValue.Bool}, {newValue.String}");
            };
        }

        private void HandleValueChange(int previousValue, int newValue)
        {
            Debug.Log(_someNumber.Value);
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }

        public void SpawnObject()
        {
            // Both functions are used for network objects to synchronize between clients
            // Only server can spawn/despawn objects - if client wants to do that use ServerRpc
            var spawnedGameObject = Instantiate(objToSpawn);
            spawnedGameObject.GetComponent<NetworkObject>().Spawn(true);
        }

        public void DespawnObject(GameObject obj)
        {
            // Has to be the NetworkObject
            obj.GetComponent<NetworkObject>().Despawn();
            // OR
            Destroy(obj); // Should work too, because NetworkObject handles despawning on destroy automatically
        }
    }
}