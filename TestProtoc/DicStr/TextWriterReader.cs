using System.Diagnostics;

namespace TestProtoc.DicStr
{
    internal class TextWriterReader
    {
        public void Run_muchIO()
        {
            var data = TextWriterReader.ReadOriginalData();

            string filePath = CONST.SELF_PATH + "/dicStr.txt";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                StreamWriter writer = File.CreateText(filePath);
                Write(writer, data);
                writer.Dispose();

                string[] allLines = File.ReadAllLines(filePath);
                Read(allLines);
            }

            GC.Collect();

            long total = timer.ElapsedMilliseconds;
            long average = total / CONST.RUN_COUNT;
            Console.WriteLine($"[Text][Run_muchIO]. totalTime: {total}; average: {average}");
        }

        public void Run_onceIO()
        {

            var data = TextWriterReader.ReadOriginalData();

            string filePath = CONST.SELF_PATH + "/dicStr.txt";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();
            StreamWriter writer = File.CreateText(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                writer.BaseStream.Position = 0;
                Write(writer, data);
            }
            writer.Dispose();
            GC.Collect();

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;
            timer.Restart();

            string[] allLines = File.ReadAllLines(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                Read(allLines);
            }

            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.WriteLine($"[Text][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage}; readTotal: {readTotal}; readAverage: {readAverage}");
        }

        public static Dictionary<string, string> ReadOriginalData()
        {
            string path = CONST.SELF_PATH + "/zh-CN.txt";
            string[] allLines = File.ReadAllLines(path);
            Dictionary<string, string> dic = new Dictionary<string, string>(allLines.Length);
            foreach (string lineStr in allLines)
            {
                string[] strs = lineStr.Split("=");
                dic[strs[0]] = strs[1];
            }
            return dic;
        }

        public Dictionary<string, string> Read(string[] allLines)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>(allLines.Length);
            foreach (string lineStr in allLines)
            {
                string[] strs = lineStr.Split("=");
                dic[strs[0]] = strs[1];
            }
            return dic;
        }

        public void Write(StreamWriter writer, Dictionary<string, string> dic)
        {
            foreach (var kv in dic)
            {
                string line = $"{kv.Key}={kv.Value}";
                writer.WriteLine(line);
            }
            
            writer.Flush();
        }
    }
}
