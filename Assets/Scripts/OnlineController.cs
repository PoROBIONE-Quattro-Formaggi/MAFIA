using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;

public class OnlineController : NetworkBehaviour
{
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
        public string String;

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
}