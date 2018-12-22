using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TechTweaking.Bluetooth;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;

namespace Serial
{
    public sealed class SerialBluetoothController
    {
        
        private static SerialBluetoothController _singleInstance = new SerialBluetoothController();
        Queue<byte> readQueue = new Queue<byte>();
        Queue<byte> writeQueue = new Queue<byte>();
        private BluetoothDevice client;
        private BluetoothDevice server;
        private ConnectState state = ConnectState.DisConnect;
        private String uuid = GUID.Generate().ToString().Substring(0, 4);
        private List<BluetoothDevice> devices = new List<BluetoothDevice>();
        private Thread writeQueueThread;
        
        enum ConnectState
        {
            DisConnect = 0,
            Connected = 1,
            Connecting = 2,
            Failed = 3
        }
        public static SerialBluetoothController GetInstanc()
        {
            return _singleInstance;
        }

        public void InitSerialBluetooth()
        {
            BluetoothAdapter.askEnableBluetooth();
            writeQueueThread = new Thread(WriteQueueThreadWork);
            writeQueueThread.Start();
        }

        public void deInitialSerialBluetooth()
        {
            writeQueueThread.Abort();
            BluetoothAdapter.OnDeviceDiscovered -= HandleOnDeviceDiscovered;
            BluetoothAdapter.OnClientRequest -= HandleOnClientRequest;
                        
        }

        void WriteQueueThreadWork()
        {
            while (true)
            {
                if (writeQueue.Count > 0)
                {
                    int sendedCnt = writeQueue.Count;
                    byte[] sended = new byte [sendedCnt];

                    for (int i = 0; i < sendedCnt;i++)
                    {
                        sended[i] = writeQueue.Dequeue();
                    }

                    if (client != null)
                    {
                        client.send_Blocking(sended);
                    }

                    if (server != null)
                    {
                        server.send_Blocking(sended);
                    } 
                }
                Thread.Sleep(30);
                
            }
        }

        public void StartServer()
        {
            BluetoothAdapter.OnClientRequest += HandleOnClientRequest;//listen to client remote devices trying to connect to your device		            
            BluetoothAdapter.startServer (uuid);            
        }
        
        private void HandleOnClientRequest (BluetoothDevice device)
        {
            this.client = device;
		
            
            this.client.ReadingCoroutine = ReadingManager;

            state = ConnectState.Connected;
            this.client.connect ();
		
        }

        private IEnumerator  ReadingManager (BluetoothDevice device)
        {//Manage Reading Coroutine

            while (device.IsReading) {                
                byte [] msg = device.read ();
                if (msg != null) {
                    for(int j = 0; j< msg.Length;j++){
                        readQueue.Enqueue(msg[j]);
                    }
                }

                yield return null;
            }

        }


        public string GetId()
        {
            return uuid;
        }

        public void SearchDevice()
        {
            devices.Clear();
            BluetoothAdapter.OnDeviceDiscovered += HandleOnDeviceDiscovered;
            BluetoothAdapter.startDiscovery();
        }
        
        void HandleOnDeviceDiscovered(BluetoothDevice dev, short rssi)
        {
            devices.Add(dev);
        }

        public string GetBluetoothIDList()
        {
            Dictionary<String, List<BluetoothController.Device>> pairableDevice = new Dictionary<string, List<BluetoothController.Device>>();
            
            List<BluetoothController.Device> devList = new List<BluetoothController.Device>();
            for (int j = 0; j < devices.Count; j++)
            {
                BluetoothController.Device dev = new BluetoothController.Device();
                dev.address = devices[j].UUID;
                dev.device = devices[j].Name;
                devList.Add(dev);
            }

            pairableDevice.Add("devices",devList);
            return JsonUtility.ToJson(pairableDevice);

        }

        public void ConnectById(string address)
        {
            BluetoothDevice conDev = null;
            for (int j = 0; j < devices.Count; j++)
            {
                if (devices[j].UUID.EndsWith(address))
                {
                    conDev = devices[j];
                    break;
                }
            }

            if (conDev != null)
            {
                this.server = conDev;
                conDev.connect();
            }
            
        }


        public void ConnectByListIndex(int index)
        {
            this.server = devices[index];
            devices[index].connect();
        }

        public void Send(byte[] data, int len)
        {
            for (int j = 0; j < len; j++)
            {
                this.writeQueue.Enqueue(data[j]);
            }
        }

        public bool Recv(byte[] data, int len)
        {
            if (this.readQueue.Count < len)
            {
                return false;
            }

            for (int j = 0; j < len; j++)
            {
                data[j] = readQueue.Dequeue();
            }

            return true;
        }

        public long GetReadTime()
        {
            return 0;
        }

        public long GetWriteTime()
        {
            return 0;
        }

        public int GetConnectState()
        {
            return (int)state;
        }

        public void DisConnect()
        {
            if (this.client != null)
            {
                this.client.close();
            }

            if (this.server != null)
            {
                this.server.close();
            }

            state = ConnectState.DisConnect;
        }
    }
}