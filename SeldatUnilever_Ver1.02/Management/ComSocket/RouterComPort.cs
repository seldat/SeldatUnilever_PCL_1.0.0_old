﻿using SeldatMRMS.Management.RobotManagent;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SelDatUnilever_Ver1._00.Management.ComSocket
{
    public class RouterComPort
    {
        // ManualResetEvent instances signal completion.  
        //private ManualResetEvent connectDone = new ManualResetEvent(false);
        //private ManualResetEvent sendDone = new ManualResetEvent(false);
        //private ManualResetEvent receiveDone = new ManualResetEvent(false);
        protected const UInt32 TIME_OUT_WAIT_RESPONSE = 500;
        protected const UInt32 TIME_OUT_WAIT_CONNECT = 1000;

        // The response from the remote device.  
        //private String response = String.Empty;

        public bool flagReadyReadData { get; private set; }
        public bool flagConnected { get; private set; }

        public Socket client = null;
        public String Ip { get; set; }
        public Int32 Port { get; set; }

        protected RobotUnity rb;
        public void setRb(RobotUnity robot)
        {
            this.rb = robot;
        }
        public struct DataReceive
        {
            public int length;
            public byte[] data;
        }
        public class StateObject
        {
            // Client socket.  
            public Socket workSocket = null;
            // Size of receive buffer.  
            public const int BufferSize = 256;
            // Receive buffer.  
            // public byte[] buffer = new byte[BufferSize];
            public DataReceive buffer;

            // Received data string.  
            public StringBuilder sb = new StringBuilder();
            public StateObject()
            {
                buffer.data = new byte[BufferSize];
                buffer.length = 0;
            }
        }

        private StateObject state;


        public RouterComPort()
        {

        }
        private void Connect(EndPoint remoteEP, Socket client)
        {
            if (client != null)
            {
                client.BeginConnect(remoteEP,new AsyncCallback(ConnectCallback), client);

                //connectDone.WaitOne();
            }
            else
            {
                Console.WriteLine("Please create socket\r\n");
            }
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.  '
                if (client.Connected)
                {
                    client.EndConnect(ar);
                    flagConnected = true;
                }
                else {
                    if (this.rb != null)
                        this.rb.ShowText("Lost connect to board");
                    Console.WriteLine("Lost connect to board");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public void Close()
        {
            try
            {
                if (client != null)
                {
                    if (client.Connected)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
        private void Receive(Socket client)
        {
            if (client != null)
            {
                try
                {
                    // Create the state object.  
                    state = new StateObject();
                    state.workSocket = client;

                    // Begin receiving the data from the remote device.  
                    client.BeginReceive(state.buffer.data, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
            else
            {
                Console.WriteLine("Please create socket\r\n");
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                if (state.workSocket.Connected)
                {
                    Socket client = state.workSocket;
                    // Read data from the remote device.  
                    state.buffer.length = client.EndReceive(ar);
                    if (state.buffer.length > 0)
                    {
                        // There might be more data, so store the data received so far.  
                        // state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                        // //  Get the rest of the data.  
                        // client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        //     new AsyncCallback(ReceiveCallback), state);
                        // response = state.sb.ToString();
                        flagReadyReadData = true;
                    }
                }
                else {
                    if (this.rb != null)
                        this.rb.ShowText("Lost connect to board");
                    Console.WriteLine("Lost connect to board");
                }
                //else
                //{
                //    // All the data has arrived; put it in response.  
                //    if (state.sb.Length > 1)
                //    {
                //        response = state.sb.ToString();
                //        FlagReadyReadData = true;
                //    }
                //    // Signal that all bytes have been received.  
                //    receiveDone.Set();
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        protected bool WaitConnected(UInt32 timeOut)
        {
            bool result = true;
            Stopwatch sw = new Stopwatch();
            //Receive(this.client);
            sw.Start();
            while (flagConnected == false)
            {
                if (sw.ElapsedMilliseconds > timeOut)
                {
                    result = false;
                    break;
                }
                Thread.Sleep(50);
            }
            sw.Stop();
            return result;
        }

        protected bool WaitForReadyRead(UInt32 timeOut)
        {
            bool result = true;
            Stopwatch sw = new Stopwatch();
            Receive(client);
            sw.Start();
            while (flagReadyReadData == false)
            {
                if (sw.ElapsedMilliseconds > timeOut)
                {
                    result = false;
                    break;
                }
                Thread.Sleep(50);
            }
            sw.Stop();
            return result;
        }
        protected bool Send(Socket client, byte[] byteData)
        {
            bool ret = false;
            // Begin sending the data to the remote device.  
            if (client != null)
            {
                try
                {
                    client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
                    ret = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    ret = false;
                }
            }
            else
            {
                if (this.rb != null)
                    this.rb.ShowText("SPlease create socket");
                ret = false;
            }
            return ret;
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                //Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                //sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        protected bool StartClient()
        {
            bool ret = false;
            // Connect to a remote device.  
            flagConnected = false;
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the   
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(this.Ip), this.Port);
                // Create a TCP/IP socket.  
                client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the remote endpoint.  
                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);

                ret = this.WaitConnected(TIME_OUT_WAIT_CONNECT);

                if (ret == false)
                {
                    if (this.rb != null)
                       // this.rb.ShowText("Connnect fail______---------------------------(-_-)---------------------------_______");
                    Console.WriteLine("Connnect fail______---------------------------(-_-)---------------------------_______");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return ret;
        }
        protected bool SendCMD(byte[] bData)
        {
            return Send(client, bData);
        }

        protected bool SendString(String sData)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(sData);
            return Send(client, byteData);
        }

        protected DataReceive GetDataRec()
        {
            DataReceive data = state.buffer;
            flagReadyReadData = false;
            return data;
        }
        public virtual void CheckAlive() { }
    }
}