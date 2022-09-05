using TestProtoc.Tool;
using System.Diagnostics;

namespace TestProtoc.Common
{
    internal class StreamWriterReader
    {

        public void Run()
        {
            var data = TextWriterReader.GetOriginalData();
            string filePath = CONST.STREAM_PATH + "/common.txt";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();

            long writeAllocation1 = GC.GetTotalAllocatedBytes(true);
            StreamTool writeTool = new StreamTool(RWType.Write, filePath);
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
            StreamTool readTool = new StreamTool(RWType.Read, filePath);
            List<AllType> list = null;
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                readTool.ResetIdx();
                list = Read(readTool);
            }
            readTool.Dispose();

            long readAllocation2 = GC.GetTotalAllocatedBytes(true);
            double readGC = (readAllocation2 - readAllocation1) / (1024 * 1024);
            readGC = (int)(readGC * 100) / (double)100;
            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.Write($"[Stream][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage};   ||    readTotal: {readTotal}; readAverage: {readAverage}");
            Console.WriteLine($"    ||  [GC]. writeGC: {writeGC} M; readGC: {readGC} M");

            //AllType allType = list?[Random.Shared.Next(0, list.Count)];
            //Console.WriteLine($"{allType.Id};{allType.Name};{allType.Vision};{allType.ListInt[1]};{allType.ListStr[2]};{allType.MapInt[22]};{allType.MapStr["cc"]};{allType.MapIntStr[45]}");
        }

        public void Write(StreamTool writeTool, List<AllType> list)
        {
            foreach (AllType obj in list)
            {
                writeTool.WriteInt(obj.Id);
                writeTool.WriteString(obj.Name);
                writeTool.WriteFloat(obj.Vision);
                writeTool.WriteList<int>(obj.ListInt);
                writeTool.WriteList<string>(obj.ListStr);
                writeTool.WriteDictionary<int, int>(obj.MapInt);
                writeTool.WriteDictionary<string, string>(obj.MapStr);
                writeTool.WriteDictionary<int, string>(obj.MapIntStr);
                writeTool.WriteLine();
            }

            writeTool.FlushContent();
        }

        public List<AllType> Read(StreamTool readTool)
        {
            List<AllType> list = new List<AllType>(1000);
            Func<bool> func = () =>
            {
                bool result = readTool.GetLine() != -1;
                if (!result) {
                    readTool.FillContent();
                    result = readTool.GetLine() != -1;
                }
                
                return result;
            };

            while (func())
            {
                AllType result = new AllType();
                readTool.ReadInt(out int id);
                readTool.ReadString(out string name);
                readTool.ReadFloat(out float vision);
                readTool.ReadList<int>(result.ListInt);
                readTool.ReadList<string>(result.ListStr);
                readTool.ReadDic<int, int>(result.MapInt);
                readTool.ReadDic<string, string>(result.MapStr);
                readTool.ReadDic<int, string>(result.MapIntStr);
                readTool.ReadMoveNext(2);
                result.Id = id;
                result.Name = name;
                result.Vision = vision;
                list.Add(result);
            }

            return list;
        }

    }
}
