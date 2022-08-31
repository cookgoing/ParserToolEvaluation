﻿using System.Diagnostics;
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

            long writeAllocation1 = GC.GetTotalAllocatedBytes(true);
            StreamWriter stream = File.CreateText(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                stream.BaseStream.Position = 0;
                Write(stream, data);
            }
            stream.Dispose();
            long writeAllocation2 = GC.GetTotalAllocatedBytes(true);
            double writeGC = (double)(writeAllocation2 - writeAllocation1) / (1024 * 1024);
            writeGC = (int)(writeGC * 100) / (double)100;
            GC.Collect();

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;
            timer.Restart();

            long readAllocation1 = GC.GetTotalAllocatedBytes(true);
            string content = File.ReadAllText(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                Read(content);
            }

            long readAllocation2 = GC.GetTotalAllocatedBytes(true);
            double readGC = (readAllocation2 - readAllocation1) / (1024 * 1024);
            readGC = (int)(readGC * 100) / (double)100;
            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.Write($"[NewtonJson][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage};   ||    readTotal: {readTotal}; readAverage: {readAverage}");
            Console.WriteLine($"    ||  [GC]. writeGC: {writeGC} M; readGC: {readGC} M");
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
