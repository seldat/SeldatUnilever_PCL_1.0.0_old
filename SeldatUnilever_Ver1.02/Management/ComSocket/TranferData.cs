﻿using System;
using System.Threading;

namespace SelDatUnilever_Ver1._00.Management.ComSocket
{
    public class TranferData : RouterComPort
    {
        private UInt32 numResent = 0;
        private const byte ACK = 0;
        private const byte NACK = 1;
        private struct ResPacket
        {
            public UInt16 header;
            public byte command;
            public UInt16 length;
            public byte ack;
            public byte[] data;
            public ResPacket(UInt16 size)
            {
                this.header = 0;
                this.command = 0;
                this.length = 0;
                this.ack = 0;
                this.data = new byte[size];
            }
        };

        public TranferData(String ip, Int32 port)
        {
            //StartClient(ip,port);
            this.Ip = ip;
            this.Port = port;
        }

        protected byte CalChecksum(byte[] data, UInt32 len)
        {
            UInt32 i;
            byte chck_sum = 0;

            for (i = 0; i < len; i++)
            {
                chck_sum += data[i + 2];/* remove header 0xFA55 */
            }
            chck_sum = (byte)(~chck_sum + 1);
            return chck_sum;
        }

        protected bool Tranfer(byte[] dataSend, ref DataReceive dataRec)
        {
            bool flagGetRespone = true;
            bool result = false;

            //this.StartClient(); // open socket
            numResent = 0;
            //Console.WriteLine("len send {0}", dataSend.Length);
            while (true == flagGetRespone)
            {
                numResent++;
                if (numResent >= 100) {
                    return false;
                }
                if (this.rb != null)
                    this.rb.ShowText("try sent socket : " + numResent);
                Console.WriteLine("try sent socket ------------------ {0} ----------------", numResent);
                this.Close();
                if (this.StartClient())// open socket
                { 
                    if (false == SendCMD(dataSend))
                    {
                        this.Close();
                        Thread.Sleep(50);
                        continue;
                    }
                }
                else
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (this.WaitForReadyRead(TIME_OUT_WAIT_RESPONSE))
                {
                    try
                    {
                        //Console.WriteLine("have data receive");
                        DataReceive dataRx = GetDataRec();
                        byte[] data = new byte[dataRx.length];
                        Buffer.BlockCopy(dataRx.data, 0, data, 0, dataRx.length);
                        ResPacket resPaket = new ResPacket();
                        resPaket.header = (UInt16)((data[1] << 8) | data[0]);
                        resPaket.command = data[2];
                        resPaket.length = (UInt16)((data[4] << 8) | data[3]);
                        resPaket.ack = data[5];
                        UInt16 len = (UInt16)(resPaket.length - 4);
                        resPaket.data = new byte[len];
                        Buffer.BlockCopy(data, 6, resPaket.data, 0, len);
                        if (resPaket.header == 0x55FA)
                        {
                            //Console.WriteLine("check header ok");
                            if (resPaket.data[len - 1] == CalChecksum(data, (UInt32)(data.Length - 3)))
                            {
                                //Console.WriteLine("calChecksum ok");
                                if (resPaket.ack == ACK)
                                {
                                    if (resPaket.command == (byte)(dataSend[2] + 1))
                                    {
                                        if (len > 1)
                                        {
                                            dataRec.length = len - 1;
                                            dataRec.data = new byte[dataRec.length];
                                            Buffer.BlockCopy(resPaket.data, 0, dataRec.data, 0, len - 1);
                                        }
                                        result = true;
                                        flagGetRespone = false;
                                        // Console.WriteLine(DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss tt")+"Send data success");
                                        numResent = 0;
                                        this.Close();
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine(e.ToString());
                        this.Close();
                        result = false;
                        flagGetRespone = false;
                    }
                }
                else
                {
                    this.Close();
                    result = false;
                    flagGetRespone = false;
                }
            }
            this.Close();
            return result;
        }
        protected bool Tranfer(byte[] dataSend)
        {
            DataReceive data = new DataReceive();

            bool onTranfer = false;
            try
            {
                onTranfer = Tranfer(dataSend, ref data);
            }
            catch
            {

            }
            return onTranfer;
        }
    }
}