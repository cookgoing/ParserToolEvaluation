# 环境

## 硬件

    2020联想 小新pro14. cup: AMD Ryzen 7 5800H(8核16线程); 内存： 16G

## 软件

    visual studio 2022

## 第三方库

    Google.Protobuf(3.21.5)
    Newtonsoft.json(13.0.1)

# 待测试的解析工具

TextWriterReader: 自定义的纯文本解析
StreamWriterReader: 以我师傅曾今的思想，自己写了一套文本流解析
BinaryStreamWriterReader: 在Stream的基础上更近一步，字节流解析
NewtonJsonWriterReader: Json 解析工具
ProtoWriterReader: protobuf 解析工具

    这些解析工具主要解析两种数据格式，strDic, common.
        strDic: 字符串对格式的文本，上万行
        common: 自认为相对通用的格式，里面包含各种数据类型，整形，字符串，List, Dic（唯独忘记了浮点型，哭）；但是只有1行。
    protobuf 也有Json解析格式，也参与测试了

# Log

100次循环
![GitHub Logo](/assets/images/100.png)

## 耗时 (ms)

### strDic write

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|9|17|19|203|35|97|
|10|35|111|116|271|92|165|
|100|222|890|1033|429|469|703|
|1000|2075|8749|9672|2993|4383|6428|

### strDic read

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|10|1|1|31|27|66|
|10|38|2|2|90|80|280|
|100|564|1|1|457|653|2111|
|1000|3792|1|1|4083|6475|20051|

### common write

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|3|6|4|51|10|3|
|10|3|5|4|51|9|3|
|100|3|6|5|40|10|5|
|1000|8|20|15|47|18|15|

### common read

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|1|5|5|22|7|6|
|10|1|5|4|18|6|5|
|100|1|6|5|19|7|9|
|1000|5|27|18|28|14|41|

## GC (MB)

### strDic write

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|1|3|3|0.28|1|2|
|10|12|29|29|0.3|17|21|
|100|128|288|288|0.5|173|217|
|1000|1286|2876|2876|2.46|1731|2172|

### strDic read

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|4|1|1|4|3|11|
|10|28|1|1|27|33|91|
|100|267|1|1|256|334|892|
|1000|2660|1|1|2548|3343|8901|

### common write

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|0|1|1|0.35|0|0|
|10|0|1|1|0.35|0|0|
|100|0|1|1|0.56|0|0|
|1000|0|7|5|2.48|7|6|

### common read

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|0|1|1|0|0|0|
|10|0|1|1|0|0|0|
|100|0|1|1|0|0|1|
|1000|6|5|5|7|8|17|

## 文件大小 (KB)

因为 common 只有1行，数据量极小，没法比较出差距来。所以这里只罗列了strDic的文本大小

|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|
|745|759|745|789|790|820|

这里的统计是有些不公平的，理论上讲 binaryStream 的文本格式小更多，因为基础数据类型之间式没有其他字符间隔的。

# 结论


