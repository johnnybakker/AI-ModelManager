using System.Diagnostics;
using System.Text.RegularExpressions;
using HillsModelManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace HillsModelManager.Pages;

public partial class Train : ComponentBase {

	[Inject] 
	private TrainService TrainService { get; set; } = null!;

	public static long DirSize(DirectoryInfo d) 
	{    
		long size = 0;    
		// Add file sizes.
		FileInfo[] fis = d.GetFiles();
		foreach (FileInfo fi in fis) 
		{      
			size += fi.Length;    
		}
		// Add subdirectory sizes.
		DirectoryInfo[] dis = d.GetDirectories();
		foreach (DirectoryInfo di in dis) 
		{
			size += DirSize(di);   
		}
		return size;  
	}

	public Dictionary<string,object> ControlAttributes() { 
		Dictionary<string, object> attributes = new();
		if(TrainService.IsTraining) attributes.Add("disabled", "disabled");
		return attributes;
	}

	protected override Task OnInitializedAsync()
	{
		SelectedModel = TrainService.ModelNames.FirstOrDefault() ?? "";
		validationFile = null;
		inputFile = null;

		TrainService.ProcessStarted += ProcessUpdated;
		TrainService.ProcessDataReceived += ProcessUpdated;
		TrainService.ProcessEnded += ProcessUpdated;

		return base.OnInitializedAsync();
	}

	private void ProcessUpdated(object? o, EventArgs e) {
		InvokeAsync(StateHasChanged);
	}

	const long MAX_SIZE = (1024L * 1024L * 1024L * 10L);

	private async void Start()
    {
		DirectoryInfo di = new(TrainService.OutputDirectory);
		long size = DirSize(di);

		if(size > MAX_SIZE) return;

		validateModelName();
	
		if(string.IsNullOrEmpty(trainName) || !string.IsNullOrEmpty(trainNameError)) return;
		if(string.IsNullOrEmpty(SelectedModel)) return;
		if(inputFile == null) return;
		if(validationFile == null) return;

		await TrainService.Start(
			trainName,
			SelectedModel, 
			inputFile.OpenReadStream(MAX_SIZE), 
			validationFile.OpenReadStream(MAX_SIZE)
		);	
    }

	private void Stop()
    {
		TrainService.Stop();
	}

	[Parameter]
	public string? trainNameError {get; set; } = null; 

	string? trainName = null;
    private void TrainNameChanged(ChangeEventArgs e)
    {
		trainName = e.Value?.ToString();
		validateModelName();
		Console.WriteLine(trainNameError);
    }

	void validateModelName() {
		
		Regex trainNameRegex = new Regex("([a-zA-Z0-9\\-_])+");

		if(string.IsNullOrEmpty(trainName)) {
			trainNameError = "";
			return;
		}
	
		if(trainNameRegex.Match(trainName).Length != trainName.Length) {
			trainNameError = "A train name may not contain spaces or special characters";
			return;
		}

		if(Directory.Exists($"{TrainService.OutputDirectory}/{trainName}")) {
			trainNameError = "A training with this name already exist"; 
			return;
		}

		trainNameError = null;
	}

	IBrowserFile? inputFile = null;
    private void LoadInputFile(InputFileChangeEventArgs e)
    {
		inputFile = e.File;
		Console.WriteLine(inputFile.Size);
		Console.WriteLine(inputFile.Name);
    }

	IBrowserFile? validationFile = null;
    private void LoadValidationFile(InputFileChangeEventArgs e)
    {
		validationFile = e.File;
		Console.WriteLine(validationFile.Size);
		Console.WriteLine(validationFile.Name);
    }

  	string SelectedModel = "all";

	void SelectModel(ChangeEventArgs e)
    {
        SelectedModel = e.Value?.ToString() ?? "";
    }

}