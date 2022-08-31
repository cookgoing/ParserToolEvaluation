using TestProtoc.Tool;
using System.Diagnostics;

namespace TestProtoc.Common
{
    internal class BinaryStreamWriterReader
    {
        public void Run()
        {
            var data = TextWriterReader.GetOriginalData();
            string filePath = CONST.BINARY_PATH + "/common.bytes";
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
            Console.Write($"[BinaryStream][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage};     ||      readTotal: {readTotal}; readAverage: {readAverage}");
            Console.WriteLine($"    ||  [GC]. writeGC: {writeGC} M; readGC: {readGC} M");
        }

        public void Write(BinaryStreamTool writeTool, List<AllType> list)
        {
            foreach (var obj in list)
            {
                writeTool.WriteInt(obj.Id);
                writeTool.WriteString(obj.Name);
                writeTool.WriteFloat(obj.Vision);
                writeTool.WriteList<int>(obj.ListInt);
                writeTool.WriteList<string>(obj.ListStr);
                writeTool.WriteDictionary<int, int>(obj.MapInt);
                writeTool.WriteDictionary<string, string>(obj.MapStr);
                writeTool.WriteDictionary<int, string>(obj.MapIntStr);
                writeTool.WriteBreakPoint(CONST.ASCII_NEXLINE);
            }

            writeTool.FlushContent();
        }

        public List<AllType> Read(BinaryStreamTool readTool)
        {
            List < AllType > list = new List < AllType >();

            while (readTool.GetBreakPoint(CONST.ASCII_NEXLINE) != -1)
            {
                AllType result = new AllType();
                readTool.TryReadInt(out int id);
                readTool.TryReadString(out string name);
                readTool.TryReadFloat(out float vision);
                readTool.TryReadList<int>(result.ListInt);
                readTool.TryReadList<string>(result.ListStr);
                readTool.TryReadDic<int, int>(result.MapInt);
                readTool.TryReadDic<string, string>(result.MapStr);
                readTool.TryReadDic<int, string>(result.MapIntStr);
                result.Id = id;
                result.Name = name;
                result.Vision = vision;
                readTool.ReadMoveNext(1);
            }

            return list;
        }
    }
}
