using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HillsModelManager.Models;
using HillsModelManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;

namespace HillsModelManager.Pages;


partial class Predict : ComponentBase {

	[Inject] 
	private TrainService TrainService { get; set; } = null!;

	[Inject] 
	private IJSRuntime JS { get; set; } = null!;


	private InputFile datasetInput = null!;

	public PredictProcess? PredictProcess { get; set; } = null;

	private string[]? Columns;
	private string SelectedTraining = default!;
	private Stream? InputStream = null;

	const long MAX_SIZE = (1024L * 1024L * 1024L * 10L);

	public List<TrainData> Results = new();

	private string? PredictionResult = null;
	private string? PredictionResultUrl = null;


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

	protected void OnTrainingChanged(ChangeEventArgs e) 
	{
		Console.WriteLine(e.Value);
		SelectedTraining = e.Value?.ToString() ?? "";
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

		PredictProcess.Ended += (obj, msg) => InvokeAsync(() => {
			string predictions = Path.Combine(msg.outputPath, "Predictions.csv");
	
			PredictionResultUrl = predictions.Replace(TrainService.OutputDirectory, TrainService.OutputUrl);
			PredictionResult = predictions;
			PredictProcess = null;	
			StateHasChanged();
		});

		string trainingPath = @"C:\Studie\Module - Artificial Intelligence\HillsLIWCase\out\Dienten-zonder-density-all";
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