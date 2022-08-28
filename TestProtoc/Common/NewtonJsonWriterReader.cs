using System.Diagnostics;
using Newtonsoft.Json;

namespace TestProtoc.Common
{
    internal class NewtonJsonWriterReader
    {
        public void Run()
        {
            var data = TextWriterReader.GetOriginalData();
            string filePath = CONST.NEWTON_JSON_PATH + "/common.json";
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

        public void Write(StreamWriter stream, AllType obj)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(stream, obj);

            stream.Flush();
        }

        public AllType Read(string content)
        {
            return JsonConvert.DeserializeObject<AllType>(content);
        }
    }
}
