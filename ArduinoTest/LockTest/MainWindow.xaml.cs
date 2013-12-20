using System;
using System.Collections.Generic;
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
using System.IO.Ports;
using System.IO;

namespace LockTest {
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();
			BStop.IsEnabled = false;
			BLock.IsEnabled = false;
			BUnLock.IsEnabled = false;
		}

		SerialPort sp=null;
		private void Start(object sender, RoutedEventArgs e) {
			if(sp==null)sp = new SerialPort("COM4",9600);
			sp.Open();
			sp.DataReceived += (m, n) => recieve();
			BStart.IsEnabled = false;
			BStop.IsEnabled = true;
			BLock.IsEnabled = true;
			BUnLock.IsEnabled = true;
		}
		private void Stop(object sender, RoutedEventArgs e) {
			sp.Close();
			sp.Dispose();
			sp = null;
			BStart.IsEnabled = true;
			BStop.IsEnabled = false;
			BLock.IsEnabled = false;
			BUnLock.IsEnabled = false;
		}
		private void Lock(object sender, RoutedEventArgs e) {
			sp.Write("l");
		}
		private void UnLock(object sender, RoutedEventArgs e) {
			sp.Write("u");
		}
		double time=0;
		private void recieve() {
			try {
				string str = sp.ReadLine();
				string[] strs = str.Split(':');
				if (strs.Count() >= 2) {
					if (strs[0].Equals("Time")) {
						time = double.Parse(strs[1]);
					}
					if (strs[0].Equals("Current")) {
						File.AppendAllText("current.txt", time.ToString()+" "+strs[1]+"\n");
					}
					if (strs[0].Equals("Drive")) {
						File.AppendAllText("drive.txt", time.ToString() + " " + strs[1] + "\n");
					}
					if (strs[0].Equals("Feed")) {
						File.AppendAllText("feed.txt", time.ToString() + " " + strs[1] + "\n");
					}
				}
			}catch(Exception){
			}
		}
	}
}
