﻿using System.Text;

namespace TestProtoc.Tool
{
    internal class StreamTool : IDisposable
    {
        public const int MAX_CACHE_NUM = 1024 * 1024;

        private Stream stream;
        private RWType type;
        public byte[] Content { get; private set; }
        public int CurIdx { get; private set; }
        public int EndIdx { get; private set; }

        public StreamTool(RWType _type, string path)
        {
            string dirPath = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            type = _type;

            Content = new byte[MAX_CACHE_NUM];
            switch (type)
            {
                case RWType.Read:
                    stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read);
                    CurIdx = 0;
                    EndIdx = CurIdx + stream.Read(Content, CurIdx, MAX_CACHE_NUM);
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

            if (newIdx > EndIdx)
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

        public void WriteLine()
        {
            byte[] lineBytes = new byte[2] { CONST.ASCII_RETURN, CONST.ASCII_NEXLINE };
            if (!CheckWrite(lineBytes.Length)) FlushContent();

            for (int i = 0; i < lineBytes.Length; ++i)
            {
                Content[EndIdx + 1 + i] = lineBytes[i];
            }

            WriteMoveNext(lineBytes.Length);
        }

        public void WriteInt(int value, byte[] newBreak = null)
        {
            if (newBreak == null) newBreak = new byte[1] { CONST.ASCII_TABLE };

            bool positive = value >= 0;
            if (!positive)
            {
                if (!CheckWrite(1)) FlushContent();
                Content[EndIdx + 1] = (byte)'-';
                WriteMoveNext(1);
            }

            int startIdx = EndIdx + 1;
            value = Math.Abs(value);

            while (value > 0)
            {
                byte b = (byte)(value % 10 + CONST.ASCII_ZERO);
                value /= 10;

                if (EndIdx == MAX_CACHE_NUM)
                {
                    // todo: 这个逻辑式错误的。 只能预先 Flush.
                    // 只能是一开始走走，然后遇到问题，Flush, 重置
                    ReverseList(startIdx, EndIdx);
                    FlushContent();
                    startIdx = EndIdx + 1;
                }

                Content[++EndIdx] = b;
            }

            ReverseList(startIdx, EndIdx);
            WriteBreakPoint(newBreak);
        }

        public void WriteFloat(float value, byte[] newBreak = null)
        {
            if (newBreak == null) newBreak = new byte[1] { CONST.ASCII_TABLE };
            int intVa = (int)value;
            float fractionVa = value - (float)intVa;

            WriteInt(intVa, new byte[0]);
            if (fractionVa == 0) return;

            WriteBreakPoint((byte)'.');

            while (fractionVa > 0)
            {
                fractionVa *= 10;
                int intV = (int)fractionVa;
                fractionVa -= intV;

                if (EndIdx == MAX_CACHE_NUM) FlushContent();

                Content[++EndIdx] = (byte)(intV + CONST.ASCII_ZERO);
            }

            WriteBreakPoint(newBreak);
        }

        public void WriteString(string value, byte[] newBreak = null)
        {
            if (newBreak == null) newBreak = new byte[1] { CONST.ASCII_TABLE };
            if (value == null) value = String.Empty;
            if (value.Length > MAX_CACHE_NUM)
            {
                throw new Exception($"[error]. string is so big that the cache is not enough. maxCache: {MAX_CACHE_NUM}");
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            if (!CheckWrite(byteCount)) FlushContent();

            Encoding.UTF8.GetBytes(value, 0, value.Length, Content, EndIdx + 1);
            
            WriteMoveNext(byteCount);
            WriteBreakPoint(newBreak);
        }


        public int GetBreakPoint(out int byteNum)
        {
            byte[] breakPoint = new byte[1] { CONST.ASCII_TABLE };
            byteNum = 1;
            
            return GetBreakPoint(breakPoint);
        }

        public int GetBreakPoint(byte[] breakPoint)
        {
            int compareIdx = 0;
            for (int i = CurIdx; i <= EndIdx; ++i)
            {
                if (Content[i] == breakPoint[compareIdx]) compareIdx++;
                else compareIdx = 0;

                if (compareIdx == breakPoint.Length) return i - breakPoint.Length;
            }

            return -1;
        }

        public int GetLine(out int lineBreakCount)
        {
            byte[] lineBytes = new byte[2] { CONST.ASCII_RETURN, CONST.ASCII_NEXLINE };
            lineBreakCount = lineBytes.Length;

            return GetBreakPoint(lineBytes);
        }

        public bool ReadInt(out int value, byte[] newBreak = null)
        {
            if (newBreak == null) newBreak = new byte[1] { CONST.ASCII_TABLE };
            value = 0;
        Check:
            // todo: 符号
            int endIdx = GetBreakPoint(newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            for (int i = endIdx; i >= CurIdx; --i)
            {
                byte number = Content[i];
                value += number * (int)Math.Pow(10, i - CurIdx);
            }

            ReadMoveNext(endIdx - CurIdx + 1 + newBreak.Length);
            return true;
        }

        public bool ReadFloat(out float value, byte[] newBreak = null)
        {
            if (newBreak == null) newBreak = new byte[1] { CONST.ASCII_TABLE };
            value = 0;
        Check:
            // todo: 符号
            int endIdx = GetBreakPoint(newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            int pointIdx = CurIdx;
            for (; pointIdx <= endIdx; ++pointIdx)
            {
                if (Content[pointIdx] == '.') break;
            }

            for (int i = pointIdx - 1; i >= CurIdx; --i)
            {
                byte number = Content[i];
                value += number * (int)Math.Pow(10, pointIdx - 1 - i);
            }

            for (int i = pointIdx + 1; i <= endIdx; ++i)
            {
                byte number = Content[i];
                value += number * (int)Math.Pow(10, pointIdx + 1 - i);
            }

            ReadMoveNext(endIdx - CurIdx + 1 + newBreak.Length);
            return true;
        }

        public bool ReadString(out string value, byte[] newBreak = null)
        {
            if (newBreak == null) newBreak = new byte[1] { CONST.ASCII_TABLE };
            value = null;
        Check:
            int endIdx = GetBreakPoint(newBreak);
            bool goBack = endIdx == -1 && FillContent();
            if (goBack) goto Check;

            if (endIdx == -1) return false;

            value = UnicodeEncoding.UTF8.GetString(Content, CurIdx, endIdx - CurIdx + 1);

            ReadMoveNext(endIdx - CurIdx + 1 + newBreak.Length);
            return true;
        }


        public void WriteList<T>(List<T> list)
        {
            Type iType = typeof(T);

            Action<Action<T>> Iterater = Action => 
            {
                foreach (T t in list) Action(t);
            };

            byte[] itemBreak = new byte[1] { CONST.ASCII_COMMA };

            if (iType == typeof(int)) Iterater(t => { WriteInt(Convert.ToInt32(t), itemBreak); });
            else if (iType == typeof(float)) Iterater(t => { WriteFloat(Convert.ToSingle(t), itemBreak); });
            else if (iType == typeof(string)) Iterater(t => { WriteString(Convert.ToString(t), itemBreak); });
            else throw new Exception($"this type is not supported. iType: {iType}");
            
            WriteBreakPoint(CONST.ASCII_TABLE);
        }

        public void WriteDictionary<K, V>(Dictionary<K, V> dic)
        {
            byte[] noBreak = new byte[0];

            Action<object> itemHandle = item =>
            {
                if (item is int iV) WriteInt(iV, noBreak);
                else if (item is float fV) WriteFloat(fV, noBreak);
                else if (item is string sV) WriteString(sV, noBreak);
                else throw new Exception($"this type is not supported. iType: {item.GetType()}");
            };

            foreach (var kv in dic)
            {
                itemHandle(kv.Key);
                WriteBreakPoint(CONST.ASCII_EQUAL);
                itemHandle(kv.Value);
                WriteBreakPoint(CONST.ASCII_COMMA);
            }

            WriteBreakPoint(CONST.ASCII_TABLE);
        }


        public bool ReadList<T>(List<T> value)
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
            byte[] itemBreak = new byte[1] { CONST.ASCII_COMMA };
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
                if (!ReadInt(out int iV, itemBreak))
                {
                    throw new Exception($"[error][StreamTool]. read int failed");
                }
                return (T)Convert.ChangeType(iV, typeof(T));
            });
            else if (iType == typeof(float)) Iterater(() => {
                if (!ReadFloat(out float fV, itemBreak))
                {
                    throw new Exception($"[error][StreamTool]. read float failed");
                }
                return (T)Convert.ChangeType(fV, typeof(T));
            });
            else if (iType == typeof(string)) Iterater(() => {
                if (!ReadString(out string sV, itemBreak))
                {
                    throw new Exception($"[error][StreamTool]. read string failed");
                }
                return (T)Convert.ChangeType(sV, typeof(T));
            });
            else throw new Exception($"this type is not supported. iType: {iType}");

            ReadMoveNext(endIdx - CurIdx + 1 + breakPoint.Length);
            return true;
        }

        public bool ReadDic<K, V>(Dictionary<K, V> value)
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

            byte[] equalBreak = new byte[1] { CONST.ASCII_EQUAL };
            byte[] commaBreak = new byte[1] { CONST.ASCII_COMMA };

            Func<Type, byte[], object> itemHandle = (type, itemBreak) =>
            {
                if (type == typeof(int)) 
                {
                    if (!ReadInt(out int iV, itemBreak))
                    {
                        throw new Exception($"[error][StreamTool]. read int failed");
                    }
                    return Convert.ChangeType(iV, type);
                } 
                else if (type == typeof(float))
                {
                    if (!ReadFloat(out float fV, itemBreak))
                    {
                        throw new Exception($"[error][StreamTool]. read float failed");
                    }
                    return Convert.ChangeType(fV, type);
                }
                else if (type == typeof(string))
                {
                    if (!ReadString(out string sV, itemBreak))
                    {
                        throw new Exception($"[error][StreamTool]. read string failed");
                    }
                    return Convert.ChangeType(sV, type);
                }
                else throw new Exception($"this type is not supported. iType: {type}");
            };

            while (CurIdx < endIdx)
            {
                itemHandle(typeof(K), equalBreak);
                itemHandle(typeof(V), commaBreak);
            }

            ReadMoveNext(endIdx - CurIdx + 1 + breakPoint.Length);
            return true;
        }

    }
}
