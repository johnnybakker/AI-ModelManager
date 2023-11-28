using System.Diagnostics;
using HillsModelManager.Services;
using Microsoft.AspNetCore.Components;

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
		SelectedInput = TrainService.Inputs.FirstOrDefault() ?? "";
		SelectedValidationInput = SelectedInput;

		TrainService.ProcessStarted += ProcessUpdated;
		TrainService.ProcessDataReceived += ProcessUpdated;
		TrainService.ProcessEnded += ProcessUpdated;

		return base.OnInitializedAsync();
	}

	private void ProcessUpdated(object? o, EventArgs e) {
		InvokeAsync(StateHasChanged);
	}

	const long MAX_SIZE = (1024L * 1024L * 1024L * 10L);

	private void Start()
    {
		DirectoryInfo di = new(TrainService.OutputDirectoryTMP);
		long size = DirSize(di);

		if(size > MAX_SIZE) return;
	

		if(string.IsNullOrEmpty(SelectedModel)) return;
		if(string.IsNullOrEmpty(SelectedInput)) return;
		if(string.IsNullOrEmpty(SelectedValidationInput)) return;

		TrainService.Start(SelectedModel, SelectedInput, SelectedValidationInput);	
    }


	private void Stop()
    {
		TrainService.Stop();
	}

  	string SelectedModel = "";

	void SelectModel(ChangeEventArgs e)
    {
        SelectedModel = e.Value?.ToString() ?? "";
    }

  	string SelectedInput = "";
	void SelectInput(ChangeEventArgs e)
    {
        SelectedInput = e.Value?.ToString() ?? "";
    }

  	string SelectedValidationInput = "";
	void SelectValidationInput(ChangeEventArgs e)
    {
        SelectedValidationInput = e.Value?.ToString() ?? "";
    }
}