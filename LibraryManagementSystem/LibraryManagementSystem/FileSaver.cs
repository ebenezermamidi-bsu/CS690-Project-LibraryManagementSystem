namespace LibraryManagementSystem;
using System.IO;
using System.Text.Json;

public class FileSaver
{
    private const string FileName = "libraryData.json";

    public void Save(Library library)
    {
        var json = JsonSerializer.Serialize(library, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FileName, json);
    }

    public Library Load()
    {
        if (!File.Exists(FileName))
            return new Library();

        var json = File.ReadAllText(FileName);
        return JsonSerializer.Deserialize<Library>(json) ?? new Library();
    }
}