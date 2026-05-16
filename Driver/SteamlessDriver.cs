using HidSharp;
using SteamlessController.Driver.utils;

namespace SteamlessController.Driver;

public static class SteamlessDriver {
	public const uint VendorId = 0x28DE;
	public static readonly int[] ProductIds = [0x1302, 0x1304];
	public static readonly object OutputLock = new();

	private static CancellationToken _cancellationToken;

	public static int Main(string[] args) {
		using var cts = new CancellationTokenSource();

		ConsoleCancelEventHandler cancelHandler = (_, e) => {
			e.Cancel = true;
			cts.Cancel();
		};
		Console.CancelKeyPress += cancelHandler;
		_cancellationToken = cts.Token;

		AppDomain.CurrentDomain.ProcessExit += (_, _) => {

		};

		// DeviceList.Local.Changed += (_, e) => {
		// 	Console.WriteLine("Device list changed. Rescanning...");
		// 	ScanForDevices();
		// };

		// Initial device scan
		ScanForDevices();
		
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

		void ScanForDevices() {
			ControllerManager.ScanForDevices(_cancellationToken, DeviceLoop);
		}
	}
	
	private static void DeviceLoop(ControllerDevice device) {
		var hidDevice = device.HidDevice;
		var token = _cancellationToken;
		try {
			// Open device and register device closing for when the cancellation token is triggered
			using var stream = hidDevice.Open();
			using var registration = token.Register(() => {
				try {
					stream.Close();
				} catch {
					// ignored
				}
			});

			var reportLength = Math.Max(1, hidDevice.GetMaxInputReportLength());
			var buffer = new byte[reportLength];
			Console.WriteLine($"Opened {device.DevicePath}");

			// Devices update loop
			while (!token.IsCancellationRequested) {
				// Read the next hid report
				int read;
				try {
					read = stream.Read(buffer, 0, buffer.Length);
				} catch (Exception ex) when
					(ex is IOException or ObjectDisposedException or InvalidOperationException) {
					if (token.IsCancellationRequested) {
						break;
					}

					Console.WriteLine($"Read stopped: {ex.Message}");
					break;
				}

				// Skip if hid data is empty
				if (read <= 0) continue;

				// Decoding
				var decoder = new BitDecoder(buffer, read);
				var reportId = decoder.ReadByte();

				if (reportId == 0x42) {
					// Console.WriteLine("Received input report");
				} else {
					Console.WriteLine($"Received unknown report: 0x{reportId:x2}");
				}
			}
		} catch (Exception e) {
			Console.WriteLine($"Failed to open/read HID device: {e.Message}");
		}
	}
}