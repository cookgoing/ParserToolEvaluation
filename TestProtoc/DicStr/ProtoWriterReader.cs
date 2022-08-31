using System.Diagnostics;
using Google.Protobuf;
using Ding.Test;

namespace TestProtoc.DicStr
{
    internal class ProtoWriterReader
    {
        public void Run_muchIO()
        {
            var data = TextWriterReader.ReadOriginalData();

            string filePath = CONST.PB_BYTES_PATH + "/dicStr.bytes";
            string jsonPath = CONST.PB_JSON_PATH + "/dicStr.json";
            if (File.Exists(filePath)) File.Delete(filePath);
            if (File.Exists(jsonPath)) File.Delete(jsonPath);

            Stopwatch timer = Stopwatch.StartNew();

            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                var writeStream = File.OpenWrite(filePath);
                Write(writeStream, data);
                writeStream.Dispose();

                var readStream = File.OpenRead(filePath);
                Read(readStream);
                readStream.Dispose();
            }

            GC.Collect();

            long total = timer.ElapsedMilliseconds;
            long average = total / CONST.RUN_COUNT;
            Console.WriteLine($"[Proto][Run_muchIO][bytes]. totalTime: {total}; average: {average}");

            timer.Restart();
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                StreamWriter streamWriter = new StreamWriter(jsonPath);
                WriteJson(streamWriter, data);
                streamWriter.Dispose();

                string content = File.ReadAllText(jsonPath);
                ReadJson(content);
            }

            GC.Collect();

            total = timer.ElapsedMilliseconds;
            average = total / CONST.RUN_COUNT;
            Console.WriteLine($"[Proto][Run_muchIO][Json]. totalTime: {total}; average: {average}");
        }

        public void Run_onceIO()
        {
            var data = TextWriterReader.ReadOriginalData();

            string filePath = CONST.PB_BYTES_PATH + "/dicStr.bytes";
            string jsonPath = CONST.PB_JSON_PATH + "/dicStr.json";
            if (File.Exists(filePath)) File.Delete(filePath);
            if (File.Exists(jsonPath)) File.Delete(jsonPath);

            Stopwatch timer = Stopwatch.StartNew();
            long pb_writeAllocation1 = GC.GetTotalAllocatedBytes(true);
            FileStream writeStream = File.OpenWrite(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                writeStream.Position = 0;
                Write(writeStream, data);
            }
            writeStream.Dispose();
            long pb_writeAllocation2 = GC.GetTotalAllocatedBytes(true);
            double pb_writeGC = (pb_writeAllocation2 - pb_writeAllocation1) / (1024 * 1024);
            pb_writeGC = (int)(pb_writeGC * 100) / (double)100;

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;

            timer.Restart();
            long pb_readAllocation1 = GC.GetTotalAllocatedBytes(true);
            FileStream readStream = File.OpenRead(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                readStream.Position = 0;
                Read(readStream);
            }
            readStream.Dispose();

            long pb_readAllocation2 = GC.GetTotalAllocatedBytes(true);
            double pb_readGC = (pb_readAllocation2 - pb_readAllocation1) / (1024 * 1024);
            pb_readGC = (int)(pb_readGC * 100) / (double)100;
            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.Write($"[Proto][Run_onceIO][bytes]. writeTotal: {writeTotal}; writeAverage: {writeAverage};    ||     readTotal: {readTotal}; readAverage: {readAverage}");
            Console.WriteLine($"    ||  [GC]. writeGC: {pb_writeGC} M; readGC: {pb_readGC} M");

            timer.Restart();
            long json_writeAllocation1 = GC.GetTotalAllocatedBytes(true);
            StreamWriter streamWriter = new StreamWriter(jsonPath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                streamWriter.BaseStream.Position = 0;
                WriteJson(streamWriter, data);
            }
            streamWriter.Dispose();
            long json_writeAllocation2 = GC.GetTotalAllocatedBytes(true);
            double json_writeGC = (json_writeAllocation2 - json_writeAllocation1) / (1024 * 1024);
            json_writeGC = (int)(json_writeGC * 100) / (double)100;

            writeTotal = timer.ElapsedMilliseconds;
            writeAverage = writeTotal / CONST.RUN_COUNT;

            timer.Restart();
            long json_readAllocation1 = GC.GetTotalAllocatedBytes(true);
            string content = File.ReadAllText(jsonPath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                ReadJson(content);
            }

            long json_readAllocation2 = GC.GetTotalAllocatedBytes(true);
            double json_readGC = (json_readAllocation2 - json_readAllocation1) / (1024 * 1024);
            json_readGC = (int)(json_readGC * 100) / (double)100;
            GC.Collect();

            readTotal = timer.ElapsedMilliseconds;
            readAverage = readTotal / CONST.RUN_COUNT;
            Console.Write($"[Proto][Run_onceIO][Json]. writeTotal: {writeTotal}; writeAverage: {writeAverage};       readTotal: {readTotal}; readAverage: {readAverage}");
            Console.WriteLine($"    ||  [GC]. writeGC: {json_writeGC} M; readGC: {json_readGC} M");
        }


        private void Write(Stream writeStream, Dictionary<string, string> dic)
        { 
            Maps maps = new Maps();
            maps.Dic.Add(dic);

            CodedOutputStream output = new CodedOutputStream(writeStream);
            maps.WriteTo(output);

            output.Flush();
        }

        private IDictionary<string, string> Read(Stream readStream)
        {
            var maps = Maps.Parser.ParseFrom(readStream);

            return maps.Dic;
        }

        private void WriteJson(StreamWriter streamWriter, Dictionary<string, string> dic)
        {
            Maps maps = new Maps();
            maps.Dic.Add(dic);

            JsonFormatter.Default.Format(maps, streamWriter);

            streamWriter.Flush();
        }

        private IDictionary<string, string> ReadJson(string content)
        {
            Maps map = JsonParser.Default.Parse<Maps>(content);

            return map.Dic;
        }
    }
}
