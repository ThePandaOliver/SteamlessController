using System.Text;
using HidSharp;
using SteamlessControllerDriver.utils;

namespace SteamlessControllerDriver;

public static class SteamlessDriver {
	public const uint VendorId = 0x28DE;
	public static readonly int[] ProductIds = [0x1302, 0x1304];
	public static readonly object OutputLock = new();

	public static int Main(string[] args) {
		using var cts = new CancellationTokenSource();

		ConsoleCancelEventHandler cancelHandler = (_, e) => {
			e.Cancel = true;
			cts.Cancel();
		};
		Console.CancelKeyPress += cancelHandler;

		DeviceList.Local.Changed += (_, e) => {
			Console.WriteLine("Device list changed. Rescanning...");
			ControllerManager.ScanForDevices(cts.Token);
		};

		// Initial device scan
		ControllerManager.ScanForDevices(cts.Token);

		try {
			Console.WriteLine("Listening for HID input reports. Press Ctrl+C to stop.");
			Task.WaitAll(ControllerManager.ActiveTasksByDevicePath.Values);
		} catch (AggregateException ex) when (cts.IsCancellationRequested) {
			foreach (var inner in ex.Flatten().InnerExceptions) {
				if (inner is OperationCanceledException) {
					continue;
				}

				Console.WriteLine(inner.Message);
			}

			return 0;
		} finally {
			Console.CancelKeyPress -= cancelHandler;
		}
		return 0;
	}
	
	private static void DumpDeviceReports(int deviceIndex, HidDevice device, object outputLock, CancellationToken token) {
		try {
			using var stream = device.Open();
			using var registration = token.Register(() => {
				try {
					stream.Close();
				} catch {
					// ignored
				}
			});

			var reportLength = Math.Max(1, device.GetMaxInputReportLength());
			var buffer = new byte[reportLength];
			Console.WriteLine($"[{deviceIndex}] opened {device.DevicePath}");

			while (!token.IsCancellationRequested) {
				int read;
				try {
					read = stream.Read(buffer, 0, buffer.Length);
				} catch (Exception ex) when (ex is IOException or ObjectDisposedException or InvalidOperationException) {
					if (token.IsCancellationRequested) {
						break;
					}

					lock (outputLock) {
						Console.WriteLine($"[{deviceIndex}] read stopped: {ex.Message}");
					}

					break;
				}

				if (read <= 0) {
					continue;
				}

				var bitString = ToBitString(buffer, read);
				var timestamp = DateTimeOffset.UtcNow.ToString("O");
				var decoder = new BitDecoder(buffer, read);
				var report = ControllerReport.Decode(decoder);

				lock (outputLock) {
					// Console.WriteLine($"{timestamp} [{deviceIndex}] {device.DevicePath} {hex}");
					// Console.WriteLine($"{timestamp} [{deviceIndex}] {device.DevicePath} {bitString}");
					Console.WriteLine(report.ToString());
					// Console.WriteLine(report.inputs.ToString());
				}
			}
		} catch (Exception ex) {
			lock (outputLock) {
				Console.WriteLine($"[{deviceIndex}] failed to open/read HID device: {ex.Message}");
			}
		}
	}

	private static string ToBitString(byte[] buffer, int length) {
		var sb = new StringBuilder(length * 8);
		for (var i = 0; i < length; i++) {
			sb.Append(Convert.ToString(buffer[i], 2).PadLeft(8, '0') + " ");
		}

		return sb.ToString();
	}
}