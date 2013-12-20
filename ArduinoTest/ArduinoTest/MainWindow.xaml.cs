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

namespace ArduinoTest {

	public class TextC {
		string Text;
	}

	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window {
		public MainWindow() {
			InitializeComponent();

			MyText.Text = "Hello World";
			MyButton.Content = "Start";
		}

		SerialPort sp = null;
		string str;

		private void ButtonIsClicked(object sender, RoutedEventArgs e) {
			if (sp == null) {
				sp = new SerialPort("COM4", 9600);
				sp.Open();
				sp.DataReceived += (s,ev) => recieve();
			}
		}
		private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) {
		}
		private void recieve() {
			try {
				str = sp.ReadLine();
				Action act = new Action(() => MyText.Text += str);
				MyText.Dispatcher.InvokeAsync(act);
			}catch(Exception){
			}
		}

		private void CloseButtonClicked(object sender, RoutedEventArgs e) {
			if (sp != null) {
				sp.Close();
				sp.Dispose();
				sp = null;
			}
		}

		private void Send(object sender, RoutedEventArgs e) {
			if (sp != null) {
				sp.WriteLine(SendText.Text);
			}
		}
	}
}
