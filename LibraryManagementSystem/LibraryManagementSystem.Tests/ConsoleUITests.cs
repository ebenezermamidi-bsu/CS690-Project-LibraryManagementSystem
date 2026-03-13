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
    }
}