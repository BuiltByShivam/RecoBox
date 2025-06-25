using Microsoft.AspNetCore.Mvc;
using MovieProductRecommenderWeb.Models;
using System.Globalization;

namespace MovieProductRecommenderWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly string movieFile = "Data/ratings.csv";
        private readonly string productFile = "Data/products.csv";

        public IActionResult Index()
        {
            return View();
        }

        // Movies Page (GET only)
        public IActionResult Movies(string genre, string age)
        {
            var movies = LoadMovies();
            ViewBag.Genres = movies.Select(m => m.Genre).Distinct().ToList();
            ViewBag.SelectedGenre = genre;
            ViewBag.SelectedAge = age;

            if (!string.IsNullOrEmpty(genre) && !string.IsNullOrEmpty(age))
            {
                int cutoff = 2015;
                var filtered = movies
                    .Where(m => m.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)
                        && ((age == "new" && m.Year >= cutoff) || (age == "old" && m.Year < cutoff)))
                    .OrderByDescending(m => m.Rating)
                    .Take(5)
                    .ToList();

                ViewBag.Movies = filtered;
            }

            return View();
        }

        // Products Page (GET only)
        public IActionResult Products(string category, string age)
        {
            var products = LoadProducts();
            ViewBag.Categories = products.Select(p => p.Category).Distinct().ToList();
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedAge = age;

            if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(age))
            {
                int cutoff = 2019;
                var filtered = products
                    .Where(p => p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
                        && ((age == "new" && p.Year >= cutoff) || (age == "old" && p.Year < cutoff)))
                    .OrderByDescending(p => p.Rating)
                    .Take(5)
                    .ToList();

                ViewBag.Products = filtered;
            }

            return View();
        }

        // Surprise Page
        public IActionResult Surprise()
        {
            var movies = LoadMovies().Where(m => m.Rating >= 4.5).ToList();
            var products = LoadProducts().Where(p => p.Rating >= 4.5).ToList();

            var combined = new List<string>();
            combined.AddRange(movies.Select(m => $"Movie: {m.Name} ({m.Year}) - Rating: {m.Rating}"));
            combined.AddRange(products.Select(p => $"Product: {p.Name} - {p.Brand} ({p.Year}) - Rating: {p.Rating}"));

            ViewBag.SurpriseItem = combined.Any()
                ? combined[new Random().Next(combined.Count)]
                : "No highly rated items found!";

            return View();
        }

        // Load movie data
        private List<Movie> LoadMovies()
        {
            var movies = new List<Movie>();
            foreach (var line in System.IO.File.ReadAllLines(movieFile).Skip(1))
            {
                var parts = ParseCsvLine(line);
                if (parts.Count < 5) continue;
                movies.Add(new Movie
                {
                    ItemID = int.Parse(parts[0]),
                    Name = parts[1],
                    Genre = parts[2],
                    Year = int.Parse(parts[3]),
                    Rating = double.Parse(parts[4], CultureInfo.InvariantCulture)
                });
            }
            return movies;
        }

        // Load product data
        private List<Product> LoadProducts()
        {
            var products = new List<Product>();
            foreach (var line in System.IO.File.ReadAllLines(productFile).Skip(1))
            {
                var parts = ParseCsvLine(line);
                if (parts.Count < 6) continue;
                products.Add(new Product
                {
                    ProductID = int.Parse(parts[0]),
                    Name = parts[1],
                    Category = parts[2],
                    Brand = parts[3],
                    Year = int.Parse(parts[4]),
                    Rating = double.Parse(parts[5], CultureInfo.InvariantCulture)
                });
            }
            return products;
        }

        // Safe CSV line parser
        private List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            string current = "";

            foreach (char c in line)
            {
                if (c == '\"') inQuotes = !inQuotes;
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.Trim());
                    current = "";
                }
                else current += c;
            }

            result.Add(current.Trim());
            return result;
        }
    }
}
