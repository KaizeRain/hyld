/****************************************************
    Author:            龙之介
    CreatTime:    #CreateTime#
    Description:     Nothing
*****************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;
using SocketProto;
using Google.Protobuf;

namespace LongZhiJie
{
    public class Message
    {
        private byte[] buffer = new byte[1024];

        private int startindex;

        public byte[] Buffer
        {
            get
            {
                return buffer;
            }
        }

        public int StartIndex
        {
            get
            {
                return startindex;
            }
        }

        public int Remsize
        {
            get
            {
                return buffer.Length - startindex;
            }
        }
        /// <summary>
        /// 解析数据 已处理粘包，半包问题 大小端问题
        /// </summary>
        /// <param name="len"></param>
        /// <param name="HandleRequest"></param>
        public void ReadBuffer(int len, Action<MainPack> HandleResponse)
        {
            startindex += len;
            while (true)
            {
                if (startindex <= 4) return;
                int count = BitConverter.ToInt32(buffer, 0);
                //Logging.HYLDDebug.LogError("消息处理  " + startindex + "  ??>=??  " + (count + 4));
                if (startindex >= (count + 4))
                {
                    //Logging.HYLDDebug.LogError("消息处理  " + startindex + "  >  " + (count + 4));
                    MainPack pack = (MainPack)MainPack.Descriptor.Parser.ParseFrom(buffer, 4, count);
                    //Logging.HYLDDebug.LogError(pack);
                    HandleResponse(pack);
                    Array.Copy(buffer, count + 4, buffer, 0, startindex - count - 4);
                    startindex -= (count + 4);
                }
                else
                {
                    break;
                }
            }
        }

        public static byte[] PackData(MainPack pack)
        {
            byte[] data = pack.ToByteArray();//包体

            byte[] head = BitConverter.GetBytes(data.Length);//包头
            if (!BitConverter.IsLittleEndian)///解决大小端问题
            {
                Console.WriteLine("小端需要翻转");
                head.Reverse();
            }
            return head.Concat(data).ToArray();
        }


        public static Byte[] PackDataUDP(MainPack pack)
        {
            return pack.ToByteArray();
        }
    }
    class ByteArray
    {
        //缓冲区
        public byte[] bytes;
        //读写位置
        public int readIdx = 0;
        public int writeIdx = 0;
        //数据长度
        public int Lenth { get { return writeIdx - readIdx; } }

        public ByteArray(byte[] defaultBetys)
        {
            bytes = defaultBetys;
            readIdx = 0;
            writeIdx = defaultBetys.Length;
        }
    }
}