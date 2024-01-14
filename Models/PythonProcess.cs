using System.Diagnostics;

namespace HillsModelManager.Models;

public abstract class PythonProcess : IDisposable
{	
	private Process? _process;
	private FileInfo _pythonFile;
	private string _pythonExecutable;
	private DateTime? _startedAt;
	
	public PythonProcess(FileInfo pythonFile, string pythonExecutable = "python") {
		_process = null;
		_pythonFile = pythonFile;
		_pythonExecutable = pythonExecutable;
		_startedAt = null;
	}

	protected Task<bool> Start(params string[] args) {

		if(_process != null) {
			Console.Error.WriteLine($"Process {_pythonFile.Name} already started!");
			return Task.FromResult(false);
		}

		if(!_pythonFile.Exists) {
			_process = null;
			Console.Error.WriteLine($"Python file '{_pythonFile.FullName}' does not exist!");
			return Task.FromResult(false);
		}

		_process = new()
		{
			StartInfo = new()
			{
				FileName = _pythonExecutable,
				Arguments = $"-u {_pythonFile.Name} {string.Join(' ', args)}",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
				WorkingDirectory = _pythonFile.Directory?.FullName
			},
			EnableRaisingEvents = true
		};

		_process.Exited += ProcessExited;
		_process.ErrorDataReceived += ProcessDataReceived;
		_process.OutputDataReceived += ProcessDataReceived;
		
		if(_process.Start()) 
		{	
			_process.BeginErrorReadLine();
			_process.BeginOutputReadLine();
			_startedAt = DateTime.Now;

			OnStart();
			return Task.FromResult(true);
		}

		_process = null;
		Console.Error.WriteLine($"Failed to start process {_pythonFile.Name}");
		
		OnExit();
		return Task.FromResult(false);
	}

	protected virtual void ProcessExited(object? sender, EventArgs e) {
		_process = null;
		Console.Error.WriteLine($"Process {_pythonFile.Name} exited");
		OnExit();
	}

	protected abstract void OnData(string data);
	protected abstract void OnExit();
	protected abstract void OnStart();

	protected virtual void ProcessDataReceived(object sender, DataReceivedEventArgs e)
	{
		if(e.Data != null) OnData(e.Data);
	}

	public void Stop()
	{
		if(_process == null)
			return;

		if(!_process.HasExited){
			Console.Error.WriteLine($"Killing process {_pythonFile.Name}");
			_process.Kill();
			_process.WaitForExit();
			_process = null;	
			OnExit();
		}
	}

	public void Dispose()
	{
		Stop();
	}
}