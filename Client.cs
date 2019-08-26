using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bejeweled_multiplayer_server
{
    public class Client
    {
        public Socket s;
        Servidor parent;
        public int score = 0;
        public Client(Socket a, Servidor pa)
        {
            s = a;
            parent = pa;

            string temp = "";
            byte[] buffer = new byte[1024];
            int k = s.Receive(buffer);

            for (int i = 0; i < k; i++)
                temp += Convert.ToChar(buffer[i]);
            //recibe el mensaje, la primera vez es solo el nombre

            name = temp;
        }
        bool Listen = true;
        public string name;
        private static Semaphore sem = new Semaphore(1, 1);
        public void STOP()
        {
            Listen = false;
        }
        public bool IsConnected(Socket socket)
        {
            try
            {
                return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
            }
            catch (SocketException) { return false; }
            catch (Exception) { return false; }
        }
        public void escuchar()
        {
            try
            {
                var t = new System.Threading.Timer(o => parent.kick(this), null, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                while (Listen)
                {
                    byte[] buffer = new byte[1024];
                    int k = s.Receive(buffer);
                    string temp = "";


                    //int startin = 120 - DateTime.Now.Second;
                    //var t = new System.Threading.Timer(o => parent.kick(this), null, startin * 1000, 120000);
                    t.Change(300000, System.Threading.Timeout.Infinite);
                    //t = new System.Threading.Timer(o => parent.kick(this), null, 120 * 1000, System.Threading.Timeout.Infinite);

                    for (int i = 0; i < k; i++)
                        temp += Convert.ToChar(buffer[i]);
                    //recibe el mensaje, debe ser el movimiento hecho (sale del drag/drop)

                    if (temp == "")
                    {
                        //Se ha desconectado
                    }
                    else
                    {
                        //TODO check semaphore working
                        sem.WaitOne();
                        parent.form.move(temp, this);
                        sem.Release();
                    }
                }
            }
            catch (Exception ext)
            {
            }
        }


    }
}
