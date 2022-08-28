using System.Diagnostics;
using Newtonsoft.Json;

namespace TestProtoc.DicStr
{
    internal class NewtonJsonWriterReader
    {
        public void Run_muchIO()
        {
            var data = TextWriterReader.ReadOriginalData();
            string filePath = CONST.NEWTON_JSON_PATH + "/dicStr.json";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();

            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                StreamWriter stream = File.CreateText(filePath);
                Write(stream, data);
                stream.Dispose();

                string content = File.ReadAllText(filePath);
                Read(content);
            }

            GC.Collect();

            long total = timer.ElapsedMilliseconds;
            long average = total / CONST.RUN_COUNT;
            Console.WriteLine($"[NewtonJson][Run_muchIO]. totalTime: {total}; average: {average}");
        }

        public void Run_onceIO()
        {
            var data = TextWriterReader.ReadOriginalData();
            string filePath = CONST.NEWTON_JSON_PATH + "/dicStr.json";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();

            StreamWriter stream = File.CreateText(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                stream.BaseStream.Position = 0;
                Write(stream, data);
            }
            stream.Dispose();
            GC.Collect();

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;
            timer.Restart();

            string content = File.ReadAllText(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                Read(content);
            }

            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.WriteLine($"[NewtonJson][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage}; readTotal: {readTotal}; readAverage: {readAverage}");
        }


        public void Write(StreamWriter stream, Dictionary<string, string> dic)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(stream, dic);
            
            stream.Flush();
        }

        public Dictionary<string, string> Read(string content)
        { 
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
        }
    }
}
