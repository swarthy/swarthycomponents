using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SwarthyComponents.Encoding
{
    public static class Arithmetic
    {
        const int BUFFER_SIZE = sizeof(ulong);        
        static ulong left, right, number, textSize;
        static long pos;
        static List<Symbol> startFreq;
        static List<byte> stream = new List<byte>(), decodedFile = new List<byte>();
        /// <summary>
        /// Арифметическое кодирование
        /// </summary>
        /// <param name="sourceFileName">Имя кодируемого файла</param>
        /// <param name="encodedFileName">Имя закодированного файла</param>
        public static void Encode(string sourceFileName, string encodedFileName)
        {
            BinaryReader inp = new BinaryReader(File.OpenRead(sourceFileName));
            List<byte> text = new List<byte>();

            Log.MsgLn("Reading file...");
            while (inp.BaseStream.Position != inp.BaseStream.Length)
                text.Add(inp.ReadByte());            
            long count = inp.BaseStream.Length;            

            Log.MsgLn("Frequency counting...");
            startFreq = CountFreq(text, count);
            Log.MsgLn("Frequency sorting...");
            startFreq.Sort((x, y) => { return y.probability.CompareTo(x.probability); });

            Decimal sum = 0;
            startFreq.ForEach(s =>
            {
                sum += s.probability;
                s.probability = sum;
            });

            Log.MsgLn("Frequency writing..");
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(encodedFileName));
            writeFreq(bw, startFreq);
            bw.Write(inp.BaseStream.Length);

            left = 0;
            right = ulong.MaxValue;

            long j = 0;
            foreach (var strByte in text)
            {
                if (++j % 4096 == 0 || j == text.Count)
                    Log.Msg("\rEncoding: {0:0.##}%", 100 * j / count);
                var seg = getSegment(strByte);
                left = seg[0];
                right = seg[1];

                number = left + (right - left) / 2;

                //Log.MsgLn("encode: [{0}, {1}] ({2}) num: {3}", left, right, strByte, ulongToString(number));
                check(bw);
            }
            savePartOfNumber(bw, BitConverter.GetBytes(number).ToList());
            inp.Close();
            bw.Flush();
            bw.Close();
            Log.MsgLn();
        }
        /// <summary>
        /// Арифметическое кодирование
        /// </summary>
        /// <param name="encodedFileName">Имя закодированного файла</param>
        /// <param name="decodedFileName">Имя раскодированного файла</param>
        public static void Decode(string encodedFileName, string decodedFileName)
        {
            left = 0;
            right = ulong.MaxValue;
            
            BinaryReader br = new BinaryReader(File.OpenRead(encodedFileName));
            BinaryWriter bwd = new BinaryWriter(File.OpenWrite(decodedFileName));
            Log.MsgLn("Frequency reading...");
            startFreq = readFreq(br);
            textSize = br.ReadUInt64();

            pos = br.BaseStream.Position;
            number = readNumberWind(br, pos);
            for (ulong i = 0; i < textSize; i++)
            {
                if (i % 4096 == 0 || i == textSize - 1)
                    Log.Msg("\rDecoding: {0:0.##}%", 100 * (i + 1) / textSize);
                byte b = 0;
                var seg2 = getSegmentByNum(number, ref b);
                left = seg2[0];
                right = seg2[1];

                //Log.MsgLn("decode: [{0}, {1}] ({2}) num: {3}", left, right, b, ulongToString(number));
                bwd.Write(b);
                //decodedFile.Add(b);
                check2(br);
            }
            br.Close();
            bwd.Flush();
            bwd.Close();
            Log.MsgLn();
        }
        static void check(BinaryWriter bw)
        {
            var l = BitConverter.GetBytes(left);
            var r = BitConverter.GetBytes(right);
            int moveCount = 0;
            for (int i = l.Length - 1; i >= 0; i--)
            {
                if (l[i] == r[i])
                {
                    moveCount++;
                }
                else
                    break;
            }
            if (moveCount != 0)
            {
                savePartOfNumber(bw, BitConverter.GetBytes(number).Skip(BUFFER_SIZE - moveCount).ToList());
                left = move(left, moveCount);
                right = move(right, moveCount);
                number = move(number, moveCount);

                //Log.MsgLn("check.moved {0}; new left: {1}, new right: {2}, new num: {3}", moveCount, left, right, ulongToString(number));
            }
        }
        static void check2(BinaryReader br)
        {
            var l = BitConverter.GetBytes(left);
            var r = BitConverter.GetBytes(right);
            int moveCount = 0;
            for (int i = l.Length - 1; i >= 0; i--)
            {
                if (l[i] == r[i])
                {
                    moveCount++;
                }
                else
                    break;
            }
            if (moveCount != 0)
            {
                pos += moveCount;
                number = readNumberWind(br, pos);
                left = move(left, moveCount);
                right = move(right, moveCount);
                //Log.MsgLn("check2.moved {0}; new left: {1}, new right: {2}, new num: {4} [pos={3}]", moveCount, left, right, pos, ulongToString(number));
            }
        }
        static ulong move(ulong num, int count)
        {
            var bytes = BitConverter.GetBytes(num);
            byte[] newnumber = new byte[bytes.Length];
            for (int i = 0; i < bytes.Length - count; i++)
                newnumber[i + count] = bytes[i];
            return BitConverter.ToUInt64(newnumber, 0);
        }
        static void savePartOfNumber(BinaryWriter bw, List<byte> part)
        {
            for (int i = part.Count - 1; i >= 0; i--)
                bw.Write(part[i]);
        }
        static ulong readNumberWind(BinaryReader br, long offset)
        {
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            return BitConverter.ToUInt64(br.ReadBytes(BUFFER_SIZE).Reverse().ToArray(), 0);
            //var v = BitConverter.Toulong32(stream.GetRange(offset, 4).Reverse<byte>().ToArray(), 0);
            //Log.MsgLn("readed: {0}", ulongToString(v));
            //return BitConverter.Toulong32(stream.GetRange((int)offset, 4).Reverse<byte>().ToArray(), 0);
        }
        static ulong[] getSegment(byte val)
        {
            ulong[] res = new ulong[2];

            var delta = right - left;
            for (int i = 0; i < startFreq.Count; i++)
            {
                if (startFreq[i].value == val)
                {
                    res[0] = left + (i == 0 ? 0 : (ulong)(startFreq[i - 1].probability * delta));
                    res[1] = left + (ulong)(startFreq[i].probability * delta);
                }
            }
            return res;
        }
        static ulong[] getSegmentByNum(ulong num, ref byte b)
        {
            ulong[] res = new ulong[2];

            var delta = right - left;

            for (int i = 0; i < startFreq.Count; i++)
            {
                if (left + (ulong)(startFreq[i].probability * delta) > num)
                {
                    res[0] = left + (i == 0 ? 0 : (ulong)(startFreq[i - 1].probability * delta));
                    res[1] = left + (ulong)(startFreq[i].probability * delta);
                    b = startFreq[i].value;
                    break;
                }
            }

            return res;
        }

        static void writeFreq(BinaryWriter writer, List<Symbol> freq)
        {
            writer.Write(freq.Count);
            for (int i = 0; i < freq.Count; i++)
            {
                writer.Write(freq[i].probability);
                writer.Write(freq[i].value);
            }
        }
        static List<Symbol> readFreq(BinaryReader reader)
        {
            List<Symbol> freq = new List<Symbol>();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                freq.Add(new Symbol(reader.ReadDecimal(), reader.ReadByte()));
            }
            return freq;
        }        
        static List<Symbol> CountFreq(List<byte> lst, long count)
        {
            var alphabet = lst.Distinct();
            List<Symbol> freq = new List<Symbol>();
            foreach (var c in alphabet)
                freq.Add(new Symbol(lst.Count(cc => c == cc), c));
            for (int i = 0; i < freq.Count; i++)
                freq[i].probability /= count;
            return freq;
        }
        static string ulongToString(ulong i)
        {
            var bytes = BitConverter.GetBytes(i);
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0}] (", i);
            foreach (byte b in bytes)
                sb.AppendFormat("{0} ", b);
            sb.Append(")");
            return sb.ToString();
        }


        class Symbol
        {
            public Decimal probability;
            public byte value;
            public Symbol(Decimal prob, byte val)
            {
                value = val;
                probability = prob;
            }
            public Symbol Clone()
            {
                return new Symbol(probability, value);
            }
            public override string ToString()
            {
                return string.Format("[{0}] p: {1}", value, probability);
            }
        }
    }
}
