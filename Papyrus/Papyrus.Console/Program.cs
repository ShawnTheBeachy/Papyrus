using System.IO;
using System.Linq;

namespace Papyrus.Console
{
	class Program
    {
        static void Main(string[] args)
        {
			var path = @"C:\Users\ABC Software\OneDrive\Documents\eBooks";
			var dirs = Directory.EnumerateDirectories(path);

			foreach (var dir in dirs)
			{
				InitializeEBook(dir);
			}
		}

		static void InitializeEBook(string path)
		{
			System.Console.Clear();
			var eBook = new EBook(path);
			eBook.TableOfContents = eBook.GetTableOfContents();
			System.Console.WriteLine($"Mimetype is {(!eBook.IsMimetypeValid() ? "not " : "")}valid.");
			System.Console.WriteLine($"Content location is {eBook.ContentLocation}.");
			System.Console.WriteLine($"Cover location is is {eBook.CoverLocation}.");
			System.Console.WriteLine($"Alternative Title: {eBook.Metadata.AlternativeTitle}");
			System.Console.WriteLine($"Audience: {eBook.Metadata.Audience}");
			System.Console.WriteLine($"Available: {eBook.Metadata.Available}");
			System.Console.WriteLine($"Contributor: {eBook.Metadata.Contributor}");
			System.Console.WriteLine($"Created: {eBook.Metadata.Created}");
			System.Console.WriteLine($"Creator: {eBook.Metadata.Creator}");
			System.Console.WriteLine($"Date: {eBook.Metadata.Date}");
			System.Console.WriteLine($"Description: {eBook.Metadata.Description}");
			System.Console.WriteLine($"Language: {eBook.Metadata.Language}");
			System.Console.WriteLine($"Title: {eBook.Metadata.Title}");
			System.Console.WriteLine();
			System.Console.WriteLine("Table of Contents:");

			foreach (var navPoint in eBook.TableOfContents.FlatItems)
			{
				System.Console.WriteLine(navPoint.Text);
			}

			System.Console.Write(eBook.GetContents(eBook.Spine.Last(), true));
			System.Console.ReadKey();
		}
    }
}
