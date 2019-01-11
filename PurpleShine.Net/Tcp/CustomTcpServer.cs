using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PurpleShine.Core.Expansions;

namespace PurpleShine.Net.Tcp
{
    /// <summary>
    /// TCP Server
    /// </summary>
    public sealed class TcpServer
    {
        private readonly ConcurrentDictionary<CommunicatetManager, string> _clients = new ConcurrentDictionary<CommunicatetManager, string>();
        private readonly CommunicatetFactory _factory;
        private TcpListener _tcpListener;
        private volatile int _status;

        public TcpServer()
        : this (null)
        {
            //
        }

        public TcpServer(CommunicatetFactory factory)
        {
            _factory = factory ?? CommunicatetFactory.Default;
        }

        /// <summary>
        /// 當有Client連線時
        /// </summary>
        public event EventHandler<CommunicatetManager> OnClientAccept;

        /// <summary>
        /// 當前Server usage IP
        /// </summary>
        public IPAddress CurrentIP { get; private set; }

        /// <summary>
        /// 當前Server usage Port
        /// </summary>
        public int CurrentPort { get; private set; }

        /// <summary>
        /// 當異常時, 是否自動重新啟動
        /// </summary>
        public bool AutoRestart { get; set; }

        /// <summary>
        /// 連線中Client數量
        /// </summary>
        public int Count => _clients.Count;

