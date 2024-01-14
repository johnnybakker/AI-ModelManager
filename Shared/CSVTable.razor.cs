using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using HillsModelManager.Models;
using HillsModelManager.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.JSInterop;

namespace HillsModelManager.Shared;


partial class CSVTable : ComponentBase {

	[Parameter]
	public string Path { get; set; } = default!;

	[Parameter]
	public int Rows { get; set; }

	[Parameter]
	public int Page { get; set; }

	private FileInfo? csvFile;

	private string[]? csvColumns = null;
	private LinkedList<List<dynamic>> csvPages = new();

	public ElementReference TableElement { get; set; } = default!;

	protected override async Task OnInitializedAsync()
	{	
		csvFile = new(Path);
		
		if(csvFile.Exists) {
			await LoadCSV(csvFile.FullName);
		}

		

		await base.OnInitializedAsync();
	}

	private void SetPage(int page) {
		Console.WriteLine(page);
		Page = page;
		StateHasChanged();
	}

	private async Task LoadCSV(string path) {


		CsvConfiguration config = new(CultureInfo.InvariantCulture){ 
			Delimiter = ";",
			HasHeaderRecord = true
		};

		csvPages = new();

		using (var reader = new StreamReader(path)) 
		{
			using (var csv = new CsvReader(reader, config))
			{
				await csv.ReadAsync();
				csv.ReadHeader();
				csvColumns = csv.HeaderRecord;

				var records = csv.GetRecordsAsync<dynamic>();
				
				List<dynamic> page = new(Rows);

				await foreach(var record in records) 
				{	
					page.Add(record);
					
					if(page.Count == Rows) {
						csvPages.AddLast(page);
						page = new(Rows);
						StateHasChanged();
					}
				}
			}
		}
	}

}