using System.Diagnostics;

namespace TestProtoc.Common
{
    public class AllType
    {
        public int Id;
        public string Name;
        public float Vision;
        public List<int> ListInt { get; private set; }
        public List<string> ListStr { get; private set; }
        public Dictionary<int, int> MapInt { get; private set; }
        public Dictionary<string, string> MapStr { get; private set; }
        public Dictionary<int, string> MapIntStr { get; private set; }

        public AllType()
        {
            ListInt = new List<int>();
            ListStr = new List<string>();
            MapInt = new Dictionary<int, int>();
            MapStr = new Dictionary<string, string>();
            MapIntStr = new Dictionary<int, string>();
        }
    }

    internal class TextWriterReader
    {
        public static List<AllType> GetOriginalData()
        {
            var obj = new AllType()
            {
                Id = 123456,
                Name = "ding",
                Vision = 1.5f,
            };

            obj.ListInt.AddRange(new int[] { 1,2,3,4,5,6,7,8,9,0});
            obj.ListStr.AddRange(new string[] { "a", "b", "c", "d", "e", "f", "g", "h", "i" });
            obj.MapInt.Add(11, 111);
            obj.MapInt.Add(22, 222);
            obj.MapInt.Add(33, 333);
            obj.MapInt.Add(44, 444);
            obj.MapInt.Add(55, 555);
            obj.MapInt.Add(66, 666);
            obj.MapInt.Add(77, 777);
            obj.MapStr.Add("aa", "aaa");
            obj.MapStr.Add("bb", "bbb");
            obj.MapStr.Add("cc", "ccc");
            obj.MapStr.Add("dd", "ddd");
            obj.MapStr.Add("ee", "eee");
            obj.MapStr.Add("ff", "fff");
            obj.MapStr.Add("gg", "ggg");
            obj.MapStr.Add("hh", "hhh");
            obj.MapIntStr.Add(12, "ab");
            obj.MapIntStr.Add(23, "bc");
            obj.MapIntStr.Add(34, "cd");
            obj.MapIntStr.Add(45, "de");
            obj.MapIntStr.Add(56, "ef");
            obj.MapIntStr.Add(67, "fg");
            obj.MapIntStr.Add(78, "gh");

            List<AllType> list = new List<AllType>(10000);
            for (int i = 0; i < 10000; ++i)
            {
                list.Add(obj);
            }

            return list;
        }

        public void Run()
        {
            var data = TextWriterReader.GetOriginalData();

            string filePath = CONST.SELF_PATH + "/common.txt";
            if (File.Exists(filePath)) File.Delete(filePath);

            Stopwatch timer = Stopwatch.StartNew();
            long writeAllocation1 = GC.GetTotalAllocatedBytes(true);
            StreamWriter writer = File.CreateText(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                writer.BaseStream.Position = 0;
                Write(writer, data);
            }
            writer.Dispose();
            long writeAllocation2 = GC.GetTotalAllocatedBytes(true);
            double writeGC = (writeAllocation2 - writeAllocation1) / (1024 * 1024);
            writeGC = (int)(writeGC * 100) / (double)100;
            GC.Collect();

            long writeTotal = timer.ElapsedMilliseconds;
            long writeAverage = writeTotal / CONST.RUN_COUNT;
            timer.Restart();

            long readAllocation1 = GC.GetTotalAllocatedBytes(true);
            string[] allLine = File.ReadAllLines(filePath);
            for (int i = 0; i < CONST.RUN_COUNT; ++i)
            {
                Read(allLine);
            }

            long readAllocation2 = GC.GetTotalAllocatedBytes(true);
            double readGC = (readAllocation2 - readAllocation1) / (1024 * 1024);
            readGC = (int)(readGC * 100) / (double)100;
            GC.Collect();

            long readTotal = timer.ElapsedMilliseconds;
            long readAverage = readTotal / CONST.RUN_COUNT;
            Console.Write($"[Text][Run_onceIO]. writeTotal: {writeTotal}; writeAverage: {writeAverage};     ||    readTotal: {readTotal}; readAverage: {readAverage}");
            Console.WriteLine($"    ||  [GC]. writeGC: {writeGC} M; readGC: {readGC} M");
        }

