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
		private const string datanamePressure = "pressure";
		private const string datanameTemperature = "temperature";
		private const string filenameConfig = "config.txt";
		private const int sleepTime = 100;
		private const double temperatureResolution = 0.05;
		private const double pressureResolution = 0.5;

		private string comName = "COM4";
		private double plotTime = 10;
		private SerialPort serial = null;
		private Process pressurePlot = null;
		private Process temperaturePlot = null;
		private List<double> pressureValue = new List<double>();
		private List<double> temperatureValue = new List<double>();

		public void run() {

			try {

				string[] configLines = File.ReadAllLines(filenameConfig);
				comName = configLines[0].Trim();
				Console.WriteLine("Connect to " + comName);
				plotTime = double.Parse(configLines[1]);
				Console.WriteLine("Data acq every " + plotTime.ToString() + " sec");

				serial = new SerialPort(comName, 9600, Parity.None, 8, StopBits.One);
				serial.DataReceived += (s, e) => recieved();
				serial.Open();

				pressurePlot = new Process();
				pressurePlot.StartInfo.FileName = "pgnuplot.exe";
				pressurePlot.StartInfo.Arguments = "-persist";
				pressurePlot.StartInfo.UseShellExecute = false;
				pressurePlot.StartInfo.RedirectStandardInput = true;
				pressurePlot.Start();
				pressurePlot.StandardInput.WriteLine("set xlabel \"Time (hour)\"");
				pressurePlot.StandardInput.WriteLine("set ylabel \"Pressure (Pa)\"");

				temperaturePlot = new Process();
				temperaturePlot.StartInfo.FileName = "pgnuplot.exe";
				temperaturePlot.StartInfo.Arguments = "-persist";
				temperaturePlot.StartInfo.UseShellExecute = false;
				temperaturePlot.StartInfo.RedirectStandardInput = true;
				temperaturePlot.Start();
				temperaturePlot.StandardInput.WriteLine("set xlabel \"Time (hour)\"");
				temperaturePlot.StandardInput.WriteLine("set ylabel \"Temperature (deg C)\"");

				DateTime nextFileUpdate = DateTime.Now.AddSeconds(plotTime);
				DateTime nextFileCreate = DateTime.Now.AddSeconds(-plotTime);
				string filenamePressure = "";
				string filenameTemperature="";

				Console.WriteLine("start logging");
				Console.WriteLine("Update - Enter key / Exit - escape key");
				while (true) {
					if (DateTime.Now > nextFileCreate) {
						filenamePressure = string.Format("{0}{1:D2}{2:D2}_{3}.txt",nextFileCreate.Year,nextFileCreate.Month,nextFileCreate.Day,datanamePressure);
						filenameTemperature = string.Format("{0}{1:D2}{2:D2}_{3}.txt", nextFileCreate.Year, nextFileCreate.Month, nextFileCreate.Day, datanameTemperature);
						if (!File.Exists(filenamePressure)) { var sw = File.CreateText(filenamePressure); sw.Close(); }
						if (!File.Exists(filenameTemperature)) { var sw = File.CreateText(filenameTemperature); sw.Close(); }
						nextFileCreate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
					}
					if (DateTime.Now > nextFileUpdate) {
						List<double> filledTemperatureValue = temperatureValue;
						List<double> filledPressureValue = pressureValue;
						temperatureValue = new List<double>();
						pressureValue = new List<double>();

						double timecur = DateTime.Now.Hour + (DateTime.Now.Minute / 60.0) + (DateTime.Now.Second / 3600.0);

						double tave=0, tdev=0, pave=0, pdev=0;

						if (filledTemperatureValue.Count > 0) {
							for (int i = 0; i < filledTemperatureValue.Count; i++) {
								tave += filledTemperatureValue[i];
							}
							tave /= filledTemperatureValue.Count;

							if (filledTemperatureValue.Count > 1) {
								for (int i = 0; i < filledTemperatureValue.Count; i++) {
									tdev += Math.Pow(filledTemperatureValue[i] - tave, 2);
								}
								tdev = Math.Sqrt(tdev / (filledTemperatureValue.Count - 1));
							} else {
								tdev = 0;
							}
							tdev = Math.Max(tdev, temperatureResolution);
							File.AppendAllText(filenameTemperature, timecur.ToString() + " " + tave.ToString() + " " + tdev.ToString() + "\n");
						}
						if (filledPressureValue.Count > 0) {
							for (int i = 0; i < filledPressureValue.Count; i++) {
								pave += filledPressureValue[i];
							}
							pave /= filledPressureValue.Count;
							if (filledPressureValue.Count > 1) {
								for (int i = 0; i < filledPressureValue.Count; i++) {
									pdev += Math.Pow(filledPressureValue[i] - pave, 2);
								}
								pdev = Math.Sqrt(pdev / (filledPressureValue.Count - 1));
							} else {
								pdev = 0;
							}
							pdev = Math.Max(pdev, pressureResolution);
							File.AppendAllText(filenamePressure, timecur.ToString() + " " + pave.ToString() + " " + pdev.ToString() + "\n");
						}

						nextFileUpdate = DateTime.Now.AddSeconds(plotTime);
					}
					if (Console.KeyAvailable) {
						ConsoleKeyInfo keyInfo = Console.ReadKey();
						if (keyInfo.Key == ConsoleKey.Enter) {
							Console.WriteLine("Replot");
							pressurePlot.StandardInput.WriteLine(string.Format("set title \"{0}\"", DateTime.Now.ToShortDateString()));
							pressurePlot.StandardInput.WriteLine("plot \"" + filenamePressure + "\" with yerrorbar ti \"data\"");
							temperaturePlot.StandardInput.WriteLine(string.Format("set title \"{0}\"", DateTime.Now.ToShortDateString()));
							temperaturePlot.StandardInput.WriteLine("plot \"" + filenameTemperature + "\" with yerrorbar ti \"data\"");
						} else if (keyInfo.Key == ConsoleKey.Escape) {
							Console.WriteLine("Exit");
							pressurePlot.StandardInput.WriteLine("exit");
							temperaturePlot.StandardInput.WriteLine("exit");
							break;
						} else {
							Console.WriteLine("Update - Enter key / Exit - escape key");
						}
					}
					Thread.Sleep(sleepTime);
				}
			}catch(Exception e){
				Console.WriteLine(e.Message);
				Console.ReadKey();
			}
			if (pressurePlot != null) {
				pressurePlot.Close();
				pressurePlot.Dispose();
			}
			if (temperaturePlot != null) {
				temperaturePlot.Close();
				temperaturePlot.Dispose();
			}
			if (serial != null) {
				serial.Close();
				serial.Dispose();
			}
		}
		public void recieved() {
			string data = serial.ReadLine();
			double val = double.Parse(data.Substring(1));
			if (data[0]=='p') {
				pressureValue.Add(val);
			} else if (data[0]=='t') {
				temperatureValue.Add(val);
			}
		}
	}
}