        /// <summary>
        /// 本地綁定伺服器
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="error">綁定失敗時, 回傳錯誤訊息</param>
        /// <returns></returns>
        public bool Listen(IPAddress address, int port, out string error)
        {
            if (Interlocked.CompareExchange(ref _status, Status.Starting, Status.Stopped) != Status.Stopped)
            {
                error = "Server was run at ip " + CurrentIP + ":" + CurrentPort;
                return false;
            }

            try
            {
                CurrentIP = address;
                CurrentPort = port;
                _tcpListener = new TcpListener(address, port);
                _tcpListener.Start();
                _tcpListener.Server.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 1000, 1000), null);
                Thread thread = new Thread(() =>
                {
                    try
                    {
                        TcpClient tmpTcpClient;
                        while (_tcpListener.Server.IsBound)
                        {
                            //建立與客戶端的連線
                            tmpTcpClient = _tcpListener.AcceptTcpClient();
                            tmpTcpClient.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 0, 100), null);
                            if (tmpTcpClient.Client.IsSocketConnected())
                            {
                                //tmpTcpClient.NoDelay = true;
                                Task.Run(() =>
                                {
                                    CommunicatetManager manager = _factory.CreateManager(tmpTcpClient);
                                    _clients.TryAdd(manager, manager.ClientIP);
                                    OnClientAccept?.Invoke(this, manager);
                                    manager.Communicate(); // blocking thread
                                    _clients.TryRemove(manager, out string ip);
                                }).ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        //
                    }
                    finally
                    {
                        _clients.Clear();
                        Interlocked.Exchange(ref _status, Status.Stopped);
                        _tcpListener.Stop();
                        _tcpListener = null;
                    }

                    if (AutoRestart)
                    {
                        while (!SpinWait.SpinUntil(() => Listen(CurrentIP, CurrentPort, out string _error), 500)) ;
                    }
                    else
                    {
                        CurrentPort = -1;
                        CurrentIP = null;
                        OnClientAccept = null;
                    }
                })
                {
                    IsBackground = true
                };
                thread.Start();
                error = null;
                return thread.IsAlive;
            }
            catch (Exception ex)
            {
                Interlocked.Exchange(ref _status, Status.Stopped);
                if (_tcpListener.IsNonNull())
                {
                    _tcpListener.Stop();
                    _tcpListener = null;
                }
                error = ex.Message;
                return false;
            }
        }

        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }

        /// <summary>
        /// 發送訊息給所有Client
        /// </summary>
        /// <param name="packet"></param>
        public void NotifyAll(string packet)
        {
            foreach (var client in _clients.Keys)
            {
                client.SendPacket(packet);
            }
        }

        /// <summary>
        /// 發送訊息給所有Client
        /// </summary>
        /// <param name="message"></param>
        public void NotifyAll(byte[] packet)
        {
            foreach (var client in _clients.Keys)
            {
                client.SendPacket(packet);
            }
        }

        /// <summary>
        /// 關閉Server
        /// </summary>
        public void Close()
        {
            if (Interlocked.CompareExchange(ref _status, Status.Stopped, Status.Starting) == Status.Starting)
            {
                _tcpListener.Stop();
            }
        }

        class Status
        {
            public const int Stopped = 0;
            public const int Starting = 1;
        }
    }

    /// <summary>
    /// 通道管理員建立工廠
    /// </summary>
    public class CommunicatetFactory
    {
        public static readonly CommunicatetFactory Default = new CommunicatetFactory();

        public virtual CommunicatetManager CreateManager(TcpClient client)
        {
            return new CommunicatetManager(client);
        }
    }

    /// <summary>
    /// 通道管理員
    /// </summary>
    public class CommunicatetManager : EventArgs
    {

        private volatile int status = Status.Stopped;

        /// <summary>
        /// 是否處於接收中
        /// 當發生連線錯誤時會變成false
        /// </summary>
        public bool IsReceiving => status == Status.Starting;

        /// <summary>
        /// 附加
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// 當開始接收中
        /// </summary>
        public event EventHandler OnReceiving;

        /// <summary>
        /// 如何接收訊息
        /// </summary>
        public event EventHandler<EventMessage> OnMessage;

        /// <summary>
        /// 當Client離開時
        /// </summary>
        public event EventHandler OnDisconnect;

        /// <summary>
        /// 這個通道的Client
        /// </summary>
        public TcpClient TcpClient { get; private set; }

        /// <summary>
        /// IP
        /// </summary>
        public string ClientIP { get; private set; }

        public CommunicatetManager(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            ClientIP = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
        }

        protected virtual void TouchReciveEvent(string msg)
        {
            OnMessage?.Invoke(this, new EventMessage(msg));
        }

        /// <summary>
        /// 執行TcpClient的Close
        /// </summary>
        public void Close()
        {
            TcpClient.Close();
        }

        /// <summary>
        /// 接收到封包後, 處理的方式
        /// </summary>
        /// <returns></returns>
        protected virtual bool Receive()
        {
            var msg = ReceivePacket();

            if (string.IsNullOrEmpty(msg))
                return false;

            TouchReciveEvent(msg);
            return true;
        }

        /// <summary>
        /// 基本的接收
        /// </summary>
        public void Communicate()
        {
            try
            {
                if (Interlocked.CompareExchange(ref status, Status.Starting, Status.Stopped) == Status.Stopped)
                {
                    OnReceiving?.Invoke(this, Empty);
                    while (TcpClient.Connected && TcpClient.Client.IsSocketConnected() && status == Status.Starting && Receive()) ;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                Interlocked.Exchange(ref status, Status.Stopped);
                TcpClient.Close();
                OnDisconnect?.Invoke(this, Empty);
                OnReceiving = null;
                OnMessage = null;
                OnDisconnect = null;
            }
        }

        /// <summary>
        /// 向連接對象傳送封包
        /// </summary>
        /// <param name="msg">要傳送的訊息</param>
        /// <param name="tmpTcpClient">TcpClient</param>
        public void SendPacket(string msg)
        {
            NetworkStream ns = TcpClient.GetStream();
            if (ns.CanWrite)
            {
                byte[] msgByte = Encoding.Default.GetBytes(msg);
                ns.Write(msgByte, 0, msgByte.Length);
            }
            ns.Flush();
        }

        /// <summary>
        /// 向連接對象傳送封包
        /// </summary>
        /// <param name="msg">要傳送的訊息</param>
        /// <param name="tmpTcpClient">TcpClient</param>
        public void SendPacket(byte[] msg)
        {
            NetworkStream ns = TcpClient.GetStream();
            if (ns.CanWrite)
            {
                ns.Write(msg, 0, msg.Length);
            }
            ns.Flush();
        }

        /// <summary>
        /// 接收封包
        /// </summary>
        /// <param name="tmpTcpClient">TcpClient</param>
        /// <returns>接收到的訊息</returns>
        public string ReceivePacket()
        {
            string receiveMsg = string.Empty;
            byte[] receiveBytes = new byte[TcpClient.ReceiveBufferSize];
            int numberOfBytesRead = 0;
            NetworkStream ns = TcpClient.GetStream();
            if (ns.CanRead)
            {
                do
                {
                    numberOfBytesRead = ns.Read(receiveBytes, 0, TcpClient.ReceiveBufferSize);
                    receiveMsg = Encoding.Default.GetString(receiveBytes, 0, numberOfBytesRead);
                }
                while (ns.DataAvailable);
                ns.Flush();
            }
            return receiveMsg;
        }

        /// <summary>
        /// 接收封包
        /// </summary>
        /// <param name="size">接收大小</param>
        /// <returns>接收到的訊息</returns>
        public string ReceivePacket(int size)
        {
            string receiveMsg = string.Empty;
            byte[] receiveBytes = new byte[size];
            int numberOfBytesRead = 0;
            NetworkStream ns = TcpClient.GetStream();
            if (ns.CanRead)
            {
                numberOfBytesRead = ns.Read(receiveBytes, 0, size);
                receiveMsg = Encoding.Default.GetString(receiveBytes, 0, numberOfBytesRead);
                ns.Flush();
            }
            return receiveMsg;
        }

        /// <summary>
        /// 接收封包
        /// </summary>
        /// <param name="size">接收大小</param>
        /// <returns>接收到的訊息(byte array)</returns>
        public byte[] ReceiveBytePacket(int size)
        {
            string receiveMsg = string.Empty;
            byte[] receiveBytes = new byte[size];
            int numberOfBytesRead = 0;
            NetworkStream ns = TcpClient.GetStream();
            if (ns.CanRead)
            {
                numberOfBytesRead = ns.Read(receiveBytes, 0, size);
                ns.Flush();
            }
            return receiveBytes;
        }

        class Status
        {
            public const int Stopped = 0;
            public const int Starting = 1;
        }
    }

    /// <summary>
    /// 訊息事件
    /// </summary>
    public sealed class EventMessage : EventArgs
    {
        /// <summary>
        /// 接收訊息
        /// </summary>
        public string Message { get; private set; }

        public EventMessage(string message)
        {
            Message = message;
        }

        public static implicit operator string(EventMessage instance)
        {
            return instance.Message;
        }
    }

    /// <summary>
    /// 通道事件
    /// </summary>
    public sealed class EventCommunicatet : EventArgs
    {
        /// <summary>
        /// 通道管理員
        /// </summary>
        public CommunicatetManager Manager { get; private set; }

        public EventCommunicatet(CommunicatetManager manager)
        {
            Manager = manager;
        }

        public static implicit operator CommunicatetManager(EventCommunicatet instance)
        {
            return instance.Manager;
        }
    }

    /// <summary>
    /// 具有通道連接和自動恢復連線的Tcp Client
    /// (為什麼不繼承 TcpClient? 因為原生TcpClient斷線後會被內部呼叫Disposed)
    /// </summary>
    public sealed class CustomTcpClient : IDisposable
    {
        private readonly CommunicatetFactory factory;
        private bool _manualClose, _keepAlive;
        private string _ip;
        private int _port;

        /// <summary>
        /// 當建立連線時所觸發
        /// </summary>
        public event EventHandler<CommunicatetManager> OnConnected;

        /// <summary>
        /// 當連接失敗時
        /// </summary>
        public event EventHandler OnConnectError;

        /// <summary>
        /// 當重新連線成功
        /// </summary>
        public event EventHandler<CommunicatetManager> OnReconnected;

        /// <summary>
        /// 當重新連線開始時
        /// </summary>
        public event EventHandler OnReconnecting;

        /// <summary>
        /// 當重新連線且失敗時
        /// </summary>
        public event EventHandler OnReconnectError;

        /// <summary>
        /// 當TcpClient是否自動重連
        /// </summary>
        public bool AutoReconnect { get; set; }

        /// <summary>
        /// 重連間隔
        /// </summary>
        public int AutoReconnectDelay { get; set; } = 1000;

        /// <summary>
        /// Socket Client
        /// </summary>
        public TcpClient TcpClient { get; set; }

        /// <summary>
        /// 通道管理員
        /// </summary>
        public CommunicatetManager Manager { get; set; }

        /// <summary>
        /// 該Client是否已被處置
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// 當前Client連線是否正常
        /// </summary>
        public bool IsConnected => TcpClient.IsNonNull() && TcpClient.Client.IsNonNull() && TcpClient.Client.Connected && Manager.IsNonNull() && Manager.IsReceiving;

        /// <summary>
        /// 附加
        /// </summary>
        public object Tag { get; set; }

        public CustomTcpClient(bool autoReconnect = false)
        : this(null)
        {
            AutoReconnect = autoReconnect;
        }

        public CustomTcpClient(CommunicatetFactory factory)
        {
            this.factory = factory ?? CommunicatetFactory.Default;
        }

        #region Dispose
        ~CustomTcpClient()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                IsDisposed = true;

                if (disposing)
                {
                    TcpClient.Close();
                }
            }
        }
        #endregion Dispose

        private bool ConnectToServer()
        {
            //建立IPEndPoint
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(_ip), _port);
            TcpClient = new TcpClient();

            //開始連線
            try
            {
                TcpClient.Connect(ipe);
                if (TcpClient.Connected)
                {
                    if (_keepAlive)   // 保持連線
                    {
                        TcpClient.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 1000, 1000), null);
                    }
                    Manager = factory.CreateManager(TcpClient);
                    OnConnected?.Invoke(this, Manager);
                    _manualClose = false;
                    Task.Run(() =>
                    {
                        Manager.Communicate();
                        Manager = null;
                        while (!_manualClose && AutoReconnect)
                        {
                            Thread.Sleep(AutoReconnectDelay);
                            OnReconnecting?.Invoke(this, EventArgs.Empty);
                            if (ConnectToServer())
                            {
                                OnReconnected(this, Manager);
                                break;
                            }
                            OnReconnectError?.Invoke(this, EventArgs.Empty);
                        }
                    }).ConfigureAwait(false);
                    return true;
                }
            }
            catch (Exception)
            {
                TcpClient.Close();
            }
            OnConnectError?.Invoke(this, EventArgs.Empty);
            return false;
        }

        /// <summary>
        /// 連線至伺服器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="manager">若連線成功回傳通道管理員, 否為null</param>
        /// <returns>true 代表連線成功</returns>
        public bool ConnectToServer(string ip, int port, bool keepAlive)
        {
            if (Manager.IsNonNull() && Manager.IsReceiving)
                return false;

            _ip = ip;
            _port = port;
            _keepAlive = keepAlive;
            return ConnectToServer();
        }

        public void Close()
        {
            if (TcpClient.IsNonNull())
            {
                _manualClose = true;
                TcpClient.Close();
            }
        }

        /// <summary>
        /// 保持聯繫
        /// </summary>
        /// <param name="onOff"></param>
        /// <param name="keepAliveTime"></param>
        /// <param name="keepAliveInterval"></param>
        /// <returns></returns>
        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }
    }

    /// <summary>
    /// TCP 錯誤例外
    /// </summary>
    public static class SocketExpansion
    {
        /// <summary>
        /// 檢查通道是否已停止接收
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool IsSocketReadConnected(this Socket socket)
        {
            try
            {
                return !(socket.Poll(-1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException)
            {
                return false;
            }
        }

        /// <summary>
        /// 檢查通道是否連線正常
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        public static bool IsSocketConnected(this Socket socket)
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections().Where(x => x.LocalEndPoint.Equals(socket.LocalEndPoint) && x.RemoteEndPoint.Equals(socket.RemoteEndPoint)).ToArray();

            if (tcpConnections.IsNonNull() && tcpConnections.Length > 0)
            {
                TcpState stateOfConnection = tcpConnections.First().State;
                return stateOfConnection == TcpState.Established;
            }
            return false;
        }
    }
}
