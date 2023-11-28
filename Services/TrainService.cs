using HillsModelManager.Models;

namespace HillsModelManager.Services;


public class TrainService  {

	private static TrainProcess? TrainProcess {get; set;} = null;
	
	public event EventHandler? ProcessStarted;
	public event EventHandler? ProcessDataReceived;
	public event EventHandler? ProcessEnded;	

	private readonly TrainServiceOptions options = new();
	private readonly IHostEnvironment environment;
	
	public IEnumerable<string> ModelNames => options.ModelNames;

	public string InputDirectory => Path.Combine(options.Path, "data");
	
	public string OutputDirectory => Path.Combine(environment.ContentRootPath, "wwwroot");
	public string OutputDirectoryTMP => Path.Combine(OutputDirectory, "tmp");
	public IEnumerable<string> Inputs => Directory.GetFiles(InputDirectory, "*.csv");


	public TrainService(IConfiguration Configuration, IHostEnvironment Environment) {
		environment = Environment;
		Configuration.GetSection("Trainer").Bind(options);
	}

	public void Start(string model, string input, string validationInput) {

		if(IsTraining)
			return;

		string name = DateTime.Now.Ticks.ToString();
		TrainProcess = new(name, options.PythonPath, options.Path, model, input, Path.Combine(OutputDirectoryTMP, name), validationInput);
		
		TrainProcess.Exited += (obj, e) => {
			//TrainProcess = null;
			ProcessEnded?.Invoke(this, new());
		};

		TrainProcess.DataReceived += (obj, e) => {
			ProcessDataReceived?.Invoke(this, new());
		};

		ProcessStarted?.Invoke(this, new());
	}

	public void Stop() {

		if(TrainProcess == null)
			return;

		TrainProcess.Stop();
		//TrainProcess = null;
	}

	public bool IsTraining => TrainProcess != null && !TrainProcess.Process.HasExited;

	public List<string>? Output => TrainProcess?.Output;
	public IEnumerable<string>? OutputImages => 
		TrainProcess != null && TrainProcess.Process.HasExited ? 
			Directory.GetFiles(Path.Combine(OutputDirectoryTMP, TrainProcess.Name), "*.png")
				.ToList()
				.Select(p => Path.Combine("/tmp/", TrainProcess.Name, Path.GetFileName(p))) :
			null;

	public string? OutputPredictions => 
		TrainProcess != null && TrainProcess.Process.HasExited ? 
			Path.Combine("/tmp/", TrainProcess.Name, "Predictions.csv") :
			null;

	public IEnumerable<string>? OutputModels => 
		TrainProcess != null && TrainProcess.Process.HasExited ? 
			Directory.GetFiles(Path.Combine(OutputDirectoryTMP, TrainProcess.Name), "*.pkl")
				.ToList()
				.Select(p => Path.Combine("/tmp/", TrainProcess.Name, Path.GetFileName(p))) :
			null;
}