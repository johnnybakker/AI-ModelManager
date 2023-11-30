using System.Diagnostics;
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
	

		if(string.IsNullOrEmpty(SelectedModel)) return;
		if(inputFile == null) return;
		if(validationFile == null) return;

		await TrainService.Start(
			SelectedModel, 
			inputFile.OpenReadStream(MAX_SIZE), 
			validationFile.OpenReadStream(MAX_SIZE)
		);	
    }

	private void Stop()
    {
		TrainService.Stop();
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

  	string SelectedModel = "";

	void SelectModel(ChangeEventArgs e)
    {
        SelectedModel = e.Value?.ToString() ?? "";
    }
}