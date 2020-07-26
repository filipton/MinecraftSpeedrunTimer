using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
using Path = System.IO.Path;

[Serializable]
public struct FColors
{
	public FColor BackgroundColor;
	public FColor TextColor;

	public FColors(FColor backColor, FColor txtColor)
	{
		BackgroundColor = backColor;
		TextColor = txtColor;
	}
}

[Serializable]
public struct FColor
{
	public byte r;
	public byte g;
	public byte b;

	public FColor(byte R, byte G, byte B)
	{
		r = R;
		g = G;
		b = B;
	}
}

namespace MinecraftTimerV3
{
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

			string jsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "colors.json");
			if (!File.Exists(jsonPath))
			{
				FColors tmpfcolors = new FColors(new FColor(23, 154, 41), new FColor(255, 255, 255));
				string json = JsonConvert.SerializeObject(tmpfcolors);

				File.WriteAllText(jsonPath, json);
			}

			FColors fcolors = JsonConvert.DeserializeObject<FColors>(File.ReadAllText(jsonPath));

			Background = new SolidColorBrush(Color.FromRgb(fcolors.BackgroundColor.r, fcolors.BackgroundColor.g, fcolors.BackgroundColor.b));
			SolidColorBrush sob = new SolidColorBrush(Color.FromRgb(fcolors.TextColor.r, fcolors.TextColor.g, fcolors.TextColor.b));
			rtl.Foreground = sob;
			rtl_tb.Foreground = sob;
			gt.Foreground = sob;
			gt_tb.Foreground = sob;
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
