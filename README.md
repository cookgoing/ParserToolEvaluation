# 环境

## 硬件

    2021联想 小新pro14. cup: AMD Ryzen 7 5800H(8核16线程); 内存： 16G

## 软件

    visual studio 2022
    .net6.0

## 第三方库

    Google.Protobuf(3.21.5)
    Newtonsoft.json(13.0.1)

# 待测试的解析工具

TextWriterReader: 自定义的纯文本解析
StreamWriterReader: 以我师傅曾今的思想，自己写了一套文本流解析
NewtonJsonWriterReader: Json 解析工具
ProtoWriterReader: protobuf 解析工具

    这些解析工具主要解析两种数据格式，strDic, common.
        strDic: 字符串对格式的文本，上万行
        common: 自认为相对通用的格式，里面包含各种数据类型，整形，字符串，浮点型，List, Dic；一万行
    protobuf 也有Json解析格式，也参与测试了

## 耗时 (ms)

[点这里](https://github.com/cookgoing/ParserToolEvaluation/blob/master/images/%E8%80%97%E6%97%B6.xlsx)

## GC (MB)

[点这里](https://github.com/cookgoing/ParserToolEvaluation/blob/master/images/GC.xlsx)

## 文件大小 (KB)

[点这里](https://github.com/cookgoing/ParserToolEvaluation/blob/master/images/%E6%96%87%E4%BB%B6%E5%A4%A7%E5%B0%8F.xlsx)

# 说明

1. 下述统计只是1次的最终结果，并不是多次取平均，所以我觉得不严谨。但是大致也能说明一些问题。
2. 在实际项目中，客户端的读取的操作要远远多于写入的操作，所以写入的权重相较要高些。
3. 在实际项目中，文件大多只需要被写入，读取一次。所以1次循环比多次更有权重。
4. 我依然觉得这个测评只能作为参考，具体项目可能结果会完全不同。

# 结论

1. Text: GC有点平庸； 耗时，文件大小都是很优秀的。让我很意外。
2. Stream: 写入字符串的GC几乎没有，其他GC比较平庸（原因是List, Dic用到泛型，写入和读取都会有GC）;耗时有点高（我功力不到家啊）。总体而言并我是有些失望的。
   
3. NewtonJson: 除去Text, 耗时和GC方面表现优异；但是文件大小比较大。不过对我而言，它最值致命的问题是1次循环写入纯字符串的文件，耗时很大。这个需要今后具体项目中测试决定是否使用
4. Proto: 各方面都比叫平庸，所以没有长板也就没什么短板了。但是感觉写入操作有点差，但是也大差不差的。
5. Proto_json: 直接放弃

