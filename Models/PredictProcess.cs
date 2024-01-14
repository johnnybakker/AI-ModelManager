using System.Diagnostics;

namespace HillsModelManager.Models;

public class PredictProcess : PythonProcess {

	public class Result {
		public string message = "Done!";
		public string outputPath = "";
		public string outputUrl = "";
	}


	public event EventHandler<string>? Progress;
	public event EventHandler<string>? Started;
	public event EventHandler<Result>? Ended;
	private DirectoryInfo? OutputDirectory;
	
	public PredictProcess(FileInfo pythonFile, string pythonPath) : 
		base(pythonFile, pythonPath) {
	}

	public async Task<bool> Start(DirectoryInfo trainingDirectory, Stream inputStream, string[] models) {
	
		if(!trainingDirectory.Exists)
			return false;

		long ticks = DateTime.Now.Ticks;
		OutputDirectory = trainingDirectory.CreateSubdirectory(ticks.ToString());
		
		string inputFilePath = Path.Combine(OutputDirectory.FullName, "input.csv");
		FileStream inputFile = new(inputFilePath, FileMode.Create);
		
		inputStream.Seek(0, SeekOrigin.Begin);
		await inputStream.CopyToAsync(inputFile);
	
		inputFile.Close();
		inputStream.Close();

		return await Start(
			"-t", $"\"{trainingDirectory.FullName}\"", 
			"-i", $"\"{inputFilePath}\"", 
			"-o", $"\"{OutputDirectory.FullName}\""	
		);
	}

	protected override void OnData(string data)
	{
		Progress?.Invoke(this, data);
	}

	protected override void OnExit()
	{
		Ended?.Invoke(this, new(){
			outputPath = OutputDirectory?.FullName ?? ""
		});
	}

	protected override void OnStart()
	{
		Started?.Invoke(this, "");
	}
}