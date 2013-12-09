using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO.Ports;
using System.IO;
using System.Diagnostics;

namespace SerialConnect {
	class Program {
		static void Main(string[] args) {
			(new Program()).run();
		}
		private int count = 0;
		private string tprs = "prs.txt";
		private string ttmp = "tmp.txt";
		private string tatm = "atm.txt";
		private string comName = "COM3";
		private SerialPort serial = null;
		private Process prsplot = null;
		private Process tmpplot = null;
		public void run() {
			if (!File.Exists(tprs)) {
				File.CreateText(tprs);
			}
			if (!File.Exists(ttmp)) {
				File.CreateText(ttmp);
			}

			try {
				prsplot = new Process();
				tmpplot = new Process();
				serial = new SerialPort(comName, 9600, Parity.None, 8, StopBits.One);
				serial.DataReceived += (s, e) => recieved();
				serial.Open();

				prsplot.StartInfo.FileName = "pgnuplot.exe";
				prsplot.StartInfo.UseShellExecute = false;
				prsplot.StartInfo.RedirectStandardInput = true;
				tmpplot.StartInfo.FileName = "pgnuplot.exe";
				tmpplot.StartInfo.UseShellExecute = false;
				tmpplot.StartInfo.RedirectStandardInput = true;
				prsplot.Start();
				tmpplot.Start();
				prsplot.StandardInput.WriteLine("set xlabel \"time (sec)\"");
				prsplot.StandardInput.WriteLine("set ylabel \"Pressure (Pa)\"");
				tmpplot.StandardInput.WriteLine("set xlabel \"time (sec)\"");
				tmpplot.StandardInput.WriteLine("set ylabel \"Temperature (C)\"");
				prsplot.StandardInput.WriteLine("plot \"" + tprs + "\" w l ti \"data\"");

				while (true) {
					ConsoleKeyInfo keyInfo = Console.ReadKey();
					if (keyInfo.Key == ConsoleKey.Enter) {
						prsplot.StandardInput.WriteLine("plot \"" + tprs + "\" w l ti \"data\"");
						tmpplot.StandardInput.WriteLine("plot \"" + ttmp + "\" w l ti \"data\"");
					} else if (keyInfo.Key == ConsoleKey.Escape) {
						break;
					} else {
						Console.WriteLine("Update - Enter key / Exit - escape key");
					}
				}
			}catch(Exception e){
				Console.WriteLine(e.Message);
				Console.ReadKey();
			}
			if (prsplot != null) {
				prsplot.Close();
				prsplot.Dispose();
			}
			if (tmpplot != null) {
				tmpplot.Close();
				tmpplot.Dispose();
			}
			if (serial != null) {
				serial.Close();
				serial.Dispose();
			}
		}
		public void recieved() {
			string data = serial.ReadLine();
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
