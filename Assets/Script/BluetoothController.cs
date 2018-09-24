using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BluetoothController : MonoBehaviour
{


	[SerializeField] private Dropdown _dropdown;
	[SerializeField] private Text _text;
	public float countdown = 5.0f;

#if UNITY_ANDROID
	private AndroidJavaClass _javaClass;
    private bool _isServerStart = false;
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
		if (_javaClass != null)
		{
			countdown -= Time.deltaTime;
			if (countdown <= 0.0f)
			{
				getSearchDevice();
				countdown = 5.0f;
			}

			int state = connectState();
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
			
		}
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
    }


	public int connectState()
	{
#if UNITY_ANDROID	
		int state = _javaClass.CallStatic<int>("getConnectState");
		return state;
#endif
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
	}

    public void Disconnect()
    {
#if UNITY_ANDROID
        if (_javaClass != null)
        {
            _javaClass.CallStatic("disConnect");
        }
    }
#endif
}
