using System.Diagnostics;
using Google.Protobuf;
using Ding.Test;

namespace TestProtoc.Common
{
    internal class ProtoWriterReader
    {
        public void Run()
        {
            var data = TextWriterReader.GetOriginalData();

            string filePath = CONST.PB_BYTES_PATH + "/common.bytes";
            string jsonPath = CONST.PB_JSON_PATH + "/common.json";
            if (File.Exists(filePath)) File.Delete(filePath);
            if (File.Exists(jsonPath)) File.Delete(jsonPath);

            Stopwatch timer = Stopwatch.StartNew();
            FileStream writeStream = File.OpenWrite(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                writeStream.Position = 0;
                Write(writeStream, data);
            }
            writeStream.Dispose();

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;

            timer.Restart();
            FileStream readStream = File.OpenRead(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                readStream.Position = 0;
                Read(readStream);
            }
            readStream.Dispose();

            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.WriteLine($"[Proto][Run_onceIO][bytes]. writeTotal: {writeTotal}; writeAverage: {writeAverage}; readTotal: {readTotal}; readAverage: {readAverage}");

            timer.Restart();
            StreamWriter streamWriter = new StreamWriter(jsonPath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                streamWriter.BaseStream.Position = 0;
                WriteJson(streamWriter, data);
            }
            streamWriter.Dispose();

            writeTotal = timer.ElapsedMilliseconds;
            writeAverage = writeTotal / CONST.RUN_COUNT;

            timer.Restart();
            string content = File.ReadAllText(jsonPath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                ReadJson(content);
            }

            GC.Collect();

            readTotal = timer.ElapsedMilliseconds;
            readAverage = readTotal / CONST.RUN_COUNT;
            Console.WriteLine($"[Proto][Run_onceIO][Json]. writeTotal: {writeTotal}; writeAverage: {writeAverage}; readTotal: {readTotal}; readAverage: {readAverage}");
        }


        private void Write(Stream writeStream, AllType obj)
        {
            Ding.Test.AllType pbObj = new Ding.Test.AllType();
            pbObj.Id = obj.Id;
            pbObj.Name = obj.Name;
            pbObj.ListInt.Add(obj.ListInt);
            pbObj.ListStr.Add(obj.ListStr);
            pbObj.MapInt.Add(obj.MapInt);
            pbObj.MapStr.Add(obj.MapStr);
            pbObj.MapIntStr.Add(obj.MapIntStr);

            CodedOutputStream output = new CodedOutputStream(writeStream);
            pbObj.WriteTo(output);

            output.Flush();
        }

        private Ding.Test.AllType Read(Stream readStream)
        {
            return Ding.Test.AllType.Parser.ParseFrom(readStream);
        }

        private void WriteJson(StreamWriter streamWriter, AllType obj)
        {
            Ding.Test.AllType pbObj = new Ding.Test.AllType();
            pbObj.Id = obj.Id;
            pbObj.Name = obj.Name;
            pbObj.ListInt.Add(obj.ListInt);
            pbObj.ListStr.Add(obj.ListStr);
            pbObj.MapInt.Add(obj.MapInt);
            pbObj.MapStr.Add(obj.MapStr);
            pbObj.MapIntStr.Add(obj.MapIntStr);

            JsonFormatter.Default.Format(pbObj, streamWriter);

            streamWriter.Flush();
        }

        private Ding.Test.AllType ReadJson(string content)
        {
            return JsonParser.Default.Parse<Ding.Test.AllType>(content);
        }
    }
}
