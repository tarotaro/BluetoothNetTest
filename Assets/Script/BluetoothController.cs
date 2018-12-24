using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BluetoothController : MonoBehaviour
{


	[SerializeField] private Dropdown _dropdown;
	[SerializeField] private Text _text;
	[SerializeField] private Text _dataLengthPerSecond;
	[SerializeField] private Text _readTime;
	[SerializeField] private Text _writeTime;
    [SerializeField] private Text _error;
    [SerializeField] private Text _uuID;
	[SerializeField] private Toggle _isAndroidToAndroid;

    public float countdown = 5.0f;
	private float _timePerSecond = 0;
	private int _alldataLength = 0;
    private int _prealldataLength = 0;
    private int frame = 0;
    private bool _isServerStart = false;
	

#if UNITY_ANDROID
	private AndroidJavaClass _javaClass;    
#endif


    [Serializable]
	public class Device
	{
		public String device;
		public String address;		
	}

	private Device[] _devices;
// Use this for initialization
	void Start () {
#if UNITY_ANDROID		
		Serial.SerialBluetoothController.GetInstanc().InitSerialBluetooth();
#endif
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_IOS
         int state = connectState();
		countdown -= Time.deltaTime;
		if (countdown <= 0.0f && state != 1)
		{
			getSearchDevice();
			countdown = 5.0f;
		}

			
		switch (state)
		{
			case 0:
				_text.text = "接続していません";                
				break;
			case 1:
				_text.text = "すでに接続しています";
				break;
			case 2:
				_text.text = "接続中";
				break;
			case 3:
				_text.text = "接続失敗しました";
                _isServerStart = false;
                break;						
		}

		if (_alldataLength > 0 || _prealldataLength> 0)
		{
			state = 1;
		} 
		if (state == 1)
		{
			recv();
			send();
                
			frame++;
			if (!_isServerStart && state == 1)
			{
				_readTime.text = " " + BluetoothiOSInterface._getReadTime();
				_writeTime.text = " " + BluetoothiOSInterface._getWriteTime();
			}
			
		}
		else
		{
			recv();
		}
#endif 
#if UNITY_ANDROID
		if (_isAndroidToAndroid.isOn)
		{
			int state = connectState();
			countdown -= Time.deltaTime;
			if (countdown <= 0.0f && state != 1)
			{
				getSearchDevice();
				countdown = 5.0f;
			}
			
			switch (state)
			{
				case 0:
					_text.text = "接続していません";
					_isServerStart = false;
					break;
				case 1:
					_text.text = "すでに接続しています";
					break;
				case 2:
					_text.text = "接続中";
					break;
				case 3:
					_text.text = "接続失敗しました";
					_isServerStart = false;
					break;

			}

			if (state == 1)
			{
				recv();
				send();

				frame++;			
			}
		}
		else
		{

			if (_javaClass != null)
			{
				int state = connectState();
				countdown -= Time.deltaTime;
				if (countdown <= 0.0f && state != 1)
				{
					getSearchDevice();
					countdown = 5.0f;
				}


				switch (state)
				{
					case 0:
						_text.text = "接続していません";
						_isServerStart = false;
						break;
					case 1:
						_text.text = "すでに接続しています";
						break;
					case 2:
						_text.text = "接続中";
						break;
					case 3:
						_text.text = "接続失敗しました";
						_isServerStart = false;
						break;

				}

				if (state == 1)
				{
					recv();
					send();

					frame++;
					if (!_isServerStart && state == 1)
					{
						_readTime.text = " " + _javaClass.CallStatic<long>("getReadTime");
						_writeTime.text = " " + _javaClass.CallStatic<long>("getWriteTime");
					}
				}

			}
		}
#endif
	}

	private void send()
	{
#if UNITY_ANDROID
		byte[] data = new byte [128];
		for (int cnt = 0; cnt < 128; cnt++)
		{
			data[cnt] = (byte) cnt;
		}
		if (_isAndroidToAndroid.isOn)
		{
			Serial.SerialBluetoothController.GetInstanc().Send(data,128);
		}
		else
		{
			_javaClass.CallStatic("send", data, 128);
		}
#endif
#if UNITY_IOS
		byte[] data = new byte [128];
		for (int cnt = 0; cnt < 128; cnt++)
		{
			data[cnt] = (byte)cnt;
		}
		BluetoothiOSInterface._send(data,128);
#endif
	}

	private void recv()
	{
#if UNITY_ANDROID

		byte[] data = new byte[256];		
		bool isFulledQueue = false;
		if (_isAndroidToAndroid.isOn)
		{
			isFulledQueue = Serial.SerialBluetoothController.GetInstanc().Recv(data, 256);
		}
		else
		{

			isFulledQueue = true;
			data = _javaClass.CallStatic<byte[]>("recv", 256);
		}

		if(data!=null && isFulledQueue){
            _alldataLength += 256;
        }        
        _timePerSecond +=  Time.deltaTime;
		if (_timePerSecond > 1)
		{
            _dataLengthPerSecond.text = ((_prealldataLength + _alldataLength) / (2*(_timePerSecond))).ToString();
            _prealldataLength = _alldataLength;
            _alldataLength = 0;
			_timePerSecond = 0;
		}
#endif
#if UNITY_IOS
		byte [] data = new byte[256];
		bool ret = BluetoothiOSInterface._recv(data,256);

		if (ret)
		{
			_alldataLength += 256;			
		}
		_timePerSecond +=  Time.deltaTime;
		if (_timePerSecond > 1)
		{
			_dataLengthPerSecond.text = ((_prealldataLength + _alldataLength) / (2*(_timePerSecond))).ToString();
			_prealldataLength = _alldataLength;
			_alldataLength = 0;
			_timePerSecond = 0;
		}
#endif
	}

    public void ServerStart()
    {
#if UNITY_ANDROID
	    if (_isAndroidToAndroid.isOn)
	    {
		    if (_javaClass == null)
		    {
			    _javaClass = new AndroidJavaClass("btlib.xjigen.com.btsocketlib.BtSocketLib");
		    }

		    String serverAddress = _javaClass.CallStatic<String>("getBluetoothDeviceAddress");
		    Serial.SerialBluetoothController.GetInstanc().StartServer(serverAddress);
		    _isServerStart = true;
		    _uuID.text = Serial.SerialBluetoothController.GetInstanc().GetId();		    
		    _isAndroidToAndroid.enabled = false;
	    }
	    else
	    {
		    if (_javaClass == null)
		    {
			    _javaClass = new AndroidJavaClass("btlib.xjigen.com.btsocketlib.BtSocketLib");
		    }
		    if (!_isServerStart)
		    {
			    if (_javaClass.CallStatic<Boolean>("isAdvertiseSupported"))
			    {
				    _isServerStart = true;
				    _javaClass.CallStatic("startServer");
				    _uuID.text = _javaClass.CallStatic<String>("getUUIDForName");
				    _isAndroidToAndroid.enabled = false;
			    }
			    else
			    {
				    _error.text = "Hostになれません";
			    }
		    }
	    }
#endif
#if UNITY_IOS
        _isServerStart = true;
        BluetoothiOSInterface._startServer();
        _uuID.text = BluetoothiOSInterface._getId();
#endif
    }

    public void onSearchServer()
    {
#if UNITY_IOS
	    BluetoothiOSInterface._searchDevice();

#elif UNITY_ANDROID
	    if (_isAndroidToAndroid.isOn)
	    {
		    Serial.SerialBluetoothController.GetInstanc().SearchDevice();		    
	    }
	    else 
	    {
		    if (_javaClass == null)
		    {
			    _javaClass = new AndroidJavaClass("btlib.xjigen.com.btsocketlib.BtSocketLib");
		    }

		    _javaClass.CallStatic("searchDevice");
	    }
	    _isAndroidToAndroid.enabled = false;
#endif
    }

    public void onConnect()
    {
#if UNITY_ANDROID
	    if (_isAndroidToAndroid.isOn)
	    {
		    if (_devices.Length != 0)
		    {
			    String address = _devices[_dropdown.value].address;
			    Serial.SerialBluetoothController.GetInstanc().ConnectById(address);
		    }
		    else
		    {
			    Debug.Log("サーチなしでは使用できません");
		    }
	    }
	    else
	    {
		    if (_javaClass != null)
		    {
			    String address = _devices[_dropdown.value].address;
			    _javaClass.CallStatic("connectById", address);
		    }
		    else
		    {
			    Debug.Log("サーチなしでは使用できません");
		    }
	    }
#endif
#if UNITY_IOS
	    String address = _devices[_dropdown.value].address;		    
	    BluetoothiOSInterface._connectById(address);
#endif	    	    
    }


	public int connectState()
	{
#if UNITY_ANDROID
		if (_isAndroidToAndroid.isOn)
		{
			int state = Serial.SerialBluetoothController.GetInstanc().GetConnectState();
			return state;
		}
		else
		{
			int state = _javaClass.CallStatic<int>("getConnectState");
			return state;
		}
#endif
#if UNITY_IOS
		int state = BluetoothiOSInterface._getConnectState();
		return state;
#endif
		return 0;
	}

	public void getSearchDevice()
	{
#if UNITY_ANDROID
		String jsonDevices;

		if (_isAndroidToAndroid.isOn)
		{
			jsonDevices = Serial.SerialBluetoothController.GetInstanc().GetBluetoothIDList();
		}
		else
		{
			jsonDevices = _javaClass.CallStatic<String>("GetBluetoothIDList");
		}

		_devices = JsonHelper.FromJson<Device>(jsonDevices);
		_dropdown.ClearOptions();
		if (_devices == null || _devices.Length == 0)
		{
			return;
		}

		List<String> dropdownList = new List<String>();
		for (int i = 0; i < _devices.Length; i++)
		{
			dropdownList.Add(_devices[i].device + ":" + _devices[i].address);
		}

		_dropdown.AddOptions(dropdownList);
#endif
#if UNITY_IOS
		String jsonDevices = BluetoothiOSInterface._getBluetoothIDList();
		_devices = JsonHelper.FromJson<Device>(jsonDevices);
		_dropdown.ClearOptions();
		if (_devices == null || _devices.Length == 0) {
			return;
		}
		List<String> dropdownList = new List<String>();
		for (int i = 0; i < _devices.Length;i++ )
		{
			dropdownList.Add(_devices[i].device + ":" + _devices[i].address);
		}
		_dropdown.AddOptions(dropdownList);
#endif
	}

	public void Disconnect()
    {
#if UNITY_ANDROID
	    if (_isAndroidToAndroid.isOn)
	    {
		    Serial.SerialBluetoothController.GetInstanc().DisConnect();
		    _isAndroidToAndroid.enabled = true;
	    }
	    else
	    {
		    if (_javaClass != null)
		    {
			    _javaClass.CallStatic("disConnect");
			    _isAndroidToAndroid.enabled = true;
		    }
	    }
#endif
#if UNITY_IOS
	    BluetoothiOSInterface._disConnect();
#endif	    
    }

}
