using System.Text;

namespace TestProtoc.Tool
{
    public enum RWType
    {
        Write = 0,
        Read = 1,
    }

    internal class BinaryStreamTool : IDisposable
    {
        public const int MAX_CACHE_NUM = 1024 * 1024;

        private Stream stream;
        private RWType type;
        public byte[] Content { get; private set; }
        public int CurIdx { get; private set; }
        public int EndIdx { get; private set; }

        public BinaryStreamTool(string path, RWType _type)
        {
            string dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            type = _type;

            switch (type)
            {
                case RWType.Write:
                    Content = new byte[MAX_CACHE_NUM];
                    stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write);
                    CurIdx = 0;
                    EndIdx = -1;
                    break;
                case RWType.Read:
                    Content = new byte[MAX_CACHE_NUM];
                    stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read);
                    CurIdx = 0;
                    EndIdx = CurIdx + stream.Read(Content, CurIdx, MAX_CACHE_NUM);
                    break;
            }
        }

        public void Dispose()
        {
            if (stream == null) return;

            stream.Dispose();
        }

        public void ResetIdx()
        {
            stream.Position = 0;
            CurIdx = 0;
            EndIdx = -1;
        }

        public void FlushContent()
        {
            stream.Write(Content, CurIdx, EndIdx - CurIdx + 1);
            stream.Flush();
            CurIdx = EndIdx = 0;
        }

        private bool FillContent()
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


        private bool CheckWrite(int count)
        {
            return EndIdx + 1 + count <= MAX_CACHE_NUM;
        }

        private bool CheckRead(int readCount)
        {
            return EndIdx - CurIdx + 1 >= readCount;
        }


        private void WriteMoveNext(int steps)
        { 
            int newIdx = EndIdx + steps;
            bool reachEnd = newIdx == MAX_CACHE_NUM;

            if (reachEnd)
            {
                FlushContent();
                return;
            }

            if (newIdx > MAX_CACHE_NUM)
            {
                throw new Exception($"[error][WriteMoveNext]. idx is ouf bound. steps: {steps}; endIdx: {EndIdx}; newIdx: {newIdx}");
            }

            EndIdx = newIdx;
        }

        public void ReadMoveNext(int steps)
        {
            int newIdx = CurIdx + steps;
            bool reachEnd = newIdx == EndIdx + 1;

            if (reachEnd)
            {
                FillContent();
                return;
            }

            if (newIdx > this.EndIdx)
            {
                throw new Exception($"[error][ReadMoveNext]. idx is ouf bound. steps: {steps}; curIdx: {CurIdx}; newIdx: {newIdx}");
            }

            CurIdx = newIdx;
        }


        public void WriteBreakPoint(params byte[] breakPoint)
        {
            if (!CheckWrite(breakPoint.Length)) FlushContent();

            for (int i = 0; i < breakPoint.Length; ++i)
            {
                Content[EndIdx + 1 + i] = breakPoint[i];
            }

            WriteMoveNext(breakPoint.Length);
        }

        public void WriteInt(int value)
        {
            byte[] arry = BitConverter.GetBytes(value);
            if (!CheckWrite(arry.Length)) FlushContent();

            for (int i = 0; i < arry.Length; ++i)
            {
                Content[EndIdx + 1 + i] = arry[i];
            }

            WriteMoveNext(arry.Length);
        }

        public void WriteFloat(float value)
        {
            byte[] arry = BitConverter.GetBytes(value);
            if (!CheckWrite(arry.Length)) FlushContent();

            for (int i = 0; i < arry.Length; ++i)
            {
                Content[EndIdx + 1 + i] = arry[i];
            }

            WriteMoveNext(arry.Length);
        }

        public void WriteString(string value)
        {
            if (value == null) value = string.Empty;
            byte[] arry = Encoding.UTF8.GetBytes(value);
            for (int i = 0; i < arry.Length; ++i)
            {
                if (arry[i] == CONST.ASCII_NULL)
                {
                    throw new Exception($"[error][BinaryStreamTool]. string has breakPoint.str: {value}");
                }
            }

            if (!CheckWrite(arry.Length)) FlushContent();

            for (int i = 0; i < arry.Length; ++i)
            {
                Content[EndIdx + 1 + i] = arry[i];
            }

            WriteMoveNext(arry.Length);
            WriteBreakPoint(CONST.ASCII_NULL);
        }


        public int GetBreakPoint(params byte[] breakPoints)
        {
            int compareIdx = 0;
            for (int i = CurIdx; i <= EndIdx; ++i)
            {
                if (Content[i] == breakPoints[compareIdx]) compareIdx++;
                else compareIdx = 0;

                if (compareIdx == breakPoints.Length) return i - breakPoints.Length;
            }

            return -1;
        }

        public bool TryReadInt(out int value)
        {
            value = 0;
        Check:
            byte bitCount = 4;
            if (!CheckRead(bitCount) && FillContent()) goto Check;

            if (!CheckRead(bitCount)) return false;
            value = BitConverter.ToInt32(Content, CurIdx);
            ReadMoveNext(bitCount);

            return true;
        }

        public bool TryReadFloat(out float value)
        {
            value = 0;
        Check:
            byte bitCount = 4;
            if (!CheckRead(bitCount) && FillContent()) goto Check;

            if (!CheckRead(bitCount)) return false;
            value = BitConverter.ToSingle(Content, CurIdx);
            ReadMoveNext(bitCount);

            return true;
        }

        public bool TryReadString(out string value)
        {
            value = null;
        Check:
            int endIdx = GetBreakPoint(CONST.ASCII_NULL);
            bool noBreak = endIdx == -1;
            if (noBreak && FillContent()) goto Check;

            if (noBreak) return false;

            int strLen = endIdx - CurIdx + 1;
            value = Encoding.UTF8.GetString(Content, CurIdx, strLen);
            ReadMoveNext(strLen + 1);

            return true;
        }


        public void WriteList<T>(List<T> list)
        {
            Type iType = typeof(T);

            Action<Action<T>> Iterater = Action =>
            {
                foreach (T t in list) Action(t);
            };

            if (iType == typeof(int)) Iterater(t => WriteInt(Convert.ToInt32(t)));
            else if (iType == typeof(float)) Iterater(t => WriteFloat(Convert.ToSingle(t)));
            else if (iType == typeof(string)) Iterater(t => WriteString(Convert.ToString(t)));
            else throw new Exception($"this type is not supported. iType: {iType}");

            WriteBreakPoint(CONST.ASCII_TABLE);
        }

        public void WriteDictionary<K, V>(Dictionary<K, V> dic)
        {
            Action<object> itemHandle = item =>
            {
                if (item is int iV) WriteInt(iV);
                else if (item is float fV) WriteFloat(fV);
                else if (item is string sV) WriteString(sV);
                else throw new Exception($"this type is not supported. iType: {item.GetType()}");
            };

            foreach (var kv in dic)
            {
                itemHandle(kv.Key);
                itemHandle(kv.Value);
            }

            WriteBreakPoint(CONST.ASCII_TABLE);
        }


        public bool TryReadList<T>(List<T> value)
        {
            if (value == null)
            {
                Console.WriteLine("[error][StreamTool]. list == null");
                return false;
            }

            byte[] breakPoint = new byte[1] { CONST.ASCII_TABLE };
        Check:
            int endIdx = GetBreakPoint(breakPoint);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            Type iType = typeof(T);
            Action<Func<T>> Iterater = func =>
            {
                while (CurIdx < endIdx)
                {
                    T t = func();
                    if (t.Equals(default(T))) continue;
                    value.Add(t);
                }
            };

            if (iType == typeof(int)) Iterater(() => {
                if (!TryReadInt(out int iV))
                {
                    throw new Exception($"[error][StreamTool]. read int failed");
                }
                return (T)Convert.ChangeType(iV, typeof(T));
            });
            else if (iType == typeof(float)) Iterater(() => {
                if (!TryReadFloat(out float fV))
                {
                    throw new Exception($"[error][StreamTool]. read float failed");
                }
                return (T)Convert.ChangeType(fV, typeof(T));
            });
            else if (iType == typeof(string)) Iterater(() => {
                if (!TryReadString(out string sV))
                {
                    throw new Exception($"[error][StreamTool]. read string failed");
                }
                return (T)Convert.ChangeType(sV, typeof(T));
            });
            else throw new Exception($"this type is not supported. iType: {iType}");

            ReadMoveNext(endIdx - CurIdx + 1 + breakPoint.Length);
            return true;
        }

        public bool TryReadDic<K, V>(Dictionary<K, V> value)
        {
            if (value == null)
            {
                Console.WriteLine("[error][StreamTool]. list == null");
                return false;
            }

            byte[] breakPoint = new byte[1] { CONST.ASCII_TABLE };
        Check:
            int endIdx = GetBreakPoint(breakPoint);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            Func<Type, object> itemHandle = type =>
            {
                if (type == typeof(int))
                {
                    if (!TryReadInt(out int iV))
                    {
                        throw new Exception($"[error][StreamTool]. read int failed");
                    }
                    return Convert.ChangeType(iV, type);
                }
                else if (type == typeof(float))
                {
                    if (!TryReadFloat(out float fV))
                    {
                        throw new Exception($"[error][StreamTool]. read float failed");
                    }
                    return Convert.ChangeType(fV, type);
                }
                else if (type == typeof(string))
                {
                    if (!TryReadString(out string sV))
                    {
                        throw new Exception($"[error][StreamTool]. read string failed");
                    }
                    return Convert.ChangeType(sV, type);
                }
                else throw new Exception($"this type is not supported. iType: {type}");
            };

            while (CurIdx < endIdx)
            {
                itemHandle(typeof(K));
                itemHandle(typeof(V));
            }

            ReadMoveNext(endIdx - CurIdx + 1 + breakPoint.Length);
            return true;
        }
    }
}
