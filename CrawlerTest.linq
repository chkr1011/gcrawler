<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Framework.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Utilities.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.Protocols.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Design.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Build.Tasks.v4.0.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Windows.Forms.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.RegularExpressions.dll</Reference>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Drawing.Imaging</Namespace>
</Query>

string _cacheDirectory = "Cache";
string _contentDirectory = "Content";
WebClient _webClient;

void Main()
{
	Initialize();
	
	//string sourceText = ReadSource("http://www.animatedgif.net/numberscharacters/numbers.shtml");
	string sourceText = @"<IMG SRC=""/art/brand_links.gif"" WIDTH=""271"" HEIGHT=""25"" BORDER=""0"" VSPACE=""0"" HSPACE=""0""><A HREF=""/cartoons/cartoons.shtml"">";
	sourceText.Dump();
	Regex regex = new Regex(@"<\s*IMG\s*.*>", RegexOptions.IgnoreCase);
	var m = regex.Match(sourceText).Dump();
	Regex sourceRegex = new Regex(@"SRC=""([\w|%|/|\.]*)""", RegexOptions.IgnoreCase);
	sourceRegex.Match(m.Value).Groups.Dump();
	
	Shutdown();
}



private void Initialize()
{
	if (!Directory.Exists(_cacheDirectory))
	{
		Directory.CreateDirectory(_cacheDirectory);
	}
	
	if (!Directory.Exists(_contentDirectory))
	{
		Directory.CreateDirectory(_contentDirectory);
	}
	
	this._webClient = new WebClient();
}

private void Shutdown()
{
	if (this._webClient != null)
	{
		this._webClient.Dispose();
		this._webClient = null;
	}
}

private string ReadSource(string uri)
{
	return this._webClient.DownloadString(uri);
}

private List<string> ExtractSourceItems(string sourceText, string sourceUri)
{
	List<string> sourceItems = new List<string>();
	Regex imgRegex = new Regex(@"<\s*IMG\s*[\w|\s|""|/|=|.]*>", RegexOptions.IgnoreCase);
	Regex sourceRegex = new Regex(@"SRC=""[\w|%|/]*""", RegexOptions.IgnoreCase);
	
	foreach (Match match in imgRegex.Matches(sourceText))
	{
		string source = sourceRegex.Match(match.Value).Value;
		
	}
	
	
	return sourceItems;
}

private List<string> ExtractSubSources(string sourceText)
{
	List<string> subSources = new List<string>();
	
	return subSources;
}

private void ProcessSourceItem(string source)
{
	string cacheFilename = null;
	try
	{
		cacheFilename = DownloadContent(source);
		if (!ValidateContent(cacheFilename))
		{
			File.Delete(cacheFilename);
		}
	}
	catch (Exception processException)
	{
		Trace.WriteWarning("Unable to process source item '{0}'. {1}", source, processException.Message);
		
		if (cacheFilename != null && File.Exists(cacheFilename))
		{
			try
			{
				File.Delete(cacheFilename);
			}
			catch (Exception deleteException)
			{
				Trace.WriteWarning("Could not delete source item '{0}'. {1}", deleteException.Message);
			}
		}
	}
}

private string DownloadContent(string source)
{
	Uri sourceUri = new Uri(source);
	string filename =  sourceUri.Segments.Last();
	filename = HttpUtility.UrlDecode(filename);
	foreach (char invalidChar in Path.GetInvalidFileNameChars())
	{
		filename = filename.Replace(invalidChar, '_');
	}
	
	filename = Path.Combine(_cacheDirectory, filename);	
	
	_webClient.DownloadFile(source, filename);
	
	return filename;
}

private bool ValidateContent(string filename)
{
	try
	{
		using (Image image = Image.FromFile(filename))
		{
			if (image.RawFormat != ImageFormat.Gif)
			{
				Trace.WriteWarning("File '{0}' is an image but no GIF.", filename);
				return false;
			}
			
			if (image.FrameDimensionsList.Length == 0)
			{
				Trace.WriteWarning("File '{0}' has no frame dimension list.", filename);
				return false;
			}
			
			if (image.GetFrameCount(new FrameDimension(image.FrameDimensionsList[0])) < 2)
			{
				Trace.WriteWarning("File '{0}' is not animated.", filename);
				return false;
			}
		}
		
		return true;
	}
	catch (Exception exception)
	{
		Trace.WriteWarning("File '{0}' is no image file. {1}", filename, exception.Message);
		return false;
	}
}

public static class Trace
{
	private static readonly object _syncRoot = new object();

	public static void WriteInformation(string text, params object[] parameters)
	{
		Trace.Write(ConsoleColor.Green, string.Format(text, parameters));
	}
	
	public static void WriteVerbose(string text, params object[] parameters)
	{
		Trace.Write(ConsoleColor.Gray, string.Format(text, parameters));
	}
	
	public static void WriteWarning(string text, params object[] parameters)
	{
		Trace.Write(ConsoleColor.DarkYellow, string.Format(text, parameters));
	}
	
	public static void Write(ConsoleColor color, string text)
	{
		lock (Trace._syncRoot)
		{
			ConsoleColor oldColor = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ForegroundColor = oldColor;
		}
	}
}

// Define other methods and classes here
