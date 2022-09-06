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

# 说明

下述统计只是1次的最终结果，并不是多次取平均，所以我觉得不严谨。但是大致也能说明一些问题。

## 耗时 (ms)




## GC (MB)



## 文件大小 (KB)



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
