using System.Data;
using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using HillsModelManager.Models;
using HillsModelManager.Services;
using Microsoft.AspNetCore.Components;

namespace HillsModelManager.Pages;

public partial class TrainingResult : ComponentBase {

	[Inject] 
	private TrainService TrainService { get; set; } = null!;

 	[Parameter]
    public string Training { get; set; } = default!;

	public TrainData Result => new(Path.Combine(TrainService.OutputDirectory, Training));
	public DataTable PredictionTable = new();

	protected override Task OnInitializedAsync()
	{
		PredictionTable = Result.GetPredictionTable();

		return base.OnInitializedAsync();
	}

}