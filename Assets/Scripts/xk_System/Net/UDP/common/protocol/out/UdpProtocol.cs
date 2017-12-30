// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: udp_protocol.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace UdpProtocols {

  /// <summary>Holder for reflection information generated from udp_protocol.proto</summary>
  public static partial class UdpProtocolReflection {

    #region Descriptor
    /// <summary>File descriptor for udp_protocol.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static UdpProtocolReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChJ1ZHBfcHJvdG9jb2wucHJvdG8SDXVkcF9wcm90b2NvbHMiGQoIdWRwU3Rh",
            "dGUSDQoFc3RhdGUYASABKAUiGwoNc2NCcm9hZGNhc3RJUBIKCgJpcBgBIAEo",
            "CSImChJQYWNrYWdlQ2hlY2tSZXN1bHQSEAoIbk9yZGVySWQYAiABKAciIAoJ",
            "SGVhcnRCZWF0EhMKC25TZXJ2ZXJUaW1lGAEgASgHYgZwcm90bzM="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::UdpProtocols.udpState), global::UdpProtocols.udpState.Parser, new[]{ "State" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::UdpProtocols.scBroadcastIP), global::UdpProtocols.scBroadcastIP.Parser, new[]{ "Ip" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::UdpProtocols.PackageCheckResult), global::UdpProtocols.PackageCheckResult.Parser, new[]{ "NOrderId" }, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::UdpProtocols.HeartBeat), global::UdpProtocols.HeartBeat.Parser, new[]{ "NServerTime" }, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  /// <summary>
  ///UDP 模仿TCP 的连接过程
  /// </summary>
  public sealed partial class udpState : pb::IMessage<udpState> {
    private static readonly pb::MessageParser<udpState> _parser = new pb::MessageParser<udpState>(() => new udpState());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<udpState> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::UdpProtocols.UdpProtocolReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public udpState() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public udpState(udpState other) : this() {
      state_ = other.state_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public udpState Clone() {
      return new udpState(this);
    }

    /// <summary>Field number for the "state" field.</summary>
    public const int StateFieldNumber = 1;
    private int state_;
    /// <summary>
    ///1:Accept ,2: Connect, 3： DisConnect
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int State {
      get { return state_; }
      set {
        state_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as udpState);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(udpState other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (State != other.State) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (State != 0) hash ^= State.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (State != 0) {
        output.WriteRawTag(8);
        output.WriteInt32(State);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (State != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(State);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(udpState other) {
      if (other == null) {
        return;
      }
      if (other.State != 0) {
        State = other.State;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            State = input.ReadInt32();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  ///UDP 服务器广播 IP 地址，让客户端都连进来
  /// </summary>
  public sealed partial class scBroadcastIP : pb::IMessage<scBroadcastIP> {
    private static readonly pb::MessageParser<scBroadcastIP> _parser = new pb::MessageParser<scBroadcastIP>(() => new scBroadcastIP());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<scBroadcastIP> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::UdpProtocols.UdpProtocolReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public scBroadcastIP() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public scBroadcastIP(scBroadcastIP other) : this() {
      ip_ = other.ip_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public scBroadcastIP Clone() {
      return new scBroadcastIP(this);
    }

    /// <summary>Field number for the "ip" field.</summary>
    public const int IpFieldNumber = 1;
    private string ip_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Ip {
      get { return ip_; }
      set {
        ip_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as scBroadcastIP);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(scBroadcastIP other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Ip != other.Ip) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Ip.Length != 0) hash ^= Ip.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Ip.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Ip);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Ip.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Ip);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(scBroadcastIP other) {
      if (other == null) {
        return;
      }
      if (other.Ip.Length != 0) {
        Ip = other.Ip;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Ip = input.ReadString();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  ///检查 包的序列，以及是否丢包
  /// </summary>
  public sealed partial class PackageCheckResult : pb::IMessage<PackageCheckResult> {
    private static readonly pb::MessageParser<PackageCheckResult> _parser = new pb::MessageParser<PackageCheckResult>(() => new PackageCheckResult());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<PackageCheckResult> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::UdpProtocols.UdpProtocolReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PackageCheckResult() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PackageCheckResult(PackageCheckResult other) : this() {
      nOrderId_ = other.nOrderId_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public PackageCheckResult Clone() {
      return new PackageCheckResult(this);
    }

    /// <summary>Field number for the "nOrderId" field.</summary>
    public const int NOrderIdFieldNumber = 2;
    private uint nOrderId_;
    /// <summary>
    ///包的序号ID
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint NOrderId {
      get { return nOrderId_; }
      set {
        nOrderId_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as PackageCheckResult);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(PackageCheckResult other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (NOrderId != other.NOrderId) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (NOrderId != 0) hash ^= NOrderId.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (NOrderId != 0) {
        output.WriteRawTag(21);
        output.WriteFixed32(NOrderId);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (NOrderId != 0) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(PackageCheckResult other) {
      if (other == null) {
        return;
      }
      if (other.NOrderId != 0) {
        NOrderId = other.NOrderId;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 21: {
            NOrderId = input.ReadFixed32();
            break;
          }
        }
      }
    }

  }

  /// <summary>
  ///心跳
  /// </summary>
  public sealed partial class HeartBeat : pb::IMessage<HeartBeat> {
    private static readonly pb::MessageParser<HeartBeat> _parser = new pb::MessageParser<HeartBeat>(() => new HeartBeat());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<HeartBeat> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::UdpProtocols.UdpProtocolReflection.Descriptor.MessageTypes[3]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public HeartBeat() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public HeartBeat(HeartBeat other) : this() {
      nServerTime_ = other.nServerTime_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public HeartBeat Clone() {
      return new HeartBeat(this);
    }

    /// <summary>Field number for the "nServerTime" field.</summary>
    public const int NServerTimeFieldNumber = 1;
    private uint nServerTime_;
    /// <summary>
    ///心跳包，每次发送服务器时间
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public uint NServerTime {
      get { return nServerTime_; }
      set {
        nServerTime_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as HeartBeat);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(HeartBeat other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (NServerTime != other.NServerTime) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (NServerTime != 0) hash ^= NServerTime.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (NServerTime != 0) {
        output.WriteRawTag(13);
        output.WriteFixed32(NServerTime);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (NServerTime != 0) {
        size += 1 + 4;
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(HeartBeat other) {
      if (other == null) {
        return;
      }
      if (other.NServerTime != 0) {
        NServerTime = other.NServerTime;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 13: {
            NServerTime = input.ReadFixed32();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
