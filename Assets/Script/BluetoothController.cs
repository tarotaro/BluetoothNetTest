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
#endif 
#if UNITY_ANDROID		
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
                //if (frame % 5 == 0)
                {
                    send();
                }
                frame++;
				if (!_isServerStart && state == 1)
				{
					_readTime.text = " " + _javaClass.CallStatic<long>("getReadTime");
					_writeTime.text = " " + _javaClass.CallStatic<long>("getWriteTime");
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
			data[cnt] = (byte)cnt;
		}
		_javaClass.CallStatic("send",data,128);
#endif
	}
	
	private void recv(){
#if UNITY_ANDROID
        byte[] data 
         = _javaClass.CallStatic<byte[]>("recv",256);

        if(data!=null){
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
	   if (_javaClass == null)
	   {
		   _javaClass = new AndroidJavaClass("btlib.xjigen.com.btsocketlib.BtSocketLib");
	   }
	   if (!_isServerStart)
        {
            _isServerStart = true;
            _javaClass.CallStatic("startServer");
        }
#endif
	}

    public void onSearchServer()
    {
#if UNITY_IOS
	    BluetoothiOSInterface._searchDevice();
#endif 
#if UNITY_EDITOR
        Debug.Log("UnityEditorでは使用できません");
#elif UNITY_ANDROID
        if(_javaClass == null){
        _javaClass = new AndroidJavaClass("btlib.xjigen.com.btsocketlib.BtSocketLib");	        
	        _javaClass.CallStatic("searchDevice");
	    }
#endif
    }

    public void onConnect()
    {
#if UNITY_ANDROID
        if (_javaClass != null)
        {
            String address = _devices[_dropdown.value].address;
            _javaClass.CallStatic("connectById",address);
        }
        else
        {
            Debug.Log("サーチなしでは使用できません");
        }
#endif
#if UNITY_IOS	    
#endif	    	    
    }


	public int connectState()
	{
#if UNITY_ANDROID	
		int state = _javaClass.CallStatic<int>("getConnectState");
		return state;
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
	
		String jsonDevices = _javaClass.CallStatic<String>("GetBluetoothIDList");
        _devices = JsonHelper.FromJson<Device>(jsonDevices);
		_dropdown.ClearOptions();
        if (_devices == null || _devices.Length == 0) {
            return;
        }
        List<String> dropdownList =new List<String>();
        for (int i = 0; i < _devices.Length;i++ )
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
		List<String> dropdownList =new List<String>();
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
        if (_javaClass != null)
        {
            _javaClass.CallStatic("disConnect");
        }
#endif
    }

}
