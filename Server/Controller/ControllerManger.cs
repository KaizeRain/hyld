using System;
using System.Collections.Generic;
using System.Text;
using SocketProto;
using System.Reflection;
namespace Server.Controller
{
    class ControllerManger
    {
        private Server _server;
        private Dictionary<RequestCode,BaseControllers> _controllerDic = new Dictionary<RequestCode, BaseControllers>();
        public ControllerManger(Server server)
        {
            _server = server;
            UserController userController = new UserController();
            FriendController friendController = new FriendController();
            FriendRoomController friendRoomController = new FriendRoomController();
            PingPongController pingPongController = new PingPongController();
            MatchingController matchingController = new MatchingController();
            ClearSenceController clearSenceController = new ClearSenceController();
            _controllerDic.Add(friendRoomController.GetRequestCode,friendRoomController);
            _controllerDic.Add(userController.GetRequestCode, userController);
            _controllerDic.Add(friendController.GetRequestCode, friendController);
            _controllerDic.Add(pingPongController.GetRequestCode, pingPongController);
            _controllerDic.Add(matchingController.GetRequestCode, matchingController);
            _controllerDic.Add(clearSenceController.GetRequestCode, clearSenceController);
        }

        public void CloseClient(Client client,int id)
        {
            foreach (BaseControllers controllers in _controllerDic.Values)
            {
                controllers.CloseClient(client, id);
            }
        }

        public void HandleRequest(MainPack pack,Client client)
        {
            if (_controllerDic.TryGetValue(pack.Requestcode, out BaseControllers controller))
            {
                //根据Requestcode找到对应的Controller
                string methodname = pack.Actioncode.ToString();
                MethodInfo method = controller.GetType().GetMethod(methodname);
                Logging.Debug.Log($"Handle  {pack}\n Controller  {controller} \n method : {methodname}");
                if (method == null)
                {
                    Logging.Debug.Log("没有找到指定事件处理" + pack.Actioncode.ToString());
                    return;
                }
                //调用对应的actioncode方法
                object[] obj = new object[] { _server, client, pack };
                object o = method.Invoke(controller, obj);
                if (o == null) { return; }
                client.Send(o as MainPack);
            }
            else 
            {
                Logging.Debug.Log("未找到对应的事件控制 :"+pack);
            }

        }

    }
}
