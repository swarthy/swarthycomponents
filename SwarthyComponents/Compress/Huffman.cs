using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SwarthyComponents.Compress
{
    public static class Huffman
    {
        /// <summary>
        /// Метод Хаффмана
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
            inp.Close();
            Log.MsgLn("Frequency counting...");
            var freq = CountFreq(text);
            Log.MsgLn("Tree building...");
            var root = buildTree(freq);
            Log.MsgLn("Encoding...");
            var en = encode(text, root);
            SaveToFile(encodedFileName, root.ToBits(), en);
        }
        /// <summary>
        /// Метод Хаффмана
        /// </summary>
        /// <param name="encodedFileName">Имя закодированного файла</param>
        /// <param name="decodedFileName">Имя раскодированного файла</param>
        public static void Decode(string encodedFileName, string decodedFileName)
        {
            Log.MsgLn("Reading file...");
            var values = OpenBittext(encodedFileName);
            Tree bRoot = values[0] as Tree;
            List<bool> bText = values[1] as List<bool>;
            Log.MsgLn("Decoding...");
            var de = decode(bText, bRoot);

            File.Delete(decodedFileName);
            BinaryWriter outp = new BinaryWriter(File.OpenWrite(decodedFileName));
            foreach (var p in de)
                outp.Write(p);
            outp.Flush();
            outp.Close();
        }
        static List<Tree> CountFreq(List<byte> lst)
        {
            var alphabet = lst.Distinct();
            List<Tree> freq = new List<Tree>();
            foreach (var c in alphabet)
                freq.Add(new Tree(c, lst.Count(cc => c == cc), null, null));
            return freq;
        }
        static Tree buildTree(List<Tree> symbols)
        {
            //PrintList(symbols);
            while (symbols.Count > 1)
            {
                //сделать качественно тут а не o(n^2)
                symbols.Sort((x, y) =>
                {
                    int freqComp = x.Freq.CompareTo(y.Freq);
                    if (freqComp != 0)
                        return freqComp;
                    else
                        return (x.isLeaf && !y.isLeaf) ? 1 : ((!x.isLeaf && y.isLeaf) ? -1 : 0); //вот так - сначала идет поддерево потом символ
                    //return x.Freq.CompareTo(y.Freq);
                });

                var first = symbols[0];
                var second = symbols[1];
                if (first.isLeaf != second.isLeaf)
                {
                    int i;
                    for (i = 2; i < symbols.Count && symbols[i].isLeaf != first.isLeaf && symbols[i].Freq == second.Freq; i++) ;
                    if (i < symbols.Count && symbols[i].Freq == second.Freq)
                        second = symbols[i];
                }
                symbols.Remove(first);
                symbols.Remove(second);
                symbols.Add(new Tree(byte.MaxValue, first.Freq + second.Freq, first, second));
            }
            FindCodes(symbols[0]);
            return symbols[0];
        }
        static void FindCodes(Tree root, string preCode = "")
        {
            if (root.isLeaf)
            {
                root.CODE = preCode;
                root.CODEbits = BitHelper.BitStringToBits(preCode);
            }
            else
            {
                if (root.Left != null)
                    FindCodes(root.Left, preCode + "0");
                if (root.Right != null)
                    FindCodes(root.Right, preCode + "1");
            }
        }
        static void GetLeaves(Tree root, ref List<Tree> lst)
        {
            if (root.isLeaf)
                lst.Add(root);
            else
            {
                if (root.Left != null)
                    GetLeaves(root.Left, ref lst);
                if (root.Right != null)
                    GetLeaves(root.Right, ref lst);
            }
        }
        static List<bool> encode(List<byte> text, Tree root)
        {
            List<Tree> leaves = new List<Tree>();
            GetLeaves(root, ref leaves);
            List<bool> enc = new List<bool>();
            for (int i = 0; i < text.Count; i++)
                for (int j = 0; j < leaves.Count; j++)
                    if (leaves[j].Symbol == text[i])
                        enc.AddRange(leaves[j].CODEbits);
            return enc;
        }
        static List<byte> decode(List<bool> encodedText, Tree root)
        {
            List<byte> result = new List<byte>();
            Tree current = root;
            int textPos = 0;
            while (textPos < encodedText.Count || current.isLeaf)
            {
                if (current.isLeaf)
                {
                    result.Add(current.Symbol);
                    current = root;
                    continue;
                }
                else
                    if (encodedText[textPos++])
                        current = current.Right;
                    else
                        current = current.Left;
            }
            return result;
        }

        static void SaveToFile(string filename, List<bool> tree, List<bool> encoded)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            var bw = new BitWriter(File.OpenWrite(filename));
            int count = tree.Count + encoded.Count;
            byte notuse = 0;
            int t = count;
            while (t % 8 != 0) { notuse++; t++; }
            bw.Write(notuse);
            bw.WriteBits(tree);
            bw.WriteBits(encoded);
            bw.Flush();
            bw.Close();
        }

        static object[] OpenBittext(string filename)
        {
            var br = new BinaryReader(File.OpenRead(filename));
            int notuse2 = br.ReadByte();
            var bits = br.ReadBits().ToList();
            bits.RemoveRange(bits.Count - notuse2, notuse2);
            int pos = 0;
            var root = Tree.ParseFromBits(bits, ref pos);
            var text = bits.GetRange(pos, bits.Count - pos);
            br.Close();
            return new object[] { root, text };
        }




        class Tree
        {
            public Tree Left, Right;
            public byte Symbol = 0;
            public int Freq;
            public string CODE;
            public bool[] CODEbits;
            public Tree() { }
            public Tree(byte sym, int freq, Tree left, Tree right)
            {
                Symbol = sym;
                Freq = freq;
                Left = left;
                Right = right;
            }
            public bool isLeaf
            {
                get
                {
                    return Left == null && Right == null;
                }
            }
            public override string ToString()
            {
                return string.Format("[{0}] {1} {2}", Symbol, Freq, CODE);
            }
            public int MaxLength
            {
                get
                {
                    if (isLeaf)
                        return 0;
                    int l_len = 0, r_len = 0;
                    if (Left != null)
                        l_len = Left.MaxLength;
                    if (Right != null)
                        r_len = Right.MaxLength;
                    return 1 + (r_len > l_len ? r_len : l_len);
                }
            }
            public List<bool> ToBits()
            {
                List<bool> res = new List<bool>();
                res.Add(Left != null);
                res.Add(Right != null);
                if (isLeaf)
                {
                    res.AddRange(BitHelper.ByteToBits(Symbol));
                    res.AddRange(BitHelper.ByteToBits(Convert.ToByte(CODE.Length)));
                    res.AddRange(BitHelper.BitStringToBits(CODE));
                }
                if (Left != null)
                    res.AddRange(Left.ToBits());
                if (Right != null)
                    res.AddRange(Right.ToBits());
                return res;
            }
            public static Tree ParseFromBits(List<bool> bits, ref int pos)
            {
                Tree elem = new Tree();
                bool hasLeft = bits[pos++];
                bool hasRight = bits[pos++];
                if (!hasLeft && !hasRight) //leaf
                {
                    //read values
                    elem.Symbol = BitHelper.BitsListToByte(bits, ref pos);
                    byte len = BitHelper.BitsListToByte(bits, ref pos);
                    elem.CODE = BitHelper.BitsToBitString(bits, len, ref pos);
                }
                else
                {
                    if (hasLeft)
                        elem.Left = ParseFromBits(bits, ref pos);
                    if (hasRight)
                        elem.Right = ParseFromBits(bits, ref pos);
                }
                return elem;
            }
        }
        
        public static class BitHelper
        {
            public static bool[] ByteToBits(byte source)
            {
                bool[] target = new bool[8];
                for (int i = 0; i < 8; i++)
                    target[i] = ((source >> i) & 1) == 1;
                return target;
            }
            public static bool[] BitStringToBits(string str)
            {
                bool[] target = new bool[str.Length];
                for (int i = 0; i < str.Length; i++)
                    target[i] = str[i].Equals('1');
                return target;
            }


            public static byte BitsListToByte(List<bool> bits, ref int pos)
            {
                byte r = 0;
                for (short i = 0; i < 8; i++)
                    if (bits[pos + i])
                        r |= (byte)(1 << i);
                pos += 8;
                return r;
            }
            public static byte BitsToByte(bool[] bits)
            {
                byte r = 0;
                for (byte i = 0; i < 8; i++)
                    if (bits[i])
                        r |= (byte)(1 << i);
                return r;
            }
            public static string BitsToBitString(List<bool> bits, int len, ref int pos)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < len; i++)
                    sb.Append(bits[pos + i] ? '1' : '0');
                pos += len;
                return sb.ToString();
            }

            public static string BitsToSting(bool[] bits)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < bits.Length; i++)
                    sb.Append(bits[i] ? '1' : '0');
                return sb.ToString();
            }
        }
        public class BitWriter : BinaryWriter
        {
            private bool[] curByte = new bool[8];
            private byte curBitIndx = 0;
            public BitWriter(Stream s) : base(s) { }
            public override void Flush()
            {
                if (curBitIndx > 0)
                    base.Write(BitHelper.BitsToByte(curByte));
                base.Flush();
            }
            public override void Write(bool value)
            {
                curByte[curBitIndx] = value;
                curBitIndx++;

                if (curBitIndx == 8)
                {
                    base.Write(BitHelper.BitsToByte(curByte));
                    this.curBitIndx = 0;
                    this.curByte = new bool[8];
                }
            }
            public void WriteBits(List<bool> bits)
            {
                foreach (var b in bits)
                    Write(b);
            }
        }
    }
    static class StreamExtensions
    {
        public static IEnumerable<bool> ReadBits(this BinaryReader input)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (!input.BaseStream.CanRead) throw new ArgumentException("Cannot read from input", "input");
            return ReadBitsCore(input);
        }

        private static IEnumerable<bool> ReadBitsCore(BinaryReader input)
        {
            int readByte;
            while (input.BaseStream.CanRead && input.BaseStream.Position != input.BaseStream.Length && (readByte = input.ReadByte()) >= 0)
            {
                for (int i = 0; i <= 7; i++)
                    yield return ((readByte >> i) & 1) == 1;
            }
        }
    }
}
