#hyld
## 游戏启动流程：
1.cmd输入  net start mysql  开启数据库mysql
2.打开hyld.sql，执行代码生成数据库。并登入数据库
3.在Server/Program.cs  中的ServerConfig.DOMConectStr 更改你的数据库信息 如：ServerConfig.DOMConectStr =  "database=hyld;data source=localhost;user=root;password=505258140;pooling=false;charset=utf8;port=1314;SSL Mode=none";
4.启动服务器-->Server.sln  直接运行脚本就行
5.启动客户端-->Client\Assets\Scenes\HYLDLogon 这个是入口场景  可以开unity测试，也可以unity打包之后测试。

## 帧同步流程：
服务器：
1.服务器上每个比赛对象，都有一个成员的freamid，保存了当前比赛下一帧要进入的id
2.服务器上定义一个数据结构，dic_match_frames,用来保存我们所有玩家的每帧的操作。(录像回放，断线重连，不同步的情况，有无作弊，udp丢包时序问题，我们要补发给客户端的帧保存起来)
3.dic_next_frame_opts 每帧服务器将采集来的客户端的操作都存放在这里；(<battleid,opt>)
4.服务器开启一个线程Thread_SendFrameData去等待所有玩家准备ok(收到第一帧操作)，收到第一帧之后游戏正式开始，之后每隔66ms(15FPS)触发一次sendFrameDate
5.触发一个帧操作，保存当前的帧操作到dic_match_frames
6.遍历每个玩家给每个玩家发送帧操作。
7.发送”服务器认为“这个玩家还没同步的帧，每个玩家对象用sync_frameid记录的客户端已经同步到了多少帧了
	从sync_frameid+1，发送到最新的帧。--》(解决udp丢包和时序问题，补发帧)
8.采用udp将数据包发送出去 可能会发送补发的帧
9.服务器进入下一帧self.frameid++
10.服务器清空上一帧操作next_frame_opt.allplayeroperation.clear()


客户端：
11.客户端通过网络收到帧同步的数据包以后，OnLogicUpdate
12.每个客户端，也都会有一个sync_frameID记录当前客户端真正已经同步到哪个帧了
13.如果收到的帧id<sync_frameID  ,直接丢弃这个帧。
	a:为什么会出现这个情况。 因为udp会有先发后到，后发先到。
	b:为什么我们没有收到99帧，就开始处理100帧，能同步吗？[如果99帧没有处理，服务器在发100帧的时候会补发99帧]
14.如果上一帧的操作不为空，我们处理下一帧之前，一定要先同步一下上一帧的结果。
	客户端A: |----|---66.3--|----|
	客户端B: |----|---66.1--|----|
	每个客户端收到数据包的时间不一样。所以在帧与帧之间会发生时间的差异导致位置不同步。
	所有都用66ms迭代出新的位置和结果。统一都以66ms来迭代。
	处理下一帧之前每帧都同步；==》同样的输入--》同样的输出
15.跳帧：快速同步过时的帧完直到最新的一帧.
16.控制客户端根据操作来更新逻辑推进。
17.SendOperation:采集自己的操作，上报给服务器，“你认为”的下一帧，next_frame_id=this.sync_framed+1,发送给服务器
	(这个是  this.InvokeRepeating("Send_BattleReady", 0.5f, 0.2f);)
服务器：
18.服务器收到数据。更新服务器的sync_framed;
	比如：服务器[发送99帧，sync_frameid=98 ]--》客户端处理99帧--》上报100帧操作--》服务器收到100帧的操作(说明前99帧已经处理完了更新sync_frameid=99)--》服务器发送100帧数据
19.如果收到玩家操作帧id不等于马上要触发的帧id，说明玩家过时的操作。
	比如：客户端发送99帧--》服务器的sync_frameid=100 > 99所以直接丢弃。
20.把这个操作插入到next_frame_opt。等待下一帧处理。GOTO: 逻辑5


如何克服时序丢包？
客户端丢包：因为可以(13)服务器会补发丢到或者没有到的帧。
服务端丢包：下一帧马上会处理。


【有人想共同维护这个项目的，有人想提pr的可以直接联系 qq：505258140】