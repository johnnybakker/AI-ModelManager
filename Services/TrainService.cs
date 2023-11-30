using HillsModelManager.Database;
using HillsModelManager.Models;
using HillsModelManager.Pages;

namespace HillsModelManager.Services;


public class TrainService  {

	private static TrainProcess? TrainProcess {get; set;} = null;
	
	public event EventHandler? ProcessStarted;
	public event EventHandler? ProcessDataReceived;
	public event EventHandler? ProcessEnded;	

	private readonly TrainServiceOptions options = new();
	private readonly IHostEnvironment environment;

	public IEnumerable<string> ModelNames => options.ModelNames;

	public string OutputDirectory => Path.Combine(options.Path, "out");
	public string OutputUrl => "/trainer/out";

	public TrainService(IConfiguration Configuration, IHostEnvironment Environment) 
	{		
		environment = Environment;

		Configuration.GetSection("Trainer").Bind(options);

		if(!Directory.Exists(OutputDirectory)) {
			Directory.CreateDirectory(OutputDirectory);
		}
	}

	public async Task Start(string model, Stream input, Stream validation) {

		if(IsTraining)
			return;

		string name = DateTime.Now.Ticks.ToString();
		TrainProcess = new(name, options.PythonPath, options.Path, model, input, validation, Path.Combine(OutputDirectory, name));
		
		TrainProcess.Exited += (obj, e) => 
			ProcessEnded?.Invoke(this, new());

		TrainProcess.DataReceived += (obj, e) => 
			ProcessDataReceived?.Invoke(this, new());

		TrainProcess.Started += (obj, e) => 
			ProcessStarted?.Invoke(this, new());

		await TrainProcess.Start();
	}

	public void Stop() {

		if(TrainProcess == null)
			return;

		TrainProcess.Stop();
	}

	public bool IsTraining => TrainProcess != null && !TrainProcess.HasExited;

	public List<string>? Output => TrainProcess?.Output;
	public IEnumerable<string>? OutputImages => 
		TrainProcess != null && TrainProcess.HasExited ? 
			Directory.GetFiles(TrainProcess.OutputPath, "*.png")
				.ToList()
				.Select(p => Path.Combine(OutputUrl, TrainProcess.Name, Path.GetFileName(p))) :
			null;

	public string? OutputPredictions => 
		TrainProcess != null && TrainProcess.HasExited ? 
			Path.Combine(OutputUrl, TrainProcess.Name, "Predictions.csv") :
			null;

	public string? OutputName => 
		TrainProcess != null && TrainProcess.HasExited ? TrainProcess.Name : null;

	public IEnumerable<string>? OutputModels => 
		TrainProcess != null && TrainProcess.HasExited ? 
			Directory.GetFiles(TrainProcess.OutputPath, "*.pkl")
				.ToList()
				.Select(p => Path.Combine(OutputUrl, TrainProcess.Name, Path.GetFileName(p))) :
			null;
}