using System;
using Unity.Netcode;

public interface INetworkVariableAccessor
{
    object GetValueBoxed();
    void SetValueBoxed(object value);
    Type ValueType { get; }
}

public class NetworkVariableAccessor<T> : INetworkVariableAccessor
{
    private readonly NetworkVariable<T> nv;
    public NetworkVariableAccessor(NetworkVariable<T> nv) { this.nv = nv; }

    public object GetValueBoxed() => nv.Value;
    public void SetValueBoxed(object value) => nv.Value = (T)value;
    public Type ValueType => typeof(T);
}