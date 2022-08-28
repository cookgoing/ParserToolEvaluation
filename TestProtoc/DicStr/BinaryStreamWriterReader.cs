using TestProtoc.Tool;
using System.Diagnostics;

namespace TestProtoc.DicStr
{
    internal class BinaryStreamWriterReader
    {
        public void Run_muchIO()
        {
            var data = TextWriterReader.ReadOriginalData();
            string filePath = CONST.BINARY_PATH + "/dicStr.bytes";

            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();

            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                BinaryStreamTool writeTool = new BinaryStreamTool(filePath, RWType.Write);
                Write(writeTool, data);
                writeTool.Dispose();

                BinaryStreamTool readTool = new BinaryStreamTool(filePath, RWType.Read);
                Read(readTool);
                readTool.Dispose();
            }

            GC.Collect();
            long total = timer.ElapsedMilliseconds;
            long average = total / CONST.RUN_COUNT;

            Console.WriteLine($"[BinaryStream][Run_muchDispose]. totalTime: {total}; average: {average}");
        }

        public void Run_onceIO()
        {
            var data = TextWriterReader.ReadOriginalData();
            string filePath = CONST.BINARY_PATH + "/dicStr.bytes";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();
            long writeAllocation1 = GC.GetTotalAllocatedBytes(true);
            BinaryStreamTool writeTool = new BinaryStreamTool(filePath, RWType.Write);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                writeTool.ResetIdx();
                Write(writeTool, data);
            }
            writeTool.Dispose();
            long writeAllocation2 = GC.GetTotalAllocatedBytes(true);
            double writeGC = (writeAllocation2 - writeAllocation1) / (1024 * 1024);
            writeGC = (int)(writeGC * 100) / (double)100;
            GC.Collect();

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;
            timer.Restart();

            long readAllocation1 = GC.GetTotalAllocatedBytes(true);
            BinaryStreamTool readTool = new BinaryStreamTool(filePath, RWType.Read);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                readTool.ResetIdx();
                Read(readTool);
            }
            readTool.Dispose();

            long readAllocation2 = GC.GetTotalAllocatedBytes(true);
            double readGC = (readAllocation2 - readAllocation1) / (1024 * 1024);
            readGC = (int)(readGC * 100) / (double)100;
            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.Write($"[BinaryStream][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage};      readTotal: {readTotal}; readAverage: {readAverage}");
            Console.WriteLine($"    ||  [GC]. writeGC: {writeGC} M; readGC: {readGC} M");
        }


        public void Write(BinaryStreamTool writeTool, Dictionary<string, string> dic)
        {
            foreach (var kv in dic)
            {
                writeTool.WriteString(kv.Key);
                writeTool.WriteString(kv.Value);
                writeTool.WriteBreakPoint(CONST.ASCII_TABLE);
            }

            writeTool.FlushContent();
        }

        public Dictionary<string, string> Read(BinaryStreamTool readTool)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            int lineCount = 0, readIdx = 0;
            while ((readIdx = readTool.GetBreakPoint(CONST.ASCII_TABLE)) != -1)
            {
                lineCount++;
                if (!readTool.TryReadString(out string key))
                {
                    Console.WriteLine($"[error][StreamWriterReader]. no key. lineCount: {lineCount}");
                    readTool.ReadMoveNext(1 + readIdx);
                    continue;
                }
                if (!readTool.TryReadString(out string value))
                {
                    Console.WriteLine($"[error][StreamWriterReader]. no value. lineCount: {lineCount}");
                    readTool.ReadMoveNext(1 + readIdx);
                    continue;
                }
                readTool.ReadMoveNext(1);

                dic.TryAdd(key, value);
            }

            return dic;
        }

    }
}
