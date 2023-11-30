namespace HillsModelManager.Models;


public enum TrainDatasetType {
	VALIDATION,
	TRAIN
}

public class TrainDataset {
	public int Id { get; set; } = -1;
	public string Path { get; set; } = string.Empty;
	public TrainDatasetType TrainData { get; set; } = TrainDatasetType.TRAIN;
}