        public void Write(StreamWriter writer, List<AllType> list)
        {
            foreach (AllType obj in list)
            {
                writer.Write(obj.Id); writer.Write((char)CONST.ASCII_TABLE);
                writer.Write(obj.Name); writer.Write((char)CONST.ASCII_TABLE);
                writer.Write(obj.Vision); writer.Write((char)CONST.ASCII_TABLE);

                foreach (var i in obj.ListInt)
                {
                    writer.Write(i); writer.Write((char)CONST.ASCII_COMMA);
                }
                writer.Write((char)CONST.ASCII_TABLE);

                foreach (var i in obj.ListStr)
                {
                    writer.Write(i); writer.Write((char)CONST.ASCII_COMMA);
                }
                writer.Write((char)CONST.ASCII_TABLE);

                foreach (var kv in obj.MapInt)
                {
                    writer.Write(kv.Key); writer.Write((char)CONST.ASCII_EQUAL);
                    writer.Write(kv.Value); writer.Write((char)CONST.ASCII_COMMA);
                }
                writer.Write((char)CONST.ASCII_TABLE);

                foreach (var kv in obj.MapStr)
                {
                    writer.Write(kv.Key); writer.Write((char)CONST.ASCII_EQUAL);
                    writer.Write(kv.Value); writer.Write((char)CONST.ASCII_COMMA);
                }
                writer.Write((char)CONST.ASCII_TABLE);

                foreach (var kv in obj.MapIntStr)
                {
                    writer.Write(kv.Key); writer.Write((char)CONST.ASCII_EQUAL);
                    writer.Write(kv.Value); writer.Write((char)CONST.ASCII_COMMA);
                }
                writer.Write((char)CONST.ASCII_NEXLINE);
            }

            writer.Flush();
        }

        public List<AllType> Read(string[] allLines)
        {
            var list = new List<AllType>();

            foreach (string content in allLines)
            {
                AllType result = new AllType();

                string[] contentArr = content.Split((char)CONST.ASCII_TABLE);
                if (contentArr.Length < 8)
                {
                    Console.WriteLine("[error][TextWriterReader]. content is illegal");
                    return null;
                }

                result.Id = Convert.ToInt32(contentArr[0]);
                result.Name = contentArr[1];
                result.Vision = Convert.ToSingle(contentArr[2]);
                string[] listIntArr = contentArr[3].Split((char)CONST.ASCII_COMMA);
                foreach (var li in listIntArr)
                {
                    if (string.IsNullOrEmpty(li)) continue;
                    result.ListInt.Add(Convert.ToInt32(li));
                }

                string[] listStrArr = contentArr[4].Split((char)CONST.ASCII_COMMA);
                foreach (var ls in listStrArr)
                {
                    if (string.IsNullOrEmpty(ls)) continue;
                    result.ListStr.Add(ls);
                }

                string[] dicIntArr = contentArr[5].Split((char)CONST.ASCII_COMMA);
                foreach (var di in dicIntArr)
                {
                    if (string.IsNullOrEmpty(di)) continue;
                    string[] kv = di.Split((char)CONST.ASCII_EQUAL);
                    result.MapInt.TryAdd(Convert.ToInt32(kv[0]), Convert.ToInt32(kv[1]));
                }

                string[] dicStrArr = contentArr[6].Split((char)CONST.ASCII_COMMA);
                foreach (var ds in dicStrArr)
                {
                    if (string.IsNullOrEmpty(ds)) continue;
                    string[] kv = ds.Split((char)CONST.ASCII_EQUAL);
                    result.MapStr.TryAdd(kv[0], kv[1]);
                }

                string[] dicIntStrArr = contentArr[7].Split((char)CONST.ASCII_COMMA);
                foreach (var dis in dicIntStrArr)
                {
                    if (string.IsNullOrEmpty(dis)) continue;
                    string[] kv = dis.Split((char)CONST.ASCII_EQUAL);
                    result.MapIntStr.TryAdd(Convert.ToInt32(kv[0]), kv[1]);
                }

                list.Add(result);
            }

            return list;
        }
    }
}
