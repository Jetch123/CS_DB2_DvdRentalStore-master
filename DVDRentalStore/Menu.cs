using DVDRentalStore.Config;
using DVDRentalStore.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DVDRentalStore.Tools
{
	class Menu
	{
		public void Start()
		{
			try
			{
				Print();

				while (true)
				{
					if (!Int32.TryParse(Console.ReadLine(), out int choice))
					{
						Print("Invalid option selected");
						continue;
					}

					Console.Clear();
					switch (choice)
					{
						case 1:
							Option1();
							break;

						case 2:
							Option2();
							break;

						case 3:
							Option3();
							break;

						case 4:
							Option4();
							break;

						case 5:
							Option5();
							break;

						case 6:
							Option6();
							break;

						case 7:
							Option7();
							break;

						case 8:
							Option8();
							break;

						case 9:
							Option9();
							break;

						case 0:
							Environment.Exit(0);
							break;
					}
					Print();
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("\nsError: " + e.Message);
			}
		}

		private void Option1()
		{
			Console.WriteLine("Movies:");
			Console.WriteLine("{0,35}{1,10}{2,20}{3,10}", "Title", "Year", "Age Restriction", "Price");

			var movies = Movie.GetAll();

			foreach (Movie m in movies)
			{
				Console.WriteLine("{0,35}{1,10}{2,20}{3,10}", CropString(m.Title, 35), m.Year, m.AgeRestriction, m.Price);
			}

			Console.WriteLine("\nCopies:");
			Console.WriteLine("{0,35}{1,20}", "Movie", "Availability");
			foreach (Copy c in Copy.GetAll())
				Console.WriteLine("{0,35}{1,20}", CropString(movies.First(obj => obj.Id == c.MovieId).Title, 35), c.Available == true ? "Available" : "Not Available");


			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}

		}

		private string CropString(string str, int limit)
		{
			return str.Substring(0, str.Length < limit ? str.Length : limit - 3) + (str.Length >= limit - 3 ? "..." : "");
		}


		private void Option2()
		{
			int clientId;

			while (true)
			{
				Console.Clear();
				Console.Write("In order to see client's rentals please provide client_id:");
				if (!Int32.TryParse(Console.ReadLine(), out clientId))
					continue;
				else
					break;
			}

			Console.WriteLine("\nClient:");
			var client = Client.GetAll().FirstOrDefault(obj => obj.Id == clientId);

			if (client == null)
				Console.WriteLine("Client was not found.");
			else
			{
				Console.WriteLine("{0,40}{1,30}", "Full Name", "Birthday");

				Console.WriteLine("{0,40}{1,30}", CropString($"{client.FirstName} {client.LastName}", 35), client.Birthday);


				IEnumerable<Rental> rentals = Rental.GetAll().Where(obj => obj.ClientId == clientId);

				Console.WriteLine("\nRentals:");
				Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", "Id", "Movie", "Date Of Rental", "Date Of Return");

				var resultTable = from c in Copy.GetAll()
								  join r in rentals on c.Id equals r.CopyId
								  join m in Movie.GetAll() on c.MovieId equals m.Id
								  where r.ClientId == clientId
								  select new { CopyId = r.CopyId, MovieTitle = m.Title, DateOfRental = r.DateOfRental, DateOfReturn = r.DateOfReturn };

				Console.WriteLine("Active:");

				foreach (var r in resultTable)
					if (r.DateOfReturn == null) Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", r.CopyId, CropString(r.MovieTitle, 35), r.DateOfRental, "Pending...");

				Console.WriteLine("\nPast rentals:");

				foreach (var r in resultTable)
					if (r.DateOfReturn <= DateTime.Now) Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", r.CopyId, CropString(r.MovieTitle, 35), r.DateOfRental, r.DateOfReturn);
			}

			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		private void Option3()
		{
			string errorMsg = null;
			while (true)
			{
				Console.Clear();
				Console.WriteLine("Creation of a new rental.(write exit to go back)\n");

				if (errorMsg != null)
				{
					Console.WriteLine("Error: " + errorMsg);
				}

				Console.Write("Please write a client_id:");
				String clientId = Console.ReadLine();
				if (clientId == "exit") return;

				Console.Write("Please write a copy_id:");
				String copyId = Console.ReadLine();
				if (copyId == "exit") return;

				try
				{
					Copy copy = Copy.GetByID(Int32.Parse(copyId));
					if (!copy.Available) throw new Exception("Copy is not available now.");

					Rental rental = new Rental(Int32.Parse(copyId), Int32.Parse(clientId), DateTime.Now);
					rental.InsertAndSave();

					copy.Available = false;
					copy.Save();
				}
				catch (Exception e)
				{
					errorMsg = e.Message;
					continue;
				}

				Console.WriteLine("Success! Rental was created!");
				break;
			}

			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		private void Option4()
		{
			Console.WriteLine("Register return:");

			int clientId;

			while (true)
			{
				Console.Clear();
				Console.Write("Please provide client_id:");
				if (!Int32.TryParse(Console.ReadLine(), out clientId))
					continue;
				else
					break;
			}

			Console.WriteLine("\nClient:");
			Client client = Client.GetAll().Where(obj => obj.Id == clientId).FirstOrDefault();

			if (client == null)
				Console.WriteLine("Client was not found.");
			else
			{
				Console.WriteLine("{0,40}{1,30}", "Full Name", "Birthday");

				Console.WriteLine("{0,40}{1,30}", CropString($"{client.FirstName} {client.LastName}", 35), client.Birthday);
			}

			int copyId;

			while (true)
			{
				Console.Write("\nPlease provide copy_id:");
				if (!Int32.TryParse(Console.ReadLine(), out copyId))
					continue;
				else
					break;
			}

			Copy copy = Copy.GetAll().Where(obj => obj.Id == copyId).FirstOrDefault();

			if (copy == null)
				Console.WriteLine("Copy was not found.");
			else
			{
				Rental rental = Rental.GetAll().Where(obj => obj.CopyId == copyId && obj.ClientId == clientId).FirstOrDefault();

				if (rental == null)
					Console.WriteLine("\nRental was not found.");
				else if (rental.DateOfReturn != null)
					Console.WriteLine("\nRental is already returned.");
				else
				{
					rental.Return();

					Console.WriteLine("\nCopy was successfully returned.");
				}
			}

			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		private void Option5()
		{
			Console.WriteLine("\nClients:");
			Console.WriteLine("{0,10}{1,40}{2,20}", "Id", "Full Name", "Birthday");

			foreach (Client client in Client.GetAll().OrderBy(obj => obj.Id))
				Console.WriteLine("{0,10}{1,40}{2,20}", client.Id, CropString($"{client.FirstName} {client.LastName}", 35), client.Birthday.ToShortDateString());


			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		private void Option6()
		{
			string errorMsg = null;
			while (true)
			{
				Console.Clear();
				Console.WriteLine("Creation of a new user.(write exit to go back)\n");

				if (errorMsg != null)
					Console.WriteLine("Error: " + errorMsg);

				Console.Write("Please write a first name of the user:");
				String firstName = Console.ReadLine();
				if (firstName == "exit") return;

				Console.Write("Please write a last name of the user:");
				String lastName = Console.ReadLine();
				if (lastName == "exit") return;

				Console.Write("Please write a birthday of the user(yyyy/mm/dd):");
				String birthday = Console.ReadLine();
				if (birthday == "exit") return;

				try
				{
					Client client = new Client(firstName, lastName, DateTime.Parse(birthday));
					client.Save();
				}
				catch (Exception e)
				{
					errorMsg = e.Message;
					continue;
				}

				Console.WriteLine("Success! Client was created!");
				break;
			}

			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		private void Option7()
		{
			string errorMsg = null;
			while (true)
			{
				Console.Clear();
				Console.WriteLine("Creation of a new movie.(write exit to go back)\n");

				if (errorMsg != null)
					Console.WriteLine("Error: " + errorMsg);

				Console.Write("Please write a title:");
				String title = Console.ReadLine();
				if (title == "exit") return;

				Console.Write("Please write a year:");
				String year = Console.ReadLine();
				if (year == "exit") return;

				Console.Write("Please write an age restriction:");
				String ageRestriction = Console.ReadLine();
				if (ageRestriction == "exit") return;

				Console.Write("Please write a price:");
				String price = Console.ReadLine();
				if (price == "exit") return;

				NpgsqlTransaction transaction = null;
				NpgsqlConnection connection = null;

				try
				{
					connection = new NpgsqlConnection(SysConfig.CONNECTION_STRING);
					connection.Open();
					transaction = connection.BeginTransaction();

					Movie movie = new Movie(title, Int32.Parse(year), Int32.Parse(ageRestriction), Int32.Parse(price));
					movie.Save();

					Copy copy = new Copy(true, movie.Id);
					copy.Save();

					transaction.Commit();
				}
				catch (Exception e)
				{
					errorMsg = e.Message;

					transaction?.Rollback();
					continue;
				}
				finally
				{
					connection?.Close();
				}

				Console.WriteLine("Success! Movie was created!");
				break;
			}

			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		private void Option8()
		{
			Console.WriteLine("Overdue rentals:");

			var overdueRentals = Rental.GetAll().Where(obj => obj.DateOfReturn == null && (DateTime.Now - obj.DateOfRental).TotalDays > 14);

			Console.WriteLine("\nRentals:");
			Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", "Id", "Movie", "Date Of Rental", "Date Of Return");

			foreach (var rental in overdueRentals)
				Console.WriteLine("{0,10}{1,40}{2,30}{3,30}", rental.CopyId, rental.ClientId, rental.DateOfRental, "Pending...");

			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		private void Option9()
		{
			string errorMsg = null;
			while (true)
			{
				Console.Clear();
				Console.Write("Statistics\n\n");

				if (errorMsg != null)
					Console.WriteLine($"Error: {errorMsg}\n");

				Console.WriteLine("Write the date from what you want to see stats");
				Console.WriteLine("Write all - to see for the all time. Format(yyyy/mm/dd)");
				Console.Write("Input: ");
				String date = Console.ReadLine();

				try
				{
					DateTime dt = date == "all" ? DateTime.Parse("1000/1/1") : DateTime.Parse(date);

					var resultTable = from c in Copy.GetAll()
									  join r in Rental.GetAll() on c.Id equals r.CopyId
									  join m in Movie.GetAll() on c.MovieId equals m.Id
									  where r.DateOfRental > dt
									  select new { Price = m.Price };

					Console.WriteLine($"\nTotal rentals: {resultTable.Count()}");
					Console.WriteLine($"Total price of rented movies: {resultTable.Sum(obj => obj.Price)}$");
				}
				catch (Exception e)
				{
					errorMsg = e.Message;
					continue;
				}

				break;
			}

			while (true)
			{
				Console.Write("\n\nPress ESC to go back...");
				if (Console.ReadKey().Key == ConsoleKey.Escape) return;
			}
		}

		public void Print(string msg = "")
		{
			Console.Clear();

			Console.WriteLine("Welcome to the DVD store management application\n");
			Console.WriteLine("Please select an option from the list:\n");
			Console.WriteLine("-----------------------");
			Console.WriteLine("1. Current offers");
			Console.WriteLine("-----------------------");
			Console.WriteLine("2. View client rentals");
			Console.WriteLine("-----------------------");
			Console.WriteLine("3. Create new rental");
			Console.WriteLine("-----------------------");
			Console.WriteLine("4. Register return");
			Console.WriteLine("-----------------------");
			Console.WriteLine("5. View all clients");
			Console.WriteLine("-----------------------");
			Console.WriteLine("6. Add new user");
			Console.WriteLine("-----------------------");
			Console.WriteLine("7. Add new movie");
			Console.WriteLine("-----------------------");
			Console.WriteLine("8. Show overdue rentals");
			Console.WriteLine("-----------------------");
			Console.WriteLine("9. Show statistics");
			Console.WriteLine("-----------------------");

			Console.WriteLine("\n0. Exit");

			if (msg != "")
				Console.WriteLine("\n\nError: " + msg);

			Console.Write("\nInput:");
		}
	}
}
