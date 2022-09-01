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
BinaryStreamWriterReader: 在Stream的基础上更近一步，字节流解析
NewtonJsonWriterReader: Json 解析工具
ProtoWriterReader: protobuf 解析工具

    这些解析工具主要解析两种数据格式，strDic, common.
        strDic: 字符串对格式的文本，上万行
        common: 自认为相对通用的格式，里面包含各种数据类型，整形，字符串，浮点型，List, Dic；一万行
    protobuf 也有Json解析格式，也参与测试了

# Log

100次循环
![GitHub Logo](/images/100.png)

# 说明

下述统计只是1次的最终结果，并不是多次取平均，所以我觉得不严谨。但是大致也能说明一些问题。

## 耗时 (ms)

### strDic write

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|98|17|19|209|51|83|
|10|44|102|136|606|164|163|
|100|394|997|1120|621|609|792|
|1000|2892|8521|10237|4142|5298|7328|

### strDic read

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|9|2|3|51|55|72|
|10|39|5|6|69|81|275|
|100|647|4|3|489|709|2081|
|1000|3803|3|3|4318|6471|19922|

### common write

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|34|114|83|111|116|138|
|10|191|1025|764|575|1088|1702|
|100|1859|10276|7828|5610|11307|18015|
|1000|18494|106029|87034|57019|111665|177698|

### common read

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|199|1|4|230|197|643|
|10|1302|2|4|1702|1447|5135|
|100|10719|1|3|17112|14783|56251|
|1000|114882|5|3|173332|146638|536966|

## GC (MB)

### strDic write

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|1|3|3|0.28|1|2|
|10|12|29|29|0.3|17|21|
|100|128|288|288|0.49|173|217|
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
|1|7|64|45|14.88|36|61|
|10|70|640|441|144.67|366|612|
|100|709|6394|4403|1442.5|3665|6128|
|1000|7095|63935|44022|14420.83|36653|61287|

### common read

|loop\tool|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|--|
|1|74|1|1|59|46|194|
|10|694|1|1|468|462|1780|
|100|6897|1|1|4556|4626|17644|
|1000|68921|1|1|45434|46266|176280|

## 文件大小 (KB)

### strDic
|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|
|745|759|745|789|790|820|

### common
|Text|Stream|BinaryStream|NewTonJson|Proto|Proto_json|
|--|--|--|--|--|--|
|2012|2022|2325|3692|2432|4532|

# 结论

**这个不能一刀切，需要去 Images中看对应的 图标**
**在实际项目中，读取的操作要远远多于写入的操作，所以这方面的权重要有一个概念**

## 耗时

### 横向

#### Write

StrDic: BinaryStream > Stream > Proto_json > Proto > NewTonJson > Text
Common: Proto_json > proto > Stream > BinaryStream > NewTonJson > Text

#### Read

StrDic: Proto_json > proto > NewTonJson > Text >> Stream == BinaryStream
Common: Proto_json > NewTonJson > proto > Text >> Stream == BinaryStream

### 纵向

#### Write

StrDic: 平缓程度， NewTonJson > Text > Proto > Proto_json > Stream > BinaryStream
Common: 平缓程度,  Text > NewTonJson > BinaryStream > Stream > Proto > Proto_json

#### Read

StrDic：平缓程度， BinaryStream == Stream >> Text > NewTonJson > Proto > Proto_json
Common: 平缓程度,  BinaryStream == Stream >> Text > Proto > NewTonJson > Proto_json

## GC

### 横向

#### Write

StrDic：NewTonJosn >> Text > Proto > Ptoto_json > Stream == BinaryStream
Common：Text > NewTonJson >> Proto > BinaryStream > Proto_json > Stream 

#### Read

StrDic：BinaryStream == Stream >> Proto > NewTonJson > Text > Proto_json
Common：BinaryStream == Stream >> Proto > NewTonJson > Text > Proto_json

### 纵向

#### Write

StrDic: 平缓程度， NewTonJson >> Text > Proto > Proto_json > Stream == BinaryStream
Common: 平缓程度,  Text > NewTonJson > BinaryStream > Proto > Proto_json > Stream

#### Read

StrDic：平缓程度， BinaryStream == Stream >> Text > NewTonJson > Proto > Proto_json
Common: 平缓程度,  BinaryStream == Stream >> NewTonJson > Proto > Text > Proto_json

## 文件尺寸

StrDic：Text == BinaryStream > Stream > NewTonJson > Proto > Proto_json; 不过他们差距都非常接近，没有绝对优劣
Common: Text > Stream >  BinaryStream > Proto > NewTonJson > Proto_json

BinaryStream之所以尺寸更大，是因为每个基本类型都是固定的大小；但是 Stream中的，因为数据大小都很小，可能只有一两个字节。

# todo
1. NewtonJson的 序列化操作 所消耗的GC有点离谱，我甚至都怀疑是我后去GC大小的方式有问题。希望有时间能够翻译dll看看
