using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace HillsModelManager.Models;

public class TrainData {

	public readonly DateTime DateTime;
	
	public string? Predictions;
	private string? PredictionsPath;
	
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

	public IEnumerable<string> Models;
	public IEnumerable<string> Scalers;
	public IEnumerable<string> Images;


	public TrainData(string path) {
		string name = Path.GetFileName(path);
			
		long ticks;
		long.TryParse(name, out ticks);

		DateTime = new(ticks);
	

		var models = new List<string>();
		var scalers = new List<string>();
		var images = new List<string>();

		foreach(string file in Directory.GetFiles(path)) {
			string contentPath = $"/trainer/out/{name}/{Path.GetFileName(file)}";

			if(file.EndsWith("Scaler.pkl")) {
				scalers.Add(contentPath);
			} else if(file.EndsWith(".pkl")) {
				models.Add(contentPath);
			} else if(file.EndsWith(".csv")) {
				Predictions = contentPath;
				PredictionsPath = file;

			} else if (file.EndsWith(".png")) {
				images.Add(contentPath);
			}
		}

		Models = models;
		Scalers = scalers;
		Images = images;
	}



}