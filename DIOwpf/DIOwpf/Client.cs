using System;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

namespace DIOwpf
{
    public class Client
    {
        public bool IsConnected { get; private set; } = false;
        private bool registerMode;
        public bool isNormClosing = false;

        TcpClient client;
        public NetworkStream netStream;
        public SslStream ssl;
        public BinaryReader bReader;
        public BinaryWriter bWriter;

        // Change to my IP
        public string Server { get { return "127.0.0.1"; } }
        public int Port { get { return 6666; } }

        public bool IsLoggedIn { get; private set; }
        public string UserName { get; private set; }
        public string UserPassword { get; private set; }


        // New thread to connect
        private void ConnectToServer(string user, string password, bool register)
        {
            try
            {
                if (!IsConnected)
                {
                    IsConnected = true;
                    UserName = user;
                    UserPassword = password;
                    registerMode = register;

                    CreateConnection();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        // Connection to server
        private void CreateConnection()
        {
            try
            {
                client = new TcpClient(Server, Port);
                netStream = client.GetStream();
                ssl = new SslStream(netStream, false, new RemoteCertificateValidationCallback(ValidateCert));
                ssl.AuthenticateAsClient("DIOServer");

                bReader = new BinaryReader(ssl, Encoding.UTF8);
                bWriter = new BinaryWriter(ssl, Encoding.UTF8);

                bWriter.Write(Constants.ShakeHands);
                bWriter.Write(registerMode ? Constants.Register : Constants.Login);
                bWriter.Write(UserName);
                bWriter.Write(UserPassword);
                bWriter.Flush();
                Thread receiveThread = new Thread(new ThreadStart(Receiver));
                receiveThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (IsConnected)
                    CloseConnection();
            }
        }


        // Authorized access
        public void Login(string user, string password)
        {
            ConnectToServer(user, password, false);
        }



        // Unauthorized access
        public void Register(string user, string password)
        {
            ConnectToServer(user, password, true);
        }


        public void SendMessage(string to, string msg)
        {
            if (IsConnected)
            {
                bWriter.Write(Constants.SendMessage);
                bWriter.Write(to);
                bWriter.Write(msg);
                bWriter.Flush();
            }
        }


        public void SendFile(string to, string fileName)
        {
            if (IsConnected)
            {
                byte[] buffer = new byte[2048];
                FileStream file = File.OpenRead(fileName);
                int fileSize = (int)file.Length;
                bWriter.Write(Constants.SendFile);
                bWriter.Write(to);
                bWriter.Write(fileName.Remove(0, fileName.LastIndexOf("\\") + 1));
                bWriter.Write(fileSize);

                while (fileSize > 0)
                {
                    int count = file.Read(buffer, 0, buffer.Length);
                    bWriter.Write(buffer, 0, count);
                    fileSize -= count;
                }

                
                bWriter.Flush();
                file.Close();
            }
        }


        // Receive all packages
        private void Receiver()
        {
            try
            {
                int hello = bReader.ReadInt32();

                if (hello == Constants.ShakeHands) // First handshake 
                {
                    CheckHandshake();
                }
                else
                    if (IsConnected)
                        CloseConnection();

                while (client.Connected)
                {
                    try
                    {
                        byte type = bReader.ReadByte();  // Receiving the type of the package

                        switch (type)
                        {
                            case Constants.UserAvailable:
                                CheckUserAvailable();
                                break;
                            case Constants.ReceivedMessage:
                                CheckMessageReceived();
                                break;
                            case Constants.ReceivedFile:
                                CheckFileRecived();
                                break;
                            default:
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        if(!isNormClosing)
                            MessageBox.Show(ex.Message);
                    }

                }

                IsLoggedIn = false;
            }
            finally
            {
                if (IsConnected)
                    CloseConnection();
            }
        }


        // Check handshake
        private void CheckHandshake()
        {
            byte ans = bReader.ReadByte();
            if (ans == Constants.ItsOK)
            {
                if (registerMode)
                    OnRegisterOK();
                OnLoginOK();
                IsLoggedIn = true;
            }
            else
            {
                MessageErrorEventArgs err = new MessageErrorEventArgs((MessageError)ans);
                if (registerMode)
                    OnRegisterFailed(err);
                else
                    OnLoginFailed(err);
            }
        }


        // Is user connected
        private void CheckUserAvailable()
        {
            string user = bReader.ReadString();
            bool isAvail = bReader.ReadBoolean();
            OnUserAvailable(new MessageAvailEventArgs(user, isAvail));
        }


        private void CheckMessageReceived()
        {
            string from = bReader.ReadString();
            string msg = bReader.ReadString();
            OnMessageReceived(new MessageReceivedEventArgs(from, msg));
        }


        private void CheckFileRecived()
        {
            string from = bReader.ReadString();
            string fileName = bReader.ReadString();
            int size = bReader.ReadInt32();
            byte[] buffer = new byte[2048];
            FileStream file = File.Create("Temp" + fileName);
            int len = size;

            while (len > 0)
            {
                int count = buffer.Length > len ? len : buffer.Length;
                buffer = bReader.ReadBytes(count);
                file.Write(buffer, 0, count);
                len -= count;
            }

            file.Flush();
            file.Close();

            OnFileReceived(new FileReceivedEventArgs(from, fileName, size));
        }

        // If connected
        public void IsAvailable(string user)
        {
            if (IsConnected)
            {
                bWriter.Write(Constants.UserAvailable);
                bWriter.Write(user);
                bWriter.Flush();
            }
        }


        public void Disconnect()
        {
            if (IsConnected)
                CloseConnection();
        }


        private void CloseConnection()
        {
            IsConnected = false;
            bReader.Close();
            bWriter.Close();
            ssl.Close();
            netStream.Close();
            client.Close();
            OnDisconnected();
        }


        // Certificate ssl
        public static bool ValidateCert(object sender, X509Certificate certificate,
              X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Allow untrusted certificates.
        }


        // Events
        public event EventHandler LoginOK;
        public event EventHandler RegisterOK;
        public event MessageErrorEventHandler LoginFailed;
        public event MessageErrorEventHandler RegisterFailed;
        public event EventHandler Disconnected;
        public event MessageAvailEventHandler UserAvailable;
        public event MessageReceivedEventHandler MessageReceived;
        public event FileReceivedEventHandler FileReceived;



        virtual protected void OnLoginOK()
        {
            LoginOK?.Invoke(this, EventArgs.Empty);
        }


        virtual protected void OnRegisterOK()
        {
            RegisterOK?.Invoke(this, EventArgs.Empty);
        }


        virtual protected void OnLoginFailed(MessageErrorEventArgs e)
        {
            LoginFailed?.Invoke(this, e);
        }


        virtual protected void OnRegisterFailed(MessageErrorEventArgs e)
        {
            RegisterFailed?.Invoke(this, e);
        }


        virtual protected void OnUserAvailable(MessageAvailEventArgs e)
        {
            UserAvailable?.Invoke(this, e);
        }


        virtual protected void OnMessageReceived(MessageReceivedEventArgs e)
        {
            MessageReceived?.Invoke(this, e);
        }


        virtual protected void OnFileReceived(FileReceivedEventArgs e)
        {
            FileReceived?.Invoke(this, e);
        }


        virtual protected void OnDisconnected()
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

    }
}
