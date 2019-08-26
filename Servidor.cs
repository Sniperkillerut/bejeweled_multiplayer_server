using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace bejeweled_multiplayer_server
{
    public class Servidor
    {
        public List<Client> clilist = new List<Client>();
        bool sigueListen = true;
        List<Thread> clith = new List<Thread>();
        TcpListener escuchador;
        int puerto;
        public Form1 form;
        public Servidor(int port, Form1 pa)
        {
            puerto = port;
            form = pa;
        }
        public string GetLocalIPv4(NetworkInterfaceType _type)
        {
            string output = "";
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                            
                        }
                    }
                }
            }
            return output;
        }
        public void OpenServer()
        {
            try
            {
                sigueListen = true;
                String tempup= GetLocalIPv4(NetworkInterfaceType.Ethernet);
                //IPAddress ipAd = IPAddress.Parse("127.0.0.1");
                form.set_ip(tempup);
                IPAddress ipAd = IPAddress.Parse(tempup);
                escuchador = new TcpListener(ipAd, puerto);

                escuchador.Start();

                while (sigueListen)
                {
                    Socket temp = escuchador.AcceptSocket();
                    Client temporal=new Client(temp, this);
                    if (!clilist.Contains(temporal))
                    {
                        if (clilist.Count < 10)
                        {//maximo 10 jugadores
                            //TODO revisar que el nombre no este en uso
                            bool existe = false;
                            foreach (Client cl in clilist)
                            {
                                if (cl.name== temporal.name)
                                {
                                    existe = true;
                                }
                            }
                            if (!existe)
                            {
                                clilist.Add(temporal);
                                clith.Add(new Thread(clilist.ElementAt(clilist.Count - 1).escuchar));
                                string name = clilist.ElementAt(clilist.Count - 1).name;
                                form.addclient(name);
                                ASCIIEncoding buffSal = new ASCIIEncoding();
                                //TODO enviar info de size, timer, turnos, colores
                                string msg = "_X001Y_*" + form.size.ToString() + "*" + form.color.ToString()+"*"+form.timer.ToString()+"*"+form.TableToString();
                                temp.Send(buffSal.GetBytes(msg));
                                broadcast("CONN," + clilist.ElementAt(clilist.Count - 1).name);
                                clith.ElementAt(clith.Count - 1).Start();
                            }
                            else
                            {
                                ASCIIEncoding buffSal = new ASCIIEncoding();
                                temp.Send(buffSal.GetBytes("_X001E_"));
                            }
                        }
                        else
                        {
                            ASCIIEncoding buffSal = new ASCIIEncoding();
                            temp.Send(buffSal.GetBytes("_X001X_"));
                            temp.Close();
                        }
                    }
                }
                escuchador.Stop();
            }
            catch (Exception ex)
            {
            }
        }

        public void IsConnect()
        {
            try
            {
                foreach (Client i in clilist)
                {
                    if (!i.IsConnected(i.s))
                    {
                        
                        clith.RemoveAt(clilist.IndexOf(i));
                        form.removeclient(i.name);
                        clilist.Remove(i);
                        broadcast("REM," + i.name);
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
            }
        }
        public void broadcast(string msg)
        {
            try
            {
                ASCIIEncoding buffSal = new ASCIIEncoding();
                foreach (Client i in clilist)
                {
                    i.s.Send(buffSal.GetBytes(msg));
                }
            }
            catch (Exception exc)
            {
            }
        }

        public void TerminServer()
        {
            try
            {
                IsConnect();
                ASCIIEncoding buffSal = new ASCIIEncoding();
                if (clilist.Count != 0)
                {
                    foreach (Client i in clilist)
                    {
                        i.s.Send(buffSal.GetBytes("_X001O_"));
                        i.STOP();
                        i.s.Close();
                    }
                }
                escuchador.Stop();
                sigueListen = false;
            }
            catch (Exception exc)
            {
            }
        }
        public void kick(Client c)
        {
            try
            {
                IsConnect();
                ASCIIEncoding buffSal = new ASCIIEncoding();
                c.s.Send(buffSal.GetBytes("_X001D_"));
                c.STOP();
                c.s.Close();
            }
            catch (Exception exc)
            {
            }
        }

        
        public Client win()
        {
            try
            {
                Client a = clilist.ElementAt(0);
                foreach (Client i in clilist)
                {
                    if (a.score<i.score)
                    {
                        a = i;
                    }
                }
                string msg = "WIN," + a.name;
                broadcast(msg);
                return a;
            }
            catch (Exception exc)
            {
                return null;
            }
        }



    }
}
