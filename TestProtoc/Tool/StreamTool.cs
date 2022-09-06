using System.Text;

namespace TestProtoc.Tool
{
    public enum RWType
    {
        Read = 1,
        Write = 2,
    }

    /*
        3. Text 和 Stream 读取，解析方式

            Text
                Write: Data -> string | string 组成 Line，按个写入
                Read: AllLine -> strings -> Datas

            Stream
                Write: Data -> Content中 -> 写入
                Read: Content中 -> Data
                
                优势在于 读取 int, Float，0GC；但是string, 大家都一样；Text 有 AllLine, Stream有Content;  所以一个难度就是掌握 Content的大小，不能太大，也不能太小（容易被某些string 超过）
                劣势，GC方面确实有点优势，但是耗时方面有点拉跨。而且那个 MAX_CACHE_NUM 非常难调节到合理的值  

        分析
            1. Param 类型的参数，也是产生GC的
            2. List 扩充也会增加GC
            3. 泛型中，把 item 转换成对应的类型，是需要经过装箱和拆箱的
     */
    internal class StreamTool : IDisposable
    {
        public const int MAX_CACHE_NUM = 5 * 1024;
        public const byte BREAK_POINT = CONST.ASCII_TABLE;
        public const byte LIST_ITEM_BREAK = CONST.ASCII_COMMA;
        public const byte DIC_KV_BREAK = CONST.ASCII_EQUAL;
        public const byte NO_BREAK = CONST.ASCII_NULL;

        private Stream stream;
        private RWType type;

        public byte[] Content { get; private set; }
        public int CurIdx { get; private set; }
        public int EndIdx { get; private set; }

        public StreamTool(RWType _type, string path)
        {
            string dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);
            
            type = _type;

