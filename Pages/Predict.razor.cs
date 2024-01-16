using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HillsModelManager.Models;
using HillsModelManager.Services;
using HillsModelManager.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.JSInterop;

namespace HillsModelManager.Pages;


partial class Predict : ComponentBase {

	[Inject] 
	private TrainService TrainService { get; set; } = null!;

	[Inject] 
	private IJSRuntime JS { get; set; } = null!;


	private InputFile datasetInput = null!;
	private string datasetInputClass => 
		$"form-control disabled {
			(missingFeatures.Count() > 0 ? "is-invalid" : "")
		} {
			(string.IsNullOrEmpty(SelectedTraining) ? "disabled" : "") 
		}";

	private IEnumerable<string> missingFeatures = new string[0];

	public PredictProcess? PredictProcess { get; set; } = null;

	private string[]? Columns;

	private string _selectedTraining = default!;
	private string SelectedTraining {
		get => _selectedTraining;
		set {
			_selectedTraining = value;
		}
	}
	
	private Stream? _inputStream = null;
	private Stream? InputStream { 
		get => _inputStream; 
		set {
			_inputStream = value;
			missingFeatures = MissingFeatures;
		}
	}

	const long MAX_SIZE = (1024L * 1024L * 1024L * 10L);

	public List<TrainData> Results = new();

	private string? PredictionResult = null;
	private string? PredictionResultUrl = null;

	public CSVTable ResultTable { get; set; } = default!;

	public IEnumerable<string> TrainingPaths => 
		Directory.GetDirectories(TrainService.OutputDirectory)
			.OrderDescending();

	protected override Task OnInitializedAsync()
	{
		Results.Clear();
		foreach(var path in TrainingPaths) {
			Results.Add(new(path));
		}

		return base.OnInitializedAsync();
	}

	private TrainData? _trainData = null;
	public TrainData? TrainData { 
		get => _trainData; 
		set {
			_trainData = value;
			missingFeatures = MissingFeatures;
		}
	}

	public IEnumerable<string> MissingFeatures {
		get {
			if(TrainData == null) {
				return new string[0];
			}

			if(TrainData.TrainDataJson == null || Columns == null) {
				return new string[0];
			}

			List<string> missingFeatures = new List<string>();
			foreach(string feature in TrainData.TrainDataJson.Features) {
				if(!Columns.Contains(feature)) 
					missingFeatures.Add(feature);
			}

			return missingFeatures;
		}
	}

	protected async void OnTrainingChanged(ChangeEventArgs e) 
	{
		SelectedTraining = e.Value?.ToString() ?? "";

		string? disabledAttr = await JS.InvokeAsync<string?>("getAttr", datasetInput.Element, "disabled");

		if(disabledAttr == null && string.IsNullOrEmpty(SelectedTraining)) {
			await JS.InvokeVoidAsync("addAttr", datasetInput.Element, "disabled", "disabled");
		} else if(disabledAttr != null && !string.IsNullOrEmpty(SelectedTraining)) {
			await JS.InvokeVoidAsync("removeAttr", datasetInput.Element, "disabled");
		}

		if(string.IsNullOrEmpty(SelectedTraining)) {
			TrainData = null;
			return;
		}

		TrainData = new TrainData(SelectedTraining);
	}

	protected async void predict() 
	{
		if(InputStream == null) return;

		PredictProcess = TrainService.CreatePredictProcess(SelectedTraining, "all", InputStream);
		
		PredictProcess.Started += (obj, msg) => InvokeAsync(async () => {
			await resetDatasetInput();
			Columns = null;
			InputStream = null;
			PredictionResult = null;
			StateHasChanged();
		});

		PredictProcess.Progress += (obj, msg) => {
			Console.WriteLine($"Progress: {msg}");
		};

		PredictProcess.Ended += (obj, msg) => InvokeAsync(async () => {
			string predictions = Path.Combine(msg.outputPath, "Predictions.csv");
	
			PredictionResultUrl = predictions.Replace(TrainService.OutputDirectory, TrainService.OutputUrl);
			PredictionResult = predictions;
			PredictProcess = null;	
			StateHasChanged();

			await Task.Delay(100);
			await JS.InvokeVoidAsync("scrollTo", ResultTable.TableElement);
		});

		string trainingPath = SelectedTraining;
		DirectoryInfo trainingDirectory = new(trainingPath);

		await PredictProcess.Start(trainingDirectory, InputStream, new string[]{ "all" });
	}

	async Task resetDatasetInput() {
		await JS.InvokeVoidAsync("resetValue", datasetInput.Element!.Value);
	}

	async void OnInputDataset(InputFileChangeEventArgs e) 
	{
		if(e.FileCount == 0) {
			Columns = null;
			StateHasChanged();
			return;
		} 

		Console.WriteLine("Input changed");
		PredictionResult = null;
		InputStream = null;
		StateHasChanged();

		Stream browserStream = e.File.OpenReadStream(MAX_SIZE);
		CsvConfiguration config = new(CultureInfo.InvariantCulture){ 
			Delimiter = ";",
			HasHeaderRecord = true
		};
		
		using (var reader = new StreamReader(browserStream)) 
		{
		 	using (var csv = new CsvReader(reader, config)) 
			{
				await csv.ReadAsync();
				csv.ReadHeader();
	
				Columns = csv.HeaderRecord;
			
				InputStream = new MemoryStream(); 
				
				var writer = new StreamWriter(InputStream, leaveOpen: true);
				var csvOut = new CsvWriter(writer, config);
				var records = csv.GetRecordsAsync<dynamic>();
				await csvOut.WriteRecordsAsync(records);			

				StateHasChanged();
			}
		}
	}

}