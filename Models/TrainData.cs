using System.Data;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using CsvHelper;
using CsvHelper.Configuration;

namespace HillsModelManager.Models;

public class TrainData {

	public readonly DateTime DateTime;
	
	public string? Predictions;
	public readonly string? PredictionsPath;
	private string? TrainDataPath;
	
	public DataTable GetPredictionTable() {
		DataTable dt = new();
		
		if(string.IsNullOrEmpty(PredictionsPath))
			return dt;

		CsvConfiguration config = new(CultureInfo.InvariantCulture){ Delimiter=";" };

		using (var reader = new StreamReader(PredictionsPath)) 
			using (var csv = new CsvReader(reader, config))
				using (var dr = new CsvDataReader(csv))	
					dt.Load(dr);

		return dt;
	}

	private TrainDataJson? _trainData = null;
	public TrainDataJson? TrainDataJson { 
		get {		
			if(_trainData != null) 
				return _trainData;
		
			if(string.IsNullOrEmpty(TrainDataPath))
				return null;

			return _trainData = new TrainDataJson(TrainDataPath);
		}
	}

	public IEnumerable<string> Models;
	public IEnumerable<string> Scalers;
	public IEnumerable<string> Images;
	public string TrainPath { get; set; }
	public string Name { get; set; }


	public TrainData(string path) {
		TrainPath = path;
		Name = Path.GetFileName(path);
			
		DateTime = Directory.GetCreationTime(path);
	

		var models = new List<string>();
		var scalers = new List<string>();
		var images = new List<string>();

		foreach(string file in Directory.GetFiles(path)) {
			string contentPath = $"/trainer/out/{Name}/{Path.GetFileName(file)}";

			if(file.EndsWith("Scaler.pkl")) {
				scalers.Add(contentPath);
			} else if(file.EndsWith(".pkl")) {
				models.Add(contentPath);
			} else if(file.EndsWith("Predictions.csv")) {
				Predictions = contentPath;
				PredictionsPath = file;
			}else if(file.EndsWith("train.json")) {
				TrainDataPath = file;
			} else if (file.EndsWith(".png")) {
				images.Add(contentPath);
			}
		}

		Models = models;
		Scalers = scalers;
		Images = images;
	}



}