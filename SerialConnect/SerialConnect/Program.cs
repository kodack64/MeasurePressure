using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.IO;

namespace SerialConnect {
	class Program {
		static void Main(string[] args) {
			(new Program()).run();
		}
		private SerialPort serial;
		private int count = 0;
		private string tprs = "prs.txt";
		private string ttmp = "tmp.txt";
		private string tatm = "atm.txt";
		public void run(){
			serial = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
			serial.DataReceived += (s, e) => recieved();
			serial.Open();
			Console.ReadKey();
			serial.Close();
		}
		public void recieved() {
			string data = serial.ReadLine();
			Console.WriteLine(data);
			double val = double.Parse(data);
			string file;
			if (count % 3 == 0) {
				file = ttmp;
			} else if (count % 3 == 1) {
				file = tprs;
			} else {
				file = tatm;
			}
			File.AppendAllText(file,val.ToString()+"\n");
			count++;
		}
	}
}
