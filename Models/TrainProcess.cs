using System.Diagnostics;

namespace HillsModelManager.Models;


public class TrainProcess : IDisposable {

	public readonly string Name;
	public readonly string Model;

	public readonly string OutputPath;
	public readonly string InputPath;
	public readonly string ValidationPath;
	
	private bool _started;
	public bool HasStarted => _started;
	public bool HasExited {
		get {
			try {
				return _process.HasExited;
			} catch(Exception) {
				return false;
			}
		}
	}

	private readonly Process _process;
	private readonly Stream _input;
	private readonly Stream _validation;

	public List<string> Output {get; set; }

	public event EventHandler? Exited;
	public event EventHandler? DataReceived;
	public event EventHandler? Started;

	public TrainProcess(string name, string pythonPath, string trainerPath, string model, Stream input, Stream validation, string output) {
		
		_started = false;
		_input = input;
		_validation = validation;
		
		Model = model;
		Name = name;
		OutputPath = output;
		InputPath = Path.Combine(output, "input.csv");
		ValidationPath = Path.Combine(output, "validation.csv");

		Output = new();
		
		_process = new()
		{
			StartInfo = new()
			{
				FileName = pythonPath,
				Arguments = $"-u ./scripts/train_one.py --input \"{InputPath}\" --output \"{OutputPath}\" --validation \"{ValidationPath}\" --models {Model}",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				WorkingDirectory = trainerPath
			},
			EnableRaisingEvents = true
		};

		_process.Exited += ProcessExited;
		_process.ErrorDataReceived += ProcessDataReceived;
		_process.OutputDataReceived += ProcessDataReceived;
	}

	private async Task copyInputFile() {
		await using FileStream InputFile = new(InputPath, FileMode.Create);
		await _input.CopyToAsync(InputFile);
		_input.Close();
	}

	private async Task copyValidationFile() {
		await using FileStream ValidationFile = new(ValidationPath, FileMode.Create);
		await _validation.CopyToAsync(ValidationFile);
		_validation.Close();
	}

	public async Task Start() 
	{
		_started = true;

		if(Directory.Exists(OutputPath))
			Directory.Delete(OutputPath, true);
		Directory.CreateDirectory(OutputPath);

		await Task.WhenAll(copyInputFile(), copyValidationFile());

		_process.Start();
		_process.BeginErrorReadLine();
		_process.BeginOutputReadLine();

		Started?.Invoke(this, new());
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
		if(!_process.HasExited)
			_process.Kill();
		_process.WaitForExit();
	}

	public void Dispose()
	{
		Stop();
	}
}