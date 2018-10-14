using System.Runtime.InteropServices;
using UnityEngine;

public class BluetoothiOSInterface {

	[DllImport("__Internal")]
	public static extern void _startServer();
	[DllImport("__Internal")]
	public static extern void _searchDevice();
    
	[DllImport("__Internal")]
	public static extern  string _getBluetoothIDList();
	[DllImport("__Internal")]
	public static extern void _connectById(string address);
	[DllImport("__Internal")]
	public static extern void _connectByListIndex(int index);
	[DllImport("__Internal")]
	public static extern void _send(byte[] data,int len);
	[DllImport("__Internal")]
	public static extern byte[] _recv(int len);
	[DllImport("__Internal")]
	public static extern long _getReadTime();
	[DllImport("__Internal")]
	public static extern long _getWriteTime();
	[DllImport("__Internal")]
	public static extern int _getConnectState();
	[DllImport("__Internal")]
	public static extern void _disConnect();
}