            Content = new byte[MAX_CACHE_NUM];
            switch (type)
            {
                case RWType.Read:
                    stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read);
                    CurIdx = 0;
                    EndIdx = -1 + stream.Read(Content, CurIdx, MAX_CACHE_NUM);
                    break;
                case RWType.Write:
                    stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
                    CurIdx = 0;
                    EndIdx = -1;
                    break;
            }
        }

        public void Dispose()
        {
            if (stream == null) return;

            stream.Close();
        }

        public void ResetIdx()
        {
            stream.Position = 0;
            switch (type)
            {
                case RWType.Read:
                    CurIdx = 0;
                    EndIdx = -1 + stream.Read(Content, CurIdx, MAX_CACHE_NUM);
                    break;
                case RWType.Write:
                    CurIdx = 0;
                    EndIdx = -1;
                    break;
            }
        }

        public void FlushContent()
        {
            stream.Write(Content, CurIdx, EndIdx - CurIdx + 1);
            stream.Flush();
            CurIdx = 0;
            EndIdx = -1;
        }

        public bool FillContent()
        {
            for (int i = CurIdx; i <= EndIdx; ++i)
            {
                Content[i - CurIdx] = Content[i];
            }

            int newEndIdx = EndIdx - CurIdx;
            int readCount = stream.Read(Content, newEndIdx + 1, MAX_CACHE_NUM - 1 - newEndIdx);
            CurIdx = 0;
            EndIdx = newEndIdx + readCount;
            return readCount > 0;
        }

        public void ReverseList(int si, int ei)
        {
            while (si < ei)
            {
                byte tmp = Content[si];
                Content[si] = Content[ei];
                Content[ei] = tmp;

                si++;
                ei--;
            }
        }


        public bool CheckWrite(int bitCount)
        {
            return EndIdx + 1 + bitCount <= MAX_CACHE_NUM;
        }


        public void WriteMoveNext(int steps)
        {
            EndIdx += steps;
            
            if (EndIdx == MAX_CACHE_NUM)
            {
                FlushContent();
                return;
            }

            if (EndIdx > MAX_CACHE_NUM)
            {
                throw new Exception($"[error][WriteMoveNext]. idx is ouf bound. steps: {steps}; endIdx: {EndIdx}; MAX_CACHE_NUM: {MAX_CACHE_NUM}");
            }

        }

        public void ReadMoveNext(int steps)
        {
            CurIdx += steps;
            bool reachEnd = CurIdx == EndIdx + 1;

            if (reachEnd)
            {
                FillContent();
                return;
            }

            if (CurIdx > EndIdx + 1)
            {
                throw new Exception($"[error][ReadMoveNext]. idx is ouf bound. steps: {steps}; curIdx: {CurIdx}; EndIdx: {EndIdx}");
            }
        }


        public void WriteBreakPoint(byte breakPoint = BREAK_POINT)
        {
            if (!CheckWrite(1)) FlushContent();

            Content[EndIdx + 1] = breakPoint;
            WriteMoveNext(1);
        }

        public void WriteLine()
        {
            WriteBreakPoint(CONST.ASCII_RETURN);
            WriteBreakPoint(CONST.ASCII_NEXLINE);
        }

        public void WriteInt(int value, byte newBreak = BREAK_POINT)
        {
            bool positive = value >= 0;
            if (!positive)
            {
                if (!CheckWrite(1)) FlushContent();
                Content[EndIdx + 1] = CONST.ASCII_NEGATIVE;
                WriteMoveNext(1);
            }

        InsertContent:
            int abValue = Math.Abs(value);
            int startIdx = EndIdx + 1, endIdx = EndIdx;
            do
            {
                byte b = (byte)(abValue % 10 + CONST.ASCII_ZERO);
                abValue /= 10;

                if (endIdx == MAX_CACHE_NUM - 1)
                {
                    FlushContent();
                    goto InsertContent;
                }

                Content[++endIdx] = b;

            } while (abValue > 0);

            ReverseList(startIdx, endIdx);
            WriteMoveNext(endIdx - EndIdx);
            if (newBreak != NO_BREAK) WriteBreakPoint(newBreak);
        }

        public void WriteFloat(float value, byte newBreak = BREAK_POINT)
        {
            int intVa = (int)value;
            float fractionVa = value - (float)intVa;

            WriteInt(intVa, NO_BREAK);
            if (fractionVa == 0) return;

            WriteBreakPoint(CONST.ASCII_POINT);

        InsertContent:
            float fractionVa2 = Math.Abs(fractionVa);
            int endIdx = EndIdx;
            while (fractionVa2 > 0)
            {
                fractionVa2 *= 10;
                int intV = (int)fractionVa2;
                fractionVa2 -= intV;

                if (endIdx == MAX_CACHE_NUM - 1)
                {
                    FlushContent();
                    goto InsertContent;
                }

                Content[++endIdx] = (byte)(intV + CONST.ASCII_ZERO);
            }

            WriteMoveNext(endIdx - EndIdx);
            if (newBreak != NO_BREAK) WriteBreakPoint(newBreak);
        }

        public void WriteString(string value, byte newBreak = BREAK_POINT)
        {
            if (value == null) value = String.Empty;
            if (value.Length > MAX_CACHE_NUM)
            {
                throw new Exception($"[error]. string is so big that the cache is not enough. maxCache: {MAX_CACHE_NUM}");
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            if (!CheckWrite(byteCount)) FlushContent();

            Encoding.UTF8.GetBytes(value, 0, value.Length, Content, EndIdx + 1);
            
            WriteMoveNext(byteCount);
            if (newBreak != NO_BREAK) WriteBreakPoint(newBreak);
        }


        public int GetBreakPoint(int endIdx = -1, byte breakPoint = BREAK_POINT)
        {
            if (endIdx == -1) endIdx = EndIdx;
            for (int i = CurIdx; i <= endIdx; ++i)
            {
                if (Content[i] == breakPoint) return i - 1;
            }

            return -1;
        }

        public int GetLine()
        {
            Check:
            int preReturnIdx = GetBreakPoint(breakPoint : CONST.ASCII_RETURN);
            int preNextLineIdx = GetBreakPoint(breakPoint : CONST.ASCII_NEXLINE);
            bool noLine = preReturnIdx == -1 || preNextLineIdx == -1;
            if (noLine && FillContent()) goto Check;

            if (preReturnIdx != -1 && preNextLineIdx != -1) return preReturnIdx;

            return -1;
        }

        public bool ReadInt(out int value, byte newBreak = BREAK_POINT)
        {
            if (newBreak == NO_BREAK) throw new Exception($"[StreamTool]. newBreak == NO_BREAK");

            value = 0;
        Check:
            int endIdx = GetBreakPoint(breakPoint : newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            int sign = 1;
            if (Content[CurIdx] == CONST.ASCII_NEGATIVE)
            {
                sign = -1;
                ReadMoveNext(1);
            }

            for (int i = endIdx; i >= CurIdx; --i)
            {
                int number = Content[i] - CONST.ASCII_ZERO;
                value += number * (int)Math.Pow(10, endIdx - i);
            }
            value *= sign;

            ReadMoveNext(endIdx + 1 - CurIdx + 1);
            return true;
        }

        public bool ReadFloat(out float value, byte newBreak = BREAK_POINT)
        {
            if (newBreak == NO_BREAK) throw new Exception($"[StreamTool]. newBreak == NO_BREAK");

            value = 0;
        Check:
            int endIdx = GetBreakPoint(breakPoint : newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            int pointEndIdx = GetBreakPoint(endIdx, CONST.ASCII_POINT);
            int intValue = 0;
            bool readInt = false, havePoint = pointEndIdx != -1;

            if (!havePoint) readInt = ReadInt(out intValue, newBreak);
            else readInt = ReadInt(out intValue, CONST.ASCII_POINT);


            if (!readInt) throw new Exception("[ReadFloat]. read int error.");

            value = intValue;
            if (!havePoint) return true;

            int sign = value >= 0 ? 1 : -1;
            for (int i = CurIdx; i <= endIdx; ++i)
            {
                int number = Content[i] - CONST.ASCII_ZERO;
                value += number * (float)Math.Pow(10, pointEndIdx + 1 - i) * sign;
            }

            ReadMoveNext(endIdx + 1 - CurIdx + 1);
            return true;
        }

        public bool ReadString(out string value, byte newBreak = BREAK_POINT)
        {
            if (newBreak == NO_BREAK) throw new Exception($"[StreamTool]. newBreak == NO_BREAK");

            value = null;
        Check:
            int endIdx = GetBreakPoint(breakPoint : newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;
            
            value = UnicodeEncoding.UTF8.GetString(Content, CurIdx, endIdx - CurIdx + 1);

            ReadMoveNext(endIdx + 1 - CurIdx + 1);
            return true;
        }


        public void WriteList<T>(List<T> list, byte newBreak = BREAK_POINT)
        {
            Type iType = typeof(T);
            Action<T> writeItem;

            if (iType == typeof(int)) writeItem = t => WriteInt(Convert.ToInt32(t), LIST_ITEM_BREAK);
            else if (iType == typeof(float)) writeItem = t => WriteFloat(Convert.ToSingle(t), LIST_ITEM_BREAK);
            else if (iType == typeof(string)) writeItem = t => WriteString(Convert.ToString(t), LIST_ITEM_BREAK);
            else throw new Exception($"this type is not supported. iType: {iType}");

            foreach (T t in list) writeItem(t);

            if (newBreak != NO_BREAK) WriteBreakPoint(newBreak);
        }

        public void WriteDictionary<K, V>(Dictionary<K, V> dic, byte newBreak = BREAK_POINT)
        {
            Type kType = typeof(K);
            Type vType = typeof(V);
            Type intType = typeof(int);
            Type floatType = typeof(float);
            Type stringType = typeof(string);

            Action<K> writeKey;
            Action<V> writeValue;

            if (kType == intType) writeKey = t => WriteInt(Convert.ToInt32(t), NO_BREAK);
            else if (kType == floatType) writeKey = t => WriteFloat(Convert.ToSingle(t), NO_BREAK);
            else if (kType == stringType) writeKey = t => WriteString(Convert.ToString(t), NO_BREAK);
            else throw new Exception($"this type is not supported. kType: {kType}");

            if (vType == intType) writeValue = t => WriteInt(Convert.ToInt32(t), NO_BREAK);
            else if (vType == floatType) writeValue = t => WriteFloat(Convert.ToSingle(t), NO_BREAK);
            else if (vType == stringType) writeValue = t => WriteString(Convert.ToString(t), NO_BREAK);
            else throw new Exception($"this type is not supported. kType: {vType}");

            foreach (var kv in dic)
            {
                writeKey(kv.Key);
                WriteBreakPoint(DIC_KV_BREAK);
                writeValue(kv.Value);
                WriteBreakPoint(LIST_ITEM_BREAK);
            }

            if (newBreak != NO_BREAK) WriteBreakPoint(newBreak);
        }


        public bool ReadList<T>(List<T> value, byte newBreak = BREAK_POINT)
        {
            if (newBreak == NO_BREAK) throw new Exception($"[StreamTool]. newBreak == NO_BREAK");

            if (value == null)
            {
                Console.WriteLine("[error][StreamTool]. list == null");
                return false;
            }

        Check:
            int endIdx = GetBreakPoint(breakPoint : newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            Type iType = typeof(T);
            Func<T> readItem;

            if (iType == typeof(int)) readItem = () => {
                if (!ReadInt(out int intVa, LIST_ITEM_BREAK))
                {
                    throw new Exception($"[error][StreamTool]. read int failed");
                }
                return (T)Convert.ChangeType(intVa, iType);
            };
            else if (iType == typeof(float)) readItem = () => {
                if (!ReadFloat(out float floatVa, LIST_ITEM_BREAK))
                {
                    throw new Exception($"[error][StreamTool]. read float failed");
                }
                return (T)Convert.ChangeType(floatVa, iType);
            };
            else if (iType == typeof(string)) readItem = () => {
                if (!ReadString(out string stringVa, LIST_ITEM_BREAK))
                {
                    throw new Exception($"[error][StreamTool]. read string failed");
                }
                return (T)Convert.ChangeType(stringVa, iType);
            };
            else throw new Exception($"this type is not supported. iType: {iType}");

            while (CurIdx < endIdx)
            {
                T t = readItem();
                value.Add(t);
            }

            ReadMoveNext(endIdx + 1 - CurIdx + 1);
            return true;
        }

        public bool ReadDic<K, V>(Dictionary<K, V> value, byte newBreak = BREAK_POINT)
        {
            if (newBreak == NO_BREAK) throw new Exception($"[StreamTool]. newBreak == NO_BREAK");

            if (value == null)
            {
                Console.WriteLine("[error][StreamTool]. list == null");
                return false;
            }

            Check:
            int endIdx = GetBreakPoint(breakPoint : newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            Type kType = typeof(K);
            Type vType = typeof(V);
            Type intType = typeof(int);
            Type floatType = typeof(float);
            Type stringType = typeof(string);

            Func<byte, K> readKey;
            Func< byte, V > readValue;

            if (kType == intType) readKey = breakPoint => {
                if (!ReadInt(out int intVa, breakPoint))
                {
                    throw new Exception($"[error][StreamTool]. read int failed");
                }
                return (K)Convert.ChangeType(intVa, kType);
            };
            else if (kType == floatType) readKey = breakPoint => {
                if (!ReadFloat(out float floatVa, breakPoint))
                {
                    throw new Exception($"[error][StreamTool]. read float failed");
                }
                return (K)Convert.ChangeType(floatVa, kType);
            };
            else if (kType == stringType) readKey = breakPoint => {
                if (!ReadString(out string stringVa, breakPoint))
                {
                    throw new Exception($"[error][StreamTool]. read string failed");
                }
                return (K)Convert.ChangeType(stringVa, kType);
            };
            else throw new Exception($"this type is not supported. kType: {kType}");

            if (vType == intType) readValue = breakPoint => {
                if (!ReadInt(out int intVa, breakPoint))
                {
                    throw new Exception($"[error][StreamTool]. read int failed");
                }
                return (V)Convert.ChangeType(intVa, vType);
            };
            else if (vType == floatType) readValue = breakPoint => {
                if (!ReadFloat(out float floatVa, breakPoint))
                {
                    throw new Exception($"[error][StreamTool]. read float failed");
                }
                return (V)Convert.ChangeType(floatVa, vType);
            };
            else if (vType == stringType) readValue = breakPoint => {
                if (!ReadString(out string stringVa, breakPoint))
                {
                    throw new Exception($"[error][StreamTool]. read string failed");
                }
                return (V)Convert.ChangeType(stringVa, vType);
            };
            else throw new Exception($"this type is not supported. kType: {kType}");

            while (CurIdx < endIdx)
            {
                K k = readKey(DIC_KV_BREAK);
                V v = readValue(LIST_ITEM_BREAK);
                if (!value.TryAdd(k, v))
                {
                    Console.WriteLine($"[warning][StreamTool]. same key: {k}; realValue: {value[k]}; curValue: {v}");
                }
            }

            ReadMoveNext(endIdx + 1 - CurIdx + 1);
            return true;
        }

    }
}
