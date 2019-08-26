using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Generic;

namespace bejeweled_multiplayer_server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public int size;
        public int color;
        public int timer;
        bool[,] revisado;
        Servidor server;
        Thread serverth;
        string ip;

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            size = Int32.Parse(numericUpDown1.Value.ToString());
            color = Int32.Parse(numericUpDown2.Value.ToString());
            timer = Int32.Parse(numericUpDown3.Value.ToString());
            label4.Text = timer.ToString();
            start();
        }
        private void start()
        {
            tableLayoutPanel2.ColumnCount = size;
            tableLayoutPanel2.RowCount = size;
            tableLayoutPanel2.Padding = Padding.Empty;
            tableLayoutPanel2.Margin = Padding.Empty;
            tableLayoutPanel2.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
            for (int i = 0; i < size * size; i++)
            {
                Button button = new Button();
                button.AllowDrop = true;
                button.Text = "";
                button.TabStop = false;
                button.Padding = Padding.Empty;
                button.Margin = Padding.Empty;
                button.Dock = DockStyle.Fill;
                button.MouseDown += this.button1_MouseDown;
                button.MouseUp += this.button1_MouseUp;
                tableLayoutPanel2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / size));
                tableLayoutPanel2.Controls.Add(button);
                button.DragDrop += this.button1_DragDrop;
                button.DragEnter += this.button1_DragEnter;
                button.BackColor = Color.White;
            }
            fill();
            bool[,] empty = new bool[size, size];
            score2(null);
            while (!IsEmpty(revisado, empty))
            {
                score2(null);
                //score2 llama a erase, y este llama a fill, una vez no hallan mas movimientos sale del while
            }
            start_server();            
        }
        private void button1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.Text))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }
        private void button1_DragDrop(object sender, DragEventArgs e)
        {
            Button a = sender as Button;
            string temp = e.Data.GetData(DataFormats.Text).ToString();
            string d=tableLayoutPanel2.GetPositionFromControl(sender as Control).ToString();
            
            //TODO cliente debe mandar al servidor el movimiento, no debe llamar a move
            move(temp + "-" + d,null);
        }
        private void button1_MouseDown(object sender, MouseEventArgs e)
        {
            Button a = sender as Button;
            a.Text = "S";
            string temp = tableLayoutPanel2.GetPositionFromControl(sender as Control).ToString();
            a.DoDragDrop(temp, DragDropEffects.Move);
        }
        private void button1_MouseUp(object sender, MouseEventArgs e)
        {
            this.label2.Focus();
        }
        private delegate void score2delegate(Client cliente);
        private void score2(Client cliente)
        {
            if (tableLayoutPanel2.InvokeRequired)
            {
                tableLayoutPanel2.Invoke(new score2delegate(score2), cliente);
            }
            else
            {
                revisado = new bool[size, size];
                int temp1;
                int temp2;
                int k;
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        temp1 = tableLayoutPanel2.GetControlFromPosition(i, j).BackColor.ToArgb();
                        if (temp1 != Color.White.ToArgb() && !revisado[i, j])
                        {//si no es blanco y no ha sido revisado
                            if (i + 2 < size)
                            {//para revision vertical, se evitan las ultimas posiciones
                                k = 1;
                                temp2 = tableLayoutPanel2.GetControlFromPosition(i + k, j).BackColor.ToArgb();
                                while (temp1 == temp2)
                                {//mientras que el color sea el mismo
                                    k++;
                                    if (i + k < size)
                                    {//se mueve hacia abajo
                                        temp2 = tableLayoutPanel2.GetControlFromPosition(i + k, j).BackColor.ToArgb();
                                    }
                                    else
                                    {
                                        //break;//tambien podria cambiar temp2=white
                                        temp2 = Color.White.ToArgb();
                                    }
                                }
                                if (k >= 3)
                                {//k=matches
                                    revisado[i, j] = true;
                                    //se marca la posicion inicial como visto
                                    for (int l = 1; l < k; l++)
                                    {//si hay una ficha a la derecha y no hay a la izq, no lo marque como visto para poder ser revisado en el proximo ciclo
                                        if (j - 1 >= 0)
                                        {//se busca la ficha de la izquierda
                                            temp2 = tableLayoutPanel2.GetControlFromPosition(i + l, j - 1).BackColor.ToArgb();
                                        }
                                        else
                                        {//si no hay ficha a la izq, haga temp2!=temp1
                                            temp2 = Color.White.ToArgb();
                                        }
                                        if (temp1 != temp2)
                                        {//si a la izquierda no hay ficha, se debe revisar a la derecha para determinar si se marca como visto
                                            if (j + 1 < size)
                                            {//si es posible verificar a la derecha
                                                temp2 = tableLayoutPanel2.GetControlFromPosition(i + l, j + 1).BackColor.ToArgb();
                                            }
                                            else
                                            {//si no es posible, haga temp1!=temp2
                                                temp2 = Color.White.ToArgb();
                                            }
                                            if (temp1 != temp2)
                                            {//si a la derecha hay ficha, no se marca como visto, para que en el proximo ciclo haga la verificacion completa
                                                revisado[i + l, j] = true;
                                            }
                                        }
                                        else
                                        {//si a la izquierda hay una ficha, ya se verifico, por tanto se puede marcar como visto
                                            revisado[i + l, j] = true;
                                        }
                                    }
                                }
                            }
                            if (j + 2 < size)
                            {//para revision hotizontal, se evitan las ultimas posiciones
                                k = 1;
                                temp2 = tableLayoutPanel2.GetControlFromPosition(i, j + k).BackColor.ToArgb();
                                while (temp1 == temp2)
                                {//mientras que el color sea el mismo
                                    k++;
                                    if (j + k < size)
                                    {//se mueve hacia la derecha
                                        temp2 = tableLayoutPanel2.GetControlFromPosition(i, j + k).BackColor.ToArgb();
                                    }
                                    else
                                    {
                                        //break;//tambien podria cambiar temp2=white
                                        temp2 = Color.White.ToArgb();
                                    }
                                }
                                if (k >= 3)
                                {//k=matches
                                    revisado[i, j] = true;
                                    //se marca la posicion inicial como visto
                                    for (int l = 1; l < k; l++)
                                    {//si hay una ficha abajo y no hay arriba, no lo marque como visto para poder ser revisado en el proximo ciclo
                                        if (i - 1 >= 0)
                                        {//se busca la ficha de arriba
                                            temp2 = tableLayoutPanel2.GetControlFromPosition(i - 1, j + l).BackColor.ToArgb();
                                        }
                                        else
                                        {//si no hay ficha arriba, haga temp2!=temp1
                                            temp2 = Color.White.ToArgb();
                                        }
                                        if (temp1 != temp2)
                                        {//si arriba no hay ficha, se debe revisar aabajo para determinar si se marca como visto
                                            if (i + 1 < size)
                                            {//si es posible verificar abajo
                                                temp2 = tableLayoutPanel2.GetControlFromPosition(i + 1, j + l).BackColor.ToArgb();
                                            }
                                            else
                                            {//si no hay ficha abajo, haga temp2!=temp1
                                                temp2 = Color.White.ToArgb();
                                            }
                                            if (temp1 != temp2)
                                            {//si a la derecha hay ficha, no se marca como visto, para que en el proximo ciclo haga la verificacion completa
                                                revisado[i, j + l] = true;
                                            }
                                        }
                                        else
                                        {//si arriba hay una ficha, ya se verificara, por tanto se puede marcar como visto
                                            revisado[i, j + l] = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                erase(cliente);
            }
            
            
        }
        private delegate void erasedelegate(Client cliente);
        private void erase(Client cliente)
        {
            if (tableLayoutPanel2.InvokeRequired)
            {
                tableLayoutPanel2.Invoke(new erasedelegate(erase), cliente);
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        if (revisado[i, j])
                        {
                            tableLayoutPanel2.GetControlFromPosition(i, j).BackColor = Color.White;
                            // asignar score al jugador correspondiente
                            if (cliente != null)
                            {
                                cliente.score += 10;
                            }
                        }
                    }
                }
                fill();
            }
            
        }
        private delegate void filldelegate();
        private void fill()
        {
            if (tableLayoutPanel2.InvokeRequired)
            {
                tableLayoutPanel2.Invoke(new filldelegate(fill));
            }
            else
            {
                Color temp;
                Color[] colores = { Color.Red, Color.Blue, Color.Yellow, Color.Green, Color.Purple, Color.Brown, Color.Aqua, Color.Orange, Color.Fuchsia, Color.LimeGreen };
                Random rand = new Random();
                int k = 0;
                int k2 = 0;
                for (int i = 0; i < size; i++)
                {
                    for (int j = size - 1; j >= 0; j--)
                    {
                        temp = tableLayoutPanel2.GetControlFromPosition(i, j).BackColor;
                        if (temp.Equals(Color.White))
                        {
                            k = k2 = 0;
                            while (j - k > 0)
                            {//se busca un NO blanco y se "baja" a la posicion del siguiente blanco
                                k++;
                                if (!tableLayoutPanel2.GetControlFromPosition(i, j - k).BackColor.Equals(Color.White))
                                {
                                    tableLayoutPanel2.GetControlFromPosition(i, j - k2).BackColor = tableLayoutPanel2.GetControlFromPosition(i, j - k).BackColor;
                                    tableLayoutPanel2.GetControlFromPosition(i, j - k).BackColor = Color.White;
                                    //tableLayoutPanel2.Refresh();
                                    k2++;
                                }
                            }
                            //ahora j-k2 debe contener la posicion del blanco mas alto
                            while (j - k2 >= 0)
                            {//se llena de aleatorios
                                tableLayoutPanel2.GetControlFromPosition(i, j - k2).BackColor = colores[rand.Next(color)];
                                //tableLayoutPanel2.Refresh();
                                k2++;
                            }
                        }
                    }
                }
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            server.IsConnect();
            timer--;
            label4.Text = timer.ToString();
            if (timer<=0)
            {
                Client a = server.win();
                if (a!=null)
                {
                    MessageBox.Show("El ganador es: " + a.name + " con un puntaje de: " + a.score+" puntos!");
                }
                timer1.Stop();
                timer1.Enabled = false;
            } 
        }
        private void start_server()
        {
            try
            {
                server = new Servidor(8000,this);
                serverth = new Thread(server.OpenServer);
                serverth.Start();
                
                timer1.Enabled = true;
                timer1.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public delegate void movedelegate(string dir, Client cliente);
        public void move(string dir, Client cliente)
        {
            if (tableLayoutPanel2.InvokeRequired)
            {
                object[] arg= { dir, cliente };
                tableLayoutPanel2.Invoke(new movedelegate(move),arg);
            }
            else
            {
                string[] pos = dir.Split('-');
                string[] a = pos[0].Split(',');
                string[] b = pos[1].Split(',');
                Button ba = tableLayoutPanel2.GetControlFromPosition(Int32.Parse(a[0]), Int32.Parse(a[1])) as Button;
                Button bb = tableLayoutPanel2.GetControlFromPosition(Int32.Parse(b[0]), Int32.Parse(b[1])) as Button;
                Color col = ba.BackColor;
                ba.BackColor = bb.BackColor;
                bb.BackColor = col;
                ba.Text = "";

                bool[,] empty = new bool[size, size];
                score2(cliente);
                while (!IsEmpty(revisado, empty))
                {
                    score2(cliente);
                    //score2 llama a erase, y este llama a fill, una vez no hallan mas movimientos sale del while
                    tableLayoutPanel2.Refresh();
                }

                //cambiar_score(cliente);
                refresh_score(server.clilist);

                //ahora se puede enviar la tabla completa
                if (cliente != null)
                {
                    server.broadcast(TableToString() + "*" + cliente.name + "*" + cliente.score);
                }
            }
            

        }
        public delegate void cambiar_scoredelegate(Client cliente);
        public void cambiar_score(Client cliente)
        {
            if (tableLayoutPanel1.InvokeRequired)
            {
                tableLayoutPanel1.Invoke(new cambiar_scoredelegate(cambiar_score), cliente);
            }
            else
            {
                if (cliente != null)
                {
                    //se busca el label con el score (nombre) del cliente
                    int index = 0;
                    foreach (Label item in tableLayoutPanel1.Controls)
                    {
                        if (item.Text == cliente.name)
                        {
                            index = tableLayoutPanel1.Controls.IndexOf(item);
                            break;
                        }
                    }
                    index = index / 2;
                    //aqui se cambia el score del label por el que tiene el cliente
                    tableLayoutPanel1.GetControlFromPosition(1, index).Text = cliente.score.ToString();
                }
            }
            
        }

        public delegate void refresh_scoredelegate(List<Client> clients);
        public void refresh_score(List<Client> clients)
        {
            if (tableLayoutPanel1.InvokeRequired)
            {
                tableLayoutPanel1.Invoke(new refresh_scoredelegate(refresh_score), clients);
            }
            else
            {
                //primero se borran todos los labels del score
                while (tableLayoutPanel1.Controls.Count > 0)
                {
                    tableLayoutPanel1.Controls.RemoveAt(0);
                }
                //ahora se deben agregar los titulos de Name y Score
                Label Name = new Label();
                Name.Text = "Name";
                Label Score = new Label();
                Score.Text = "Score";
                Score.TextAlign=Name.TextAlign = ContentAlignment.MiddleCenter;
                Score.Font=Name.Font=new Font(Name.Font, FontStyle.Bold);

                tableLayoutPanel1.Controls.Add(Name);
                tableLayoutPanel1.Controls.Add(Score);
                //ahora se agregan los clientes

                String msg = "_SCORE_:";
                foreach (Client client in clients)
                {
                    Name = new Label();
                    Name.Text = client.name;
                    Score = new Label();
                    Score.Text = client.score.ToString();
                    tableLayoutPanel1.Controls.Add(Name);
                    tableLayoutPanel1.Controls.Add(Score);


                    if (clients.IndexOf(client) == 0)
                    {
                        msg += client.name + "," + client.score.ToString();
                    }
                    else
                    {
                        msg += ";" + client.name + "," + client.score.ToString();
                    }
                }

                //se debe enviar la lista completa de scores a todos los clientes
                server.broadcast(msg);

            }

        }


        public delegate void addclientdelegate(string name);
        public void addclient(string name)
        {
            if (this.tableLayoutPanel1.InvokeRequired)
            {
                this.tableLayoutPanel1.Invoke(new addclientdelegate(this.addclient), name);
            }
            else
            {
                //TODO comprobar que no exista ya
                int index = -1;
                //se busca si exite un label con el nombre del nuevo cliente
                foreach (Label item in tableLayoutPanel1.Controls)
                {
                    if (item.Text == name)
                    {
                        index = tableLayoutPanel1.Controls.IndexOf(item);
                        break;
                    }
                }
                if (index == -1)
                {//entonces no existe
                    Label a = new Label();
                    a.Text = name;
                    Label b = new Label();
                    b.Text = "0";
                    tableLayoutPanel1.Controls.Add(a);
                    tableLayoutPanel1.Controls.Add(b);
                }
            }
            
        }

        public delegate void removeclientdelegate(string name);
        public void removeclient(string name)
        {
            if (this.tableLayoutPanel1.InvokeRequired)
            {
                this.tableLayoutPanel1.Invoke(new removeclientdelegate(this.removeclient), name);
            }
            else
            {
                int index = 0;
                //se busca el label con el nombre del cliente
                foreach (Label item in tableLayoutPanel1.Controls)
                {
                    if (item.Text == name)
                    {
                        index = tableLayoutPanel1.Controls.IndexOf(item);
                        break;
                    }
                }
                if (index != 2)
                {
                    index = index / 2;
                }
                //se remueven los label de nombre y de score
                tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(index, 1));
                tableLayoutPanel1.Controls.Remove(tableLayoutPanel1.GetControlFromPosition(index, 0));
            }
        }

        private bool IsEmpty(bool[,] a, bool[,] b)
        {
            if (a.Length == b.Length)
            {
                for (int i = 0; i < Math.Sqrt(a.Length); i++)
                {
                    for (int j = 0; j < Math.Sqrt(a.Length); j++)
                    {
                        if (a[i, j] != b[i, j])
                        {
                            return false;  // If one is not equal, the two arrays differ
                        }
                    }
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        public delegate string TableToStringdelegate();
        string msg;
        public string TableToString()
        {
            msg = "";
            if (this.tableLayoutPanel2.InvokeRequired)
            {
                this.tableLayoutPanel2.Invoke(new TableToStringdelegate(this.TableToString));
            }
            else
            {
                Button but;
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        but = tableLayoutPanel2.GetControlFromPosition(i,j) as Button;
                        msg += "," + but.BackColor.ToArgb().ToString();
                    }
                }
            }
            return msg;
        }

        public delegate void llenardelegate(string msg);
        public void llenar(string msg)
        {
            if (this.tableLayoutPanel2.InvokeRequired)
            {
                this.tableLayoutPanel2.Invoke(new llenardelegate(this.llenar), msg);
            }
            else
            {
                Button but;
                msg = msg.Substring(1);
                string[] array = msg.Split(',');
                Color col;
                int pos = 0;
                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        but = tableLayoutPanel2.GetControlFromPosition(i, j) as Button;
                        pos = j % size + (i * size);
                        col = Color.FromArgb(Int32.Parse(array[pos]));
                        but.BackColor = col;
                    }
                }
            }

        }

        public delegate void set_ipdelegate(string ad);
        public void set_ip(string ad)
        {
            if (this.label7.InvokeRequired)
            {
                this.label7.Invoke(new set_ipdelegate(this.set_ip), ad);
            }
            else
            {
                ip = ad;
                label7.Text = ip;
                label7.Refresh();
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (server!=null)
            {
                server.TerminServer();
            }
            timer1.Stop();
            timer1.Enabled = false;
        }
    }

}

