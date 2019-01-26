using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
        private BluetoothDevice device;
        private ConnectState state = ConnectState.DisConnect;
        private WriteState wstate = WriteState.Success;
        private ReadState rstate = ReadState.Success;
        private String uuid = "00001101-0000-1000-8000-00805F9B34FB";
        private List<BluetoothDevice> devices = new List<BluetoothDevice>();
        private String serverId;
        private List<BluetoothController.Device> devList;

        private int lastBufLen;


        enum ConnectState
        {
            DisConnect = 0,
            Connected = 1,
            Connecting = 2,            
            Failed = 3
        }

        enum WriteState
        {
            Success = 0,
            WriteFail = 1
        }

        enum ReadState
        {
            Success = 0, 
            WaitReading = 1            
        }
        public static SerialBluetoothController GetInstance()
        {
            return _singleInstance;
        }

        public void InitSerialBluetooth()
        {
            BluetoothAdapter.askEnableBluetooth();
            state = ConnectState.DisConnect;
            BluetoothAdapter.OnConnected -= HandleOnConnected;
            BluetoothAdapter.OnConnected += HandleOnConnected;
            readQueue.Clear();
        }

        void HandleOnConnected(BluetoothDevice obj)
        {
            obj.UUID = uuid;
            state = ConnectState.Connected;
        }

        public void StartServer(String address)
        {
            BluetoothAdapter.OnClientRequest -= HandleOnClientRequest;
            BluetoothAdapter.OnClientRequest += HandleOnClientRequest;//listen to client remote devices trying to connect to your device		            
            BluetoothAdapter.startServer(uuid, 180);
            serverId = ComputeSha256Hash(address);
        }

        private String ComputeSha256Hash(String rawData)
        {
            // Create a SHA256   
            using (SHA256Managed sha256Hash = new SHA256Managed())
            {
                // ComputeHash - returns byte array 
                String data = rawData.ToLower();
                byte[] hash = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));

                // Convert byte array to a string   
                StringBuilder sb = new StringBuilder();
                foreach (Byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString().Substring(0, 4);
            }
        }

        private void HandleOnClientRequest(BluetoothDevice device)
        {
            this.device = device;
            this.device.ReadingCoroutine = ReadingManager;
            this.device.connect();
        }

        private IEnumerator ReadingManager(BluetoothDevice device)
        {//Manage Reading Coroutine

            while (device.IsReading)
            {
                rstate = ReadState.Success;
                byte[] msg = device.read();
                if (msg != null)
                {
                    for (int j = 0; j < msg.Length; j++)
                    {
                        readQueue.Enqueue(msg[j]);
                    }
                }

                yield return null;
            }

            rstate = ReadState.WaitReading;

        }


        public string GetId()
        {
            return serverId;
        }

        public void SearchDevice()
        {
            devices.Clear();
            BluetoothAdapter.OnDeviceDiscovered -= HandleOnDeviceDiscovered;
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

            this.devList = new List<BluetoothController.Device>();
            for (int j = 0; j < devices.Count; j++)
            {
                if (devices[j].MacAddress.Length < 0)
                    continue;
                BluetoothController.Device dev = new BluetoothController.Device();
                String hash = ComputeSha256Hash(devices[j].MacAddress);
                dev.address = hash;
                dev.device = devices[j].Name;
                this.devList.Add(dev);
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("{\"devices\":[");
            for (int i = 0; i < devices.Count; i++)
            {
                builder.Append("{\"device\":\"");
                builder.Append(this.devList[i].device);
                builder.Append("\",");
                builder.Append("\"address\":\"");
                builder.Append(this.devList[i].address);
                builder.Append("\"}");
                if (i + 1 < devices.Count)
                {
                    builder.Append(",");
                }
            }

            builder.Append("]}");
            return builder.ToString();

        }

        public void ConnectById(string address)
        {
            BluetoothDevice conDev = null;
            for (int j = 0; j < devices.Count; j++)
            {
                if (this.devList[j].address.Equals(address))
                {
                    conDev = devices[j];
                    break;
                }
            }

            if (conDev != null)
            {
                this.device = conDev;
                conDev.connect();
                this.device.ReadingCoroutine = ReadingManager;

            }

        }


        public void ConnectByListIndex(int index)
        {
            this.device = devices[index];
            devices[index].connect();
            this.device.ReadingCoroutine = ReadingManager;
        }

        public void Send(byte[] data, int len)
        {
            if (lastBufLen != len)
            {
                device.setBufferSize(len);
                lastBufLen = len;
                bool iswrite = device.send_Blocking(data);
                if (iswrite)
                {
                    wstate = WriteState.Success;
                }
                else
                {
                    wstate = WriteState.WriteFail;
                }
            }
            else
            {
                device.send(data);
                wstate = WriteState.Success;
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

        public int GetWriteState()
        {
            return (int) wstate;
        }

        public int GetReadState()
        {
            return (int) rstate;
        }

        public void DisConnect()
        {
            if (this.device != null)
            {
                this.device.close();
            }

            state = ConnectState.DisConnect;
        }
    }
}