using System.Diagnostics;

namespace HillsModelManager.Models;


public class TrainProcess : IDisposable {

	public readonly string Name;
	public readonly string Model;
	public Process Process { get; set; }
	public List<string> Output {get; set; }

	public event EventHandler? Exited;
	public event EventHandler? DataReceived;

	public TrainProcess(string name, string pythonPath, string trainerPath, string model, string input, string output, string validation) {
		Model = model;
		Name = name;
		Output = new();
		Process = new()
		{
			StartInfo = new()
			{
				FileName = pythonPath,
				Arguments = $"-u ./scripts/train_one.py --input \"{input}\" --output \"{output}\" --validation \"{validation}\" --models {model}",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				WorkingDirectory = trainerPath
			},
			EnableRaisingEvents = true
		};

		Process.Exited += ProcessExited;
		Process.ErrorDataReceived += ProcessDataReceived;
		Process.OutputDataReceived += ProcessDataReceived;

		Process.Start();
		Process.BeginErrorReadLine();
		Process.BeginOutputReadLine();
	}

	private void ProcessExited(object? sender, EventArgs e) {
		Output.Add("Done!");
		Exited?.Invoke(this, new());
	}

	private void ProcessDataReceived(object sender, DataReceivedEventArgs e)
	{
		if(e.Data != null) Output.Add(e.Data);
		DataReceived?.Invoke(this, new());
	}

	public void Stop()
	{
		if(!Process.HasExited)
			Process.Kill();
		Process.WaitForExit();
	}

	public void Dispose()
	{
		Stop();
	}
}