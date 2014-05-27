using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SwarthyComponents.Encoding
{
    public static class LZSS
    {
        public static void Encode(string sourceFileName, string encodedFileName, int windowSize = 255, int minCompressCount = 2, int maxCompressCount = 256)
        {
            if (windowSize <= 1)
                throw new Exception("windowSize must be > 1");
            if (minCompressCount < 2)
                throw new Exception("minCompressCount must be > 1");
            if (maxCompressCount < 2)
                throw new Exception("maxCompressCount must be > 1");
            BinaryReader br = new BinaryReader(File.OpenRead(sourceFileName));
            var bytes = br.ReadBytes((int)br.BaseStream.Length);
            br.Close();

            File.Delete(encodedFileName);
            BitWriter bw = new BitWriter(File.OpenWrite(encodedFileName));

            List<byte> search = new List<byte>();
            int prevInd = 0, index = 0;
            for (int i = 0; i < bytes.Length; )
            {
                int left = Math.Max(0, i - windowSize);
                search.Add(bytes[i]);//добавили первый

                int beginIndex = i;
                index = IndexOf(bytes, search, left, i);
                while (index != i && index != -1 && search.Count < maxCompressCount && beginIndex + 1 < bytes.Length)//ищем цепочку в сзади (в окне)
                {
                    prevInd = index;
                    search.Add(bytes[++beginIndex]);
                    index = IndexOf(bytes, search, left, i);
                    if (index == -1 || search.Count >= maxCompressCount)
                        search.RemoveAt(search.Count - 1);
                }

                if (search.Count >= minCompressCount)
                {
                    //сжимаем как (prevIndex, search.Count)
                    bw.Add(true);
                    bw.Add((byte)(prevInd - left));
                    bw.Add((byte)search.Count);
                    i += search.Count;
                }
                else
                {
                    bw.Add(false);
                    bw.Add(bytes[i]);
                    i++;
                    //пишем как обычные байты
                }
                search.Clear();
            }
            bw.WriteData();
            bw.Close();

        }
        public static void Decode(string encodedFileName, string decodedFileName, int windowSize = 255)
        {
            if (windowSize <= 1)
                throw new Exception("windowSize must be > 1");
            BitReader bitr = new BitReader(File.OpenRead(encodedFileName));
            List<byte> result = new List<byte>();
            while (!bitr.EOS)
            {
                if (bitr.ReadBit())
                {
                    //compressed
                    var pos = bitr.ReadByte() + Math.Max(0, result.Count - windowSize);
                    var count = bitr.ReadByte();
                    for (int i = 0; i < count; i++)
                    {
                        result.Add(result[pos + i]);
                    }
                }
                else
                {
                    result.Add(bitr.ReadByte());
                }
            }
            File.Delete(decodedFileName);
            BinaryWriter bwo = new BinaryWriter(File.OpenWrite(decodedFileName));
            foreach (byte b in result)
                bwo.Write(b);
            bwo.Close();
        }

        static int IndexOf(byte[] buffer, List<byte> search, int startIndex = 0, int stopIndex = -1)
        {
            if (search.Count == 0)
                return -1;
            int startInd = startIndex;
            int stopInd = stopIndex == -1 ? buffer.Length : stopIndex;
            while (startInd + search.Count - 1 < stopInd)
            {
                int first = Array.IndexOf<byte>(buffer, search[0], startInd, stopInd - startInd);
                if (first == -1)
                    return -1;
                if (first + search.Count - 1 >= stopInd)
                    return -1;
                bool stop = true;
                for (int i = 1; i < search.Count; i++)
                    if (search[i] != buffer[first + i])
                    {
                        startInd = first + 1;
                        stop = false;
                        break;
                    }
                if (stop)
                    return first;
            }
            return -1;
        }
    }
}
