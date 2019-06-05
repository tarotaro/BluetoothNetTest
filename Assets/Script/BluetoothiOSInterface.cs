using System.Runtime.InteropServices;
using UnityEngine;
#if UNITY_IOS
public class BluetoothiOSInterface {

	[DllImport("__Internal")]
	public static extern void Bt_startServer();
    [DllImport("__Internal")]
    public static extern string Bt_getUuidForName();
    [DllImport("__Internal")]
	public static extern void Bt_searchDevice();
    
	[DllImport("__Internal")]
	public static extern  string Bt_getBluetoothList();
	[DllImport("__Internal")]
	public static extern void Bt_connectByUuid(string uuid);
	[DllImport("__Internal")]
	public static extern void Bt_connectByListIndex(int index);
	[DllImport("__Internal")]
	public static extern void Bt_send(byte[] data,int len);
	[DllImport("__Internal")]
	public static extern bool Bt_recv(byte[] data,int len);
	[DllImport("__Internal")]
	public static extern long Bt_getReadTime();
	[DllImport("__Internal")]
	public static extern long Bt_getWriteTime();
	[DllImport("__Internal")]
	public static extern int Bt_getConnectState();
	[DllImport("__Internal")]
	public static extern void Bt_disConnect();
}
#endif