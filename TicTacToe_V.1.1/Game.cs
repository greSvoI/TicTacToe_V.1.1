using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe_V._1._1
{
	public partial class Game : Form
	{
		private char Player;
		private char Opponent;

		IPAddress iPAddress = IPAddress.Parse("127.0.0.1");

		public TcpListener tcpListener;

		TcpClient tcpClient = new TcpClient();
		NetworkStream networkStream;

		public Game()
		{
			InitializeComponent();
			ButtonDisabled();
			foreach(Control control in Controls)
				if(control is Button)
					if(control.Name.Length<3)
				control.Click += Control_Click;
		}
		//private void GameStream()
		//{
		//	if (Chek()) return;
		//	ButtonDisabled();
		//	this.textBox1.Text = "Ход противника";
		//	GetMsg();
		//	this.textBox1.Text = "Ваш ход";
		//	if (!Chek())
		//		ButtonEnabled();
		//}
		private void GetMsg()
		{
			try
			{
				while (true)
				{
					byte[] getData = new byte[64];
					StringBuilder stringBuilder = new StringBuilder();
					int size = 0;
					do
					{
						size = networkStream.Read(getData, 0, getData.Length);
						stringBuilder.Append(Encoding.UTF8.GetString(getData, 0, size));

					} while (networkStream.DataAvailable);

					string temp = stringBuilder.ToString();
					if (temp == "reload")
						//buttonReload_Click(null,EventArgs.Empty);
						foreach (Control control in Controls)
							if (control is Button && control.Name.Length < 3)
								control.Invoke(new Action(()=> { control.Text = ""; }));

					foreach (Control control in Controls)
						if (control is Button && control.Name.Length < 3)
							if (control.Name == temp)
								control.Invoke(new Action(() =>
								{
									control.Text = Opponent.ToString();
									textBox1.Text = "Ваш ход";
									if (!Chek())
										ButtonEnabled();
								}));
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + " GetMsg");
				tcpListener?.Stop();
				tcpClient?.Close();
				networkStream?.Close();
			}
		}
		private void Control_Click(object sender, System.EventArgs e)
		{
			Button button = (Button)sender;
			foreach (Control control in Controls)
				if (control is Button)
					if (control.Name == button.Name)
					{
						control.Text = Player.ToString(); 
						this.textBox1.Text = "Ход противника";
					}

			byte[] data = Encoding.UTF8.GetBytes(button.Name);
			networkStream.Write(data, 0, data.Length);

			if (Chek()) return;
			ButtonDisabled();
		}

		private void Game_FormClosing(object sender, FormClosingEventArgs e)
		{
			networkStream?.Close();
			tcpClient?.Close();
		}
		private void ButtonDisabled()
		{
			
			foreach (Control control in Controls)
				if (control is Button)
					if (control.Name.Length < 3)
						control.Enabled = false;
		}
		private void ButtonEnabled()
		{
				foreach (Control control in Controls)
				if (control is Button)
					if (control.Name.Length < 3 && control.Text == "")
						control.Enabled = true;
		}
		private async void Connect()
		{
			await Task.Run(new Action(() => tcpClient = tcpListener.AcceptTcpClient()));
			//Task.WaitAll();
			networkStream = tcpClient.GetStream();
			textBox1.Text = "Ваш ход";
			ButtonEnabled();
			Thread thread = new Thread(new ThreadStart(GetMsg));
			thread.Start();
		}
		private void buttonCreate_Click(object sender, System.EventArgs e)
		{
			Player = 'X';
			Opponent = 'O';
			buttonReload.Enabled = true;
			try
			{
				if (textBoxIP.Text == "")
				{
					tcpListener = new TcpListener(iPAddress, 8000);
					tcpListener.Start();
					Connect();
					//Task.WaitAll(Task.Run(new Action(() => tcpClient = tcpListener.AcceptTcpClient())));
					//networkStream = tcpClient.GetStream();
					//textBox1.Text = "Ваш ход";
					//ButtonEnabled();
					//Thread thread = new Thread(new ThreadStart(GetMsg));
					//thread.Start();
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message + " buttonCreate_Click");
				tcpListener?.Stop();
				tcpClient?.Close();
				networkStream?.Close();
			}

			
		}
		private void buttonConnect_Click(object sender, EventArgs e)
		{
			buttonReload.Enabled = false;
			Player = 'O';
			Opponent = 'X';
			try
			{
				tcpClient = new TcpClient();
				tcpClient.Connect("127.0.0.1", 8000);
				networkStream = tcpClient.GetStream();
				Thread thread = new Thread(new ThreadStart(GetMsg));
				thread.Start();
			}
			catch (Exception ex)
			{
				networkStream?.Close();
				tcpClient?.Close();
				MessageBox.Show(ex.Message+ " buttonConnect_Click");
			}
		}
		private bool Chek()
		{
			//Горизонталь
			if (B1.Text == B2.Text && B2.Text == B3.Text && B3.Text != "")
			{
				Win(B1);
				return true;
			}
			else if (B4.Text == B5.Text && B5.Text == B6.Text && B6.Text != "")
			{
				Win(B4);
				return true;
			}
			else if (B7.Text == B8.Text && B8.Text == B9.Text && B9.Text != "")
			{
				Win(B7);
				return true;
			}
			//Диагональ
			else if (B1.Text == B5.Text && B5.Text == B9.Text && B9.Text != "")
			{
				Win(B1);
				return true;
			}
			else if (B3.Text == B5.Text && B5.Text == B7.Text && B7.Text != "")
			{
				Win(B3);
				return true;
			}
			//Вертикаль
			else if (B1.Text == B4.Text && B4.Text == B7.Text && B7.Text != "")
			{
				Win(B1);
				return true;
			}
			else if (B2.Text == B5.Text && B5.Text == B8.Text && B8.Text != "")
			{
				Win(B2);
				return true;
			}
			else if (B3.Text == B6.Text && B6.Text == B9.Text && B9.Text != "")
			{
				Win(B3);
				return true;
			}
			int flag = 0;
			foreach (Control item in Controls)
				if (item is Button&&item.Name.Length<3)
					if (item.Text != "")
						flag++;
			if (flag == 9) { MessageBox.Show("Ничья"); return true; }


			return false;
		}
		private void Win(Button button)
		{
			if (button.Text[0] == Player)
				MessageBox.Show("Вы победили");
			else MessageBox.Show("Вы проиграли");
		}

		private void buttonReload_Click(object sender, EventArgs e)
		{
			foreach (Control control in Controls)
				if (control is Button && control.Name.Length < 3)
					control.Text = "";
			byte[] data = Encoding.UTF8.GetBytes("reload");
			networkStream.Write(data, 0, data.Length);
		}
	}
}
