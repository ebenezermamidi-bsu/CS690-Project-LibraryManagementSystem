namespace LibraryManagementSystem;
using System.IO;
using System.Text.Json;

public class FileSaver
{
    private const string FileName = "libraryData.json";

    // Method to serialize and save Library object as JSON string to a file.
    public void Save(Library library)
    {
        var json = JsonSerializer.Serialize(library, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FileName, json);
    }

    // Method to retrieve the data from json file and convert it back to Library object.
    public Library Load()
    {
        if (!File.Exists(FileName))
            return new Library();

        var json = File.ReadAllText(FileName);
        return JsonSerializer.Deserialize<Library>(json) ?? new Library();
    }
}