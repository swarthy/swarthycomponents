using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SwarthyComponents.Encoding
{
    public static class LZ78
    {
        const int maxDictSize = 4097;
        /// <summary>
        /// LZ78
        /// </summary>
        /// <param name="sourceFileName">Имя кодируемого файла</param>
        /// <param name="encodedFileName">Имя закодированного файла</param>
        public static void Encode(string sourceFileName, string encodedFileName)
        {
            Dict dictionary = new Dict();

            List<byte> currentWord = new List<byte>();
            int index = 0, prevIndex = 0;

            List<Pair> codes = new List<Pair>();

            BinaryReader reader = new BinaryReader(File.OpenRead(sourceFileName));
            while (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                if (reader.BaseStream.Position % 16 * 1024 == 0 || reader.BaseStream.Position + 1 == reader.BaseStream.Length)
                    Log.Msg("\rEncoding: {0:0.##}%", ((float)(reader.BaseStream.Position + 1) * 100) / reader.BaseStream.Length);
                currentWord.Add(reader.ReadByte());
                prevIndex = index;
                index = dictionary.IndexOf(currentWord);
                if (index == 0)
                {
                    //добавляем в словарь
                    dictionary.Add(currentWord.Clone());
                    if (dictionary.elements.Count == maxDictSize)
                        dictionary = new Dict();
                    codes.Add(new Pair(prevIndex, currentWord.Last()));
                    currentWord.Clear();
                }
            }
            reader.Close();
            bool lastValid = true;
            if (index != 0)
            {
                codes.Add(new Pair(index, 0));
                lastValid = false;
            }
            saveToFile(encodedFileName, codes, lastValid);
            Log.MsgLn();
        }
        /// <summary>
        /// LZ78
        /// </summary>
        /// <param name="encodedFileName">Имя закодированного файла</param>
        /// <param name="decodedFileName">Имя раскодированного файла</param>
        public static void Decode(string encodedFileName, string decodedFileName)
        {
            Dict dictionary = new Dict();
            List<Pair> codes = new List<Pair>();
            bool lastValid = false;
            openFromFile(encodedFileName, ref codes, ref lastValid);
            File.Delete(decodedFileName);
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(decodedFileName));
            for (int i = 0; i < codes.Count; i++)
            {
                if (i % 64 == 0 || i + 1 == codes.Count)
                    Log.Msg("\rDecoding: {0:0.##}%", ((float)(i + 1) * 100) / codes.Count);
                var newElem = dictionary.elements[codes[i].index].Clone();
                newElem.Add(codes[i].value);
                dictionary.Add(newElem);
                bw.Write(dictionary.elements[codes[i].index].ToArray());
                if (i == codes.Count - 1)
                {
                    if (lastValid)
                        bw.Write(codes[i].value);
                }
                else
                    bw.Write(codes[i].value);
                if (dictionary.elements.Count == maxDictSize)
                    dictionary = new Dict();
            }
            bw.Flush();
            bw.Close();
        }

        static void saveToFile(string filename, List<Pair> codes, bool lastValid)
        {
            File.Delete(filename);
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(filename));
            bw.Write(codes.Count);
            foreach (var code in codes)
                code.ToStream(bw);
            bw.Write(lastValid);
            bw.Flush();
            bw.Close();
        }
        static void openFromFile(string filename, ref List<Pair> codes, ref bool lastValid)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(filename));
            int count = br.ReadInt32();
            codes.Clear();
            for (int i = 0; i < count; i++)
                codes.Add(Pair.FromStream(br));
            lastValid = br.ReadBoolean();
            br.Close();
        }
        class Dict
        {
            public List<List<byte>> elements = new List<List<byte>>();
            public Dict()
            {
                elements.Add(new List<byte> { });// 0
            }
            public void Add(List<byte> val)
            {
                elements.Add(val);
                //return new Pair(elements.Count - 1, val);
            }
            public int IndexOf(List<byte> value)
            {
                for (int i = 1; i < elements.Count; i++)
                    if (EqualLists(elements[i], value))
                        return i;
                return 0;
            }
            private bool EqualLists(List<byte> lst1, List<byte> lst2)
            {
                if (lst1.Count != lst2.Count)
                    return false;
                for (int i = 0; i < lst1.Count; i++)
                    if (lst1[i] != lst2[i])
                        return false;
                return true;
            }
            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < elements.Count; i++)
                    sb.AppendFormat("{0}, [{1}]\n", i, String.Join(", ", elements[i]));
                return sb.ToString();
            }
        }
        struct Pair
        {
            public int index;
            public byte value;
            public Pair(int Index, byte Value)
            {
                index = Index;
                value = Value;
            }
            public override string ToString()
            {
                return string.Format("({0}, [{1}])", index, value);
            }
            public void ToStream(BinaryWriter bw)
            {
                bw.Write(index);
                bw.Write(value);
            }
            public static Pair FromStream(BinaryReader br)
            {
                return new Pair(br.ReadInt32(), br.ReadByte());
            }
        }
    }
    static class Ext
    {
        public static List<T> Clone<T>(this List<T> list)
        {
            List<T> result = new List<T>();
            foreach (var x in list)
                result.Add(x);
            return result;
        }
    }
}
