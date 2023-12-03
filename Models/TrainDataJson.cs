using System.Collections;
using System.Text.Json;
using System.Text.Json.Nodes;

public class TrainDataJson {
	public class Model {
		public string name { get; set; } = default!;
		public float mse { get; set; }
		public float mae { get; set; }
		public float r2 { get; set; }
	}

	private JsonObject json;

	public readonly IEnumerable<string> Features;
	public readonly IEnumerable<Model> Models;



	public TrainDataJson(string path) {
		using (var reader = new StreamReader(path)) {
			if(reader != null)
				json = JsonSerializer.Deserialize<JsonObject>(reader.ReadToEnd()) ?? new JsonObject();
			else
				json = new JsonObject();
		}

		Features = json["features"]?.AsArray().Select(e => e?.ToString() ?? "") ?? new string[] {};
		Models = json["models"]?.Deserialize<IEnumerable<Model>>() ?? new Model[] {};
	}
}