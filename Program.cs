using System.Security.Cryptography;
using System.IO;
using System.IO.Compression;
using System.Text;
using Ionic.Zip;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

List<string> passwords = new();

async Task ProcessZip(string zipPath)
{
    Console.WriteLine(" ");

    if (!File.Exists(zipPath)) {
        Console.WriteLine($"[{zipPath}] does not exist!");
        return;
    }
    if (!zipPath.EndsWith(".zip") && !zipPath.EndsWith(".cbz"))
    {
        return;
    }

    Console.WriteLine($"Processing {zipPath}");
    Ionic.Zip.ZipFile? zfile = null;
  
    try
    {
      zfile = Ionic.Zip.ZipFile.Read(zipPath);
    }
    catch
    {
        Console.WriteLine($"{zipPath} is not a ZIP file. Skipping.");
        return;
    }

    if (zfile == null || zfile.Entries.Count == 0)
    {
        Console.WriteLine("ZIP file has no files inside! Skipping..");
        return;
    }

    var testEntry = zfile.Entries.First();
    if(testEntry.Encryption == EncryptionAlgorithm.None)
    {
        Console.WriteLine("No encryption! Skipping.");
        return;
    }

    //Find the correct pass from our passwords for this file, testing if it works.
    string? correctPass = null;

    using (var ms = new MemoryStream())
    {
        foreach (string password in passwords)
        {
            try
            {
                testEntry.ExtractWithPassword(ms, password);
            }
            catch { continue; }

            correctPass = password;
            break;
        }
    }
    if (correctPass == null)
    {
        Console.WriteLine($"No Successful Password found for: [{zipPath}]... skipping.");
        return;
    }


    List<MemoryStream> entryStreams = new List<MemoryStream>(zfile.Entries.Count);
    var entries = zfile.Entries.ToArray();

    foreach (var zipentry in entries)
    {
        var ms = new MemoryStream();
        entryStreams.Add(ms);

        zipentry.ExtractWithPassword(ms, correctPass);
        ms.Seek(0, SeekOrigin.Begin);

        zfile.UpdateEntry(zipentry.FileName, ms);
    }
    
    zfile.Save(zipPath);

    Console.WriteLine($"{zipPath} successfully decrypted using password: {correctPass}");

    foreach (var ms in entryStreams) { ms.Dispose(); }
}

async Task<bool> ProcessArgs(string[] args)
{
    string? path = null;
    string? pass = null;
    string? passfile = null;

    for(int i = 0; i < args.Length; i++)
    {
        Console.WriteLine(args[i] + " ");
        if (args[i] == "-path")
        {
            path = args[i + 1];
            i++;
        }
        else if (args[i] == "-pass")
        {
            pass = args[i + 1];
            i++;
        }
        else if (args[i] == "-passfile")
        {
            passfile = args[i + 1];
            i++;
        }
    }
    if (path == null)
    {
        Console.WriteLine("No File or Path defined. Make sure to use -path PATH  to target a file or directory.");
        return false;
    }
    if (passfile != null)
    {
        if (!File.Exists(passfile))
        {
            Console.WriteLine($"{passfile} does not exist!");
            return false;
        }

        var passes = File.ReadAllLines(passfile);

        foreach(string curPass in passes)
        {
            if (!String.IsNullOrWhiteSpace(curPass))
            {
                passwords.Add(curPass);
            }
        }
    }
    else if (pass != null)
    { 
        passwords.Add(pass);
    }
    else
    {
        Console.WriteLine("No Password or Password file provided. Use '-pass PASSWORD' or '-passfile FILEPATH' to provide passwords for files.");
        return false;
    }

    if (Directory.Exists(path))
    {
        Console.WriteLine($"{path} is a directory! Processing all subfiles.");
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        if(files != null)
        {
            foreach(string file in files)
            {
                await ProcessZip(file);
            }
        }
    }
    else { await ProcessZip(path); }

    return true;
}

bool success = await ProcessArgs(args);

if (!success)
{
    Console.WriteLine("Full usage: batchzipdecrypt.exe -path FILE_OR_FOLDER -pass PASSWORD  (-passfile to use a file of passwords)");
}