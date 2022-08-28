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
            BinaryStreamTool writeTool = new BinaryStreamTool(filePath, RWType.Write);
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

            BinaryStreamTool readTool = new BinaryStreamTool(filePath, RWType.Read);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                readTool.ResetIdx();
                Read(readTool);
            }
            readTool.Dispose();

            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.WriteLine($"[BinaryStream][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage}; readTotal: {readTotal}; readAverage: {readAverage}");
        }

        public void Write(BinaryStreamTool writeTool, AllType obj)
        {
            writeTool.WriteInt(obj.Id);
            writeTool.WriteString(obj.Name);
            writeTool.WriteList<int>(obj.ListInt);
            writeTool.WriteList<string>(obj.ListStr);
            writeTool.WriteDictionary<int, int>(obj.MapInt);
            writeTool.WriteDictionary<string, string>(obj.MapStr);
            writeTool.WriteDictionary<int, string>(obj.MapIntStr);

            writeTool.FlushContent();
        }

        public AllType Read(BinaryStreamTool readTool)
        {
            AllType result = new AllType();
            readTool.TryReadInt(out int id);
            readTool.TryReadString(out string name);
            readTool.TryReadList<int>(result.ListInt);
            readTool.TryReadList<string>(result.ListStr);
            readTool.TryReadDic<int, int>(result.MapInt);
            readTool.TryReadDic<string, string>(result.MapStr);
            readTool.TryReadDic<int, string>(result.MapIntStr);

            return result;
        }
    }
}
