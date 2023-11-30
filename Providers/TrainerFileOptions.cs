using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;

namespace HillsModelManager;

public class TrainerFileOptions : StaticFileOptions {
	public TrainerFileOptions(IConfiguration config) : base() {
		
		if(!string.IsNullOrEmpty(config["Trainer:Path"]))
			FileProvider = new PhysicalFileProvider(config["Trainer:Path"]!);
		
		RequestPath = new PathString("/trainer");
		ContentTypeProvider = new FileExtensionContentTypeProvider(
			new Dictionary<string, string>{
				{ ".pkl","application/octet-stream"}
			}
		);
		DefaultContentType = "application/octet-stream";
	}
}