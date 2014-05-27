using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SwarthyComponents.Encoding
{
    public class BitWriter
    {
        List<bool> data = new List<bool>();
        BinaryWriter bw;
        public BitWriter(Stream stream)
        {
            this.bw = new BinaryWriter(stream);
        }
        public void Add(bool bit)
        {
            data.Add(bit);
        }
        public void Add(int intValue)
        {
            BitArray ba = new BitArray(new int[] { intValue });
            foreach (bool b in ba)
                data.Add(b);
        }
        public void Add(byte byteValue)
        {
            BitArray ba = new BitArray(new byte[] { byteValue });
            foreach (bool b in ba)
                data.Add(b);
        }
        public void WriteData()
        {
            byte[] bytes = data.ToByteArray();
            for (int i = 0; i < bytes.Length; i++)
                bw.Write(bytes[i]);
            bw.Write((byte)((8 - (data.Count % 8)) % 8));
            bw.Flush();
        }
        public void Close()
        {
            bw.Close();
        }
    }
    public class BitReader
    {
        List<bool> data = new List<bool>();
        BinaryReader br;
        int position = 0;
        public BitReader(Stream stream)
        {
            this.br = new BinaryReader(stream);
            byte[] bytes = br.ReadBytes((int)br.BaseStream.Length);
            byte notUsing = bytes[bytes.Length - 1];
            Array.Resize(ref bytes, bytes.Length - 1);
            var temp = new BitArray(bytes);
            for (int i = 0; i < temp.Length - notUsing; i++)
                data.Add(temp[i]);
            br.Close();
        }
        public bool EOS
        {
            get
            {
                return position == data.Count;
            }
        }
        public bool ReadBit()
        {
            return data[position++];
        }
        public byte ReadByte()
        {
            byte result = 0;
            for (int i = 0; i < 8; i++, position++)
                if (data[position])
                    result |= (byte)(1 << i);
            return result;
        }
        public int ReadInt()
        {
            int result = 0;
            for (int i = 0; i < 32; i++, position++)
                if (data[position])
                    result |= (int)(1 << i);
            return result;
        }

    }
    static public class exten
    {
        public static byte[] ToByteArray(this List<bool> bits)
        {
            int numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            byte[] bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (int i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }
    }
}
