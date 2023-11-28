namespace HillsModelManager.Services;

public class TrainServiceOptions  {
	public string PythonPath {get; set;} = default!;
	public string Path {get; set;} = default!;
	public List<string> ModelNames {get; set;} = default!;
}