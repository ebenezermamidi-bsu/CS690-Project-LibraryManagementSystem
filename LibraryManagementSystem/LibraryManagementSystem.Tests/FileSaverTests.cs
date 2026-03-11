using System;
using Xunit;
using LibraryManagementSystem;
using System.Linq;

namespace LibraryManagementSystem.Tests
{
    public class FileSaverTests
    {
        private const string TestFile = "testLibraryData.json";

        // Test Data Manager data persistance - Save and retrieve data from json file
        [Fact]
        public void SaveAndLoad_ShouldPersistLibraryData()
        {
            var library = new Library();
            library.Books.Add(new Book { BookId = 1, Title = "Test Book", Author = "Author", Category = "Test" });

            var saver = new FileSaver();
            saver.Save(library);

            var loadedLibrary = saver.Load();

            Assert.Single(loadedLibrary.Books);
            Assert.Equal("Test Book", loadedLibrary.Books[0].Title);

            if (File.Exists(TestFile))
                File.Delete(TestFile);
        }
    }
}