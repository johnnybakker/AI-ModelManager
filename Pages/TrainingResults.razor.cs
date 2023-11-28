using System.Diagnostics;
using HillsModelManager.Models;
using HillsModelManager.Services;
using Microsoft.AspNetCore.Components;

namespace HillsModelManager.Pages;

public partial class TrainingResults : ComponentBase {

	[Inject] 
	private TrainService TrainService { get; set; } = null!;

	public IEnumerable<string> TrainingPaths => 
		Directory.GetDirectories(TrainService.OutputDirectoryTMP)
			.OrderDescending();


	public List<TrainData> Results = new();

	protected override Task OnInitializedAsync()
	{
		Results.Clear();
		foreach(var path in TrainingPaths) {
			Results.Add(new(path));
		}

		return base.OnInitializedAsync();
	}

	public async Task DeleteResult(long result) {
		Directory.Delete(Path.Combine(TrainService.OutputDirectoryTMP, result.ToString()), true);
		await OnInitializedAsync();
	}
}