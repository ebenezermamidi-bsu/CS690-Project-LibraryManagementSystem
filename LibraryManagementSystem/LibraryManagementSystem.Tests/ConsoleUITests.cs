using System;
using Xunit;
using LibraryManagementSystem;
using System.Linq;

namespace LibraryManagementSystem.Tests
{
    public class ConsoleUITests
    {
        // Test console UI to write message to console
        [Fact]
        public void ShowMessage_ShouldPrintMessageText()
        {
            var message = "Hello, Library!";
            using var sw = new StringWriter();
            Console.SetOut(sw);

            ConsoleUI.showMessage(message);

            var output = sw.ToString().Trim();
            Assert.Contains("Hello, Library!", output);
        }

        // Test Console UI to load initial data
        [Fact]
        public void SeedData_ShouldPopulateLibraryWithInitialData()
        {
            var ui = new ConsoleUI();
            ui.SeedData();

            Assert.NotEmpty(ui.library.Books);
            Assert.NotEmpty(ui.library.Members);
            Assert.NotEmpty(ui.library.StaffMembers);
        }
    }
}