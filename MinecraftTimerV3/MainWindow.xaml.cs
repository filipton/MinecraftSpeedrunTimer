using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MinecraftTimerV3
{
	/// <summary>
	/// Logika interakcji dla klasy MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public long time = 0;
		public Stopwatch rtlTimer = new Stopwatch();

		public string oldPath = string.Empty;

		public MainWindow()
		{
			using (Process p = Process.GetCurrentProcess())
				p.PriorityClass = ProcessPriorityClass.High;

			InitializeComponent();

			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(1);
			timer.Tick += timer_Tick;
			timer.Start();
			rtlTimer.Start();

			Width = 190;
			Height = 170;
		}

		void timer_Tick(object sender, EventArgs e)
		{
			try
			{
				string path = System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft"), "saves");
				var allsaves = Directory.GetDirectories(path).Select(x => new DirectoryInfo(x));

				DateTime ldt = DateTime.Today;
				DirectoryInfo ldi = new DirectoryInfo(path);

				foreach (DirectoryInfo di in allsaves)
				{
					if (di.LastWriteTime > ldt)
					{
						ldt = di.LastWriteTime;
						ldi = di;
					}
				}

				path = System.IO.Path.Combine(ldi.FullName, "stats");
				string ptf = System.IO.Directory.GetFiles(path, "*.json").First();

				if (oldPath != path)
				{
					oldPath = path;

					rtlTimer.Restart();
				}


				this.rtl_tb.Text = rtlTimer.Elapsed.ToString();


				var dic = JsonConvert.DeserializeObject<JToken>(File.ReadAllText(ptf));
				long ticks = dic.SelectToken("stats").SelectToken("minecraft:custom").SelectToken("minecraft:play_one_minute").ToObject<long>();

				this.gt_tb.Text = TimeSpan.FromSeconds(ticks / 20).ToString();
			}
			catch { }
		}
	}
}
