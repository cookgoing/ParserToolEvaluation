﻿using System.Diagnostics;

public class Program
{
    class JsonTest2
    {
        public int id;
        public string name;
        public List<int> listInt;
        public List<string> listStr;
        public Dictionary<int, int> mapInt;
        public Dictionary<string, string> mapStr;
        public Dictionary<int, string> mapIntStr;
    }

    public static void Main()
    {
        //GenerateCs(CONST.PROTO_EXE_PATH, CONST.PROTO_PATH, CONST.PROTO_CS_PATH);

        Run_strDic();
        Run_common();

    }

    public static void Run_strDic()
    {
        Console.WriteLine("============Run_strDic===========");

        //new TestProtoc.DicStr.TextWriterReader().Run_muchIO();
        //new TestProtoc.DicStr.StreamWriterReader().Run_muchIO();
        //new TestProtoc.DicStr.BinaryStreamWriterReader().Run_muchIO();
        //new TestProtoc.DicStr.NewtonJsonWriterReader().Run_muchIO();
        //new TestProtoc.DicStr.ProtoWriterReader().Run_muchIO();

        //Console.WriteLine("--> oneIO");

        new TestProtoc.DicStr.TextWriterReader().Run_onceIO();
        new TestProtoc.DicStr.StreamWriterReader().Run_onceIO();
        new TestProtoc.DicStr.BinaryStreamWriterReader().Run_onceIO();
        new TestProtoc.DicStr.NewtonJsonWriterReader().Run_onceIO();
        new TestProtoc.DicStr.ProtoWriterReader().Run_onceIO();
    }

    public static void Run_common()
    {
        Console.WriteLine("============Run_common===========");

        new TestProtoc.Common.TextWriterReader().Run();
        new TestProtoc.Common.StreamWriterReader().Run();
        new TestProtoc.Common.BinaryStreamWriterReader().Run();
        new TestProtoc.Common.NewtonJsonWriterReader().Run();
        new TestProtoc.Common.ProtoWriterReader().Run();
    }

    

    private void GenerateCs(string exePath, string protoPath, string csPath)
    {
        Process protoc = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo(exePath);

        startInfo.Arguments = $"--proto_path={protoPath} --csharp_out={csPath} {protoPath}/Test.proto";
        protoc.StartInfo = startInfo;
        protoc.Start();
    }

}