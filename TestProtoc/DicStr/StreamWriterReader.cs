using TestProtoc.Tool;
using System.Diagnostics;

namespace TestProtoc.DicStr
{
    internal class StreamWriterReader
    {
        public void Run_muchIO()
        {
            var data = TextWriterReader.ReadOriginalData();
            string filePath = CONST.STREAM_PATH + "/dicStr.txt";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();

            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                StreamTool writeTool = new StreamTool(RWType.Write, filePath);
                Write(writeTool, data);
                writeTool.Dispose();

                StreamTool readTool = new StreamTool(RWType.Read, filePath);
                Read(readTool);
                readTool.Dispose();
            }

            GC.Collect();
            long total = timer.ElapsedMilliseconds;
            long average = total / CONST.RUN_COUNT;

            Console.WriteLine($"[Stream][Run_muchDispose]. totalTime: {total}; average: {average}");
        }

        public void Run_onceIO()
        {
            var data = TextWriterReader.ReadOriginalData();
            string filePath = CONST.STREAM_PATH + "/dicStr.txt";
            if(File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();
            
            StreamTool writeTool = new StreamTool(RWType.Write, filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                writeTool.ResetIdx();
                Write(writeTool, data);
            }
            writeTool.Dispose();
            GC.Collect();

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;
            timer.Restart();

            StreamTool readTool = new StreamTool(RWType.Read, filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                readTool.ResetIdx();
                Read(readTool);
            }
            readTool.Dispose();

            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.WriteLine($"[Stream][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage}; readTotal: {readTotal}; readAverage: {readAverage}");
        }

        public void Write(StreamTool writeTool, Dictionary<string, string> dic)
        {
            foreach (var kv in dic)
            {
                writeTool.WriteString(kv.Key);
                writeTool.WriteString(kv.Value);
                writeTool.WriteLine();
            }
            writeTool.FlushContent();
        }

        public Dictionary<string, string> Read(StreamTool readTool)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            int lineCount = 0, readIdx = 0;
            while ((readIdx = readTool.GetLine(out int lineByte)) != -1)
            {
                lineCount++;
                if (!readTool.ReadString(out string key))
                {
                    Console.WriteLine($"[error][StreamWriterReader]. no key. lineCount: {lineCount}");
                    readTool.ReadMoveNext(lineByte + readIdx);
                    continue;
                }
                if (!readTool.ReadString(out string value))
                {
                    Console.WriteLine($"[error][StreamWriterReader]. no value. lineCount: {lineCount}");
                    readTool.ReadMoveNext(lineByte + readIdx);
                    continue;
                }   
                readTool.ReadMoveNext(lineByte);

                dic.TryAdd(key, value);
            }

            return dic;
        }
    }
}
