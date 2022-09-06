namespace TestProtoc
{
    public static class CONST
    {
        private static string project_dir;

        public static string Project_dir
        { 
            get 
            {
                if (string.IsNullOrEmpty(project_dir))
                {
                    project_dir = Directory.GetCurrentDirectory();
                    project_dir = project_dir.Replace(@"TestProtoc\bin\Debug\net6.0", string.Empty);
                }
                
                return project_dir;
            }
        }

        public static string SELF_PATH = Project_dir + "self";

        public static string NEWTON_JSON_PATH = Project_dir + "newtonJson";

        public static string PB_BYTES_PATH = Project_dir + "pbBytes";

        public static string PB_JSON_PATH = Project_dir + "pbJson";

        public static string PROTO_EXE_PATH = Project_dir + "protoc.exe";

        public static string PROTO_PATH = Project_dir + "proto";

        public static string PROTO_CS_PATH = Project_dir + "protoCS";

        public static string STREAM_PATH = Project_dir + "stream";

        public static string BINARY_PATH = Project_dir + "binary";

        public const int RUN_COUNT = 100;


        public const byte ASCII_TABLE = (byte)'\t';
        public const byte ASCII_RETURN = (byte)'\r';
        public const byte ASCII_NEXLINE = (byte)'\n';
        public const byte ASCII_COMMA = (byte)',';
        public const byte ASCII_EQUAL = (byte)'=';
        public const byte ASCII_ZERO = (byte)'0';
        public const byte ASCII_NEGATIVE = (byte)'-';
        public const byte ASCII_POINT = (byte)'.';
        public const byte ASCII_NULL = 0;
    }
}
