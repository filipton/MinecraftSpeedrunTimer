using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
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
public struct Settings
{
	public FColor BackgroundColor;
	public FColor TextColor;
	public double WindowSizeMultiplier;
	public string MinecraftSavesPath;

	public Settings(FColor backColor, FColor txtColor, double windowSizeMultiplier, string minecraftSavesPath)
	{
		BackgroundColor = backColor;
		TextColor = txtColor;
		WindowSizeMultiplier = windowSizeMultiplier;
		MinecraftSavesPath = minecraftSavesPath;
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

namespace MinecraftSpeedrunTimer
{
	public partial class MainWindow : Window
	{
		public long time = 0;
		public Stopwatch rtlTimer = new Stopwatch();

		public string oldPath = string.Empty;

		public Settings settings;

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

			Title = "MinecraftSpeedrunTimer";

			//---------------------------------------------CHECK FOR UPDATE---------------------------------------------//
			var github = new GitHubClient(new ProductHeaderValue("MinecraftSpeedrunTimerVC"));
			string latestReleaseVersion = github.Repository.Release.GetLatest("filipton", "MinecraftSpeedrunTimer").Result.TagName;

			string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

			if (latestReleaseVersion != version)
			{
				if(MessageBox.Show("Please download latest version! Do you want to go to the latest release page?", $"YOU ARE NOT RUNNING LATEST VERSION! ({version} vs {latestReleaseVersion})", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
				{
					Process.Start("https://github.com/filipton/MinecraftSpeedrunTimer/releases/latest");
					Environment.Exit(0);
				}
			}

			//-------------------------------------------------SETTINGS-------------------------------------------------//
			string jsonPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "settings.json");
			try
			{
				if (!File.Exists(jsonPath))
				{
					Settings tmpsettings = new Settings(new FColor(23, 154, 41), new FColor(255, 255, 255), 1, System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft"), "saves"));
					string json = JsonConvert.SerializeObject(tmpsettings);

					File.WriteAllText(jsonPath, json);
				}

				Settings settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(jsonPath));
				ChangeSettings(settings);

				this.settings = settings;
				Console.WriteLine(this.settings.MinecraftSavesPath);
			}
			catch(Exception e)
			{
				if(e is Newtonsoft.Json.JsonReaderException)
				{
					Settings tmpsettings = new Settings(new FColor(23, 154, 41), new FColor(255, 255, 255), 1, System.IO.Path.Combine(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".minecraft"), "saves"));
					string json = JsonConvert.SerializeObject(tmpsettings);

					File.WriteAllText(jsonPath, json);

					ChangeSettings(tmpsettings);
					settings = tmpsettings;
				}
			}
		}

		void timer_Tick(object sender, EventArgs e)
		{
			try
			{
				string path = settings.MinecraftSavesPath;
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

		void ChangeSettings(Settings settings)
		{
			Background = new SolidColorBrush(Color.FromRgb(settings.BackgroundColor.r, settings.BackgroundColor.g, settings.BackgroundColor.b));
			SolidColorBrush sob = new SolidColorBrush(Color.FromRgb(settings.TextColor.r, settings.TextColor.g, settings.TextColor.b));
			rtl.Foreground = sob;
			rtl_tb.Foreground = sob;
			gt.Foreground = sob;
			gt_tb.Foreground = sob;

			ChangeFormSize(settings.WindowSizeMultiplier);
		}

		void ChangeFormSize(double cnt)
		{
			rtl.Height *= cnt;
			rtl_tb.Height *= cnt;
			gt.Height *= cnt;
			gt_tb.Height *= cnt;

			rtl.Width *= cnt;
			rtl_tb.Width *= cnt;
			gt.Width *= cnt;
			gt_tb.Width *= cnt;

			rtl.FontSize *= cnt;
			rtl_tb.FontSize *= cnt;
			gt.FontSize *= cnt;
			gt_tb.FontSize *= cnt;

			rtl.Margin = new Thickness(10, 10, 0, 0);
			rtl_tb.Margin = new Thickness(10, rtl.Margin.Top + rtl.Height + 5, 0, 0);
			gt.Margin = new Thickness(10, rtl_tb.Margin.Top + rtl_tb.Height + 5, 0, 0);
			gt_tb.Margin = new Thickness(10, gt.Margin.Top + gt.Height + 5, 0, 0);

			Width = 190 * (cnt == 1 ? 1 : cnt - 0.1);
			Height = cnt == 1 ? 170 : (30 + rtl.Height + 5 + rtl_tb.Height + 5 + gt.Height + 5 + gt_tb.Height + 5);
		}
	}
}