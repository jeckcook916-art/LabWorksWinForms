using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LabWorksWinForms
{
    // ЗАДАНИЕ 1. ПАТТЕРН BUILDER
    public class Article
    {
        public string Title { get; set; } = "";
        public List<string> Authors { get; set; } = new List<string>();
        public string Text { get; set; } = "";
        public string HashCode { get; set; } = "";
    }

    public interface IArticleBuilder
    {
        void SetTitle(string title);
        void SetAuthors(List<string> authors);
        void SetText(string text);
        void SetHashCode(string hash);
        Article Build();
        void Reset();
    }

    public class TxtArticleBuilder : IArticleBuilder
    {
        private Article _article = new Article();
        public void SetTitle(string title) => _article.Title = title;
        public void SetAuthors(List<string> authors) => _article.Authors = authors ?? new List<string>();
        public void SetText(string text) => _article.Text = text;
        public void SetHashCode(string hash) => _article.HashCode = hash;
        public Article Build() => _article;
        public void Reset() => _article = new Article();
    }

    public class ArticleDirector
    {
        private readonly IArticleBuilder _builder;
        public ArticleDirector(IArticleBuilder builder) => _builder = builder;

        public Article ConstructFromTxt(string content)
        {
            _builder.Reset();
            string title = "";
            string hash = "";
            var authors = new List<string>();
            var textBuilder = new StringBuilder();
            string section = "";

            var lines = content.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');

            foreach (var line in lines)
            {
                if (line.StartsWith("Title:", StringComparison.OrdinalIgnoreCase))
                {
                    section = "title";
                    title = line.Substring("Title:".Length).Trim();
                }
                else if (line.StartsWith("Authors:", StringComparison.OrdinalIgnoreCase))
                {
                    section = "authors";
                    authors = line.Substring("Authors:".Length)
                        .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim())
                        .Where(a => a.Length > 0)
                        .ToList();
                }
                else if (line.StartsWith("Text:", StringComparison.OrdinalIgnoreCase))
                {
                    section = "text";
                    string first = line.Substring("Text:".Length).TrimStart();
                    if (!string.IsNullOrEmpty(first)) textBuilder.Append(first).Append('\n');
                }
                else if (line.StartsWith("Hash:", StringComparison.OrdinalIgnoreCase))
                {
                    section = "hash";
                    hash = line.Substring("Hash:".Length).Trim();
                }
                else if (section == "text")
                {
                    textBuilder.Append(line).Append('\n');
                }
            }

            string text = textBuilder.ToString().TrimEnd('\n');
            _builder.SetTitle(title);
            _builder.SetAuthors(authors);
            _builder.SetText(text);
            _builder.SetHashCode(hash);
            return _builder.Build();
        }
    }

    public static class HashUtil
    {
        public static string ComputeHash(string? text)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty));
                return Convert.ToHexString(bytes);
            }
        }
    }

    public static class ArticleJsonConverter
    {
        public static string ToJson(Article a)
        {
            return JsonSerializer.Serialize(a, new JsonSerializerOptions { WriteIndented = true });
        }

        public static bool ValidateHash(Article? a)
        {
            if (a == null || string.IsNullOrWhiteSpace(a.HashCode)) return false;
            string computed = HashUtil.ComputeHash(a.Text);
            return string.Equals(computed, a.HashCode, StringComparison.OrdinalIgnoreCase);
        }
    }
    // ЗАДАНИЕ 2. ПАТТЕРН ABSTRACT FACTORY
    public class Film
    {
        public string Name { get; set; } = "";
        public string AudioLanguage { get; set; } = "";
        public string SubtitleLanguage { get; set; } = "";

        public override string ToString()
        {
            return $"Фильм \"{Name}\" | Звук: {AudioLanguage} | Субтитры: {SubtitleLanguage}";
        }
    }

    public interface IFilmFactory
    {
        Film CreateFilm(string name);
    }

    public class RussianFilmFactory : IFilmFactory
    {
        public Film CreateFilm(string name) => new Film { Name = name, AudioLanguage = "Russian", SubtitleLanguage = "Russian" };
    }

    public class EnglishFilmFactory : IFilmFactory
    {
        public Film CreateFilm(string name) => new Film { Name = name, AudioLanguage = "English", SubtitleLanguage = "English" };
    }

    public class GermanFilmFactory : IFilmFactory
    {
        public Film CreateFilm(string name) => new Film { Name = name, AudioLanguage = "German", SubtitleLanguage = "German" };
    }

    public class FilmRental
    {
        public Film OrderFilm(string name, string language)
        {
            IFilmFactory factory;
            string lang = language.ToLower();

            if (lang == "russian" || lang == "ru" || lang == "русский") factory = new RussianFilmFactory();
            else if (lang == "english" || lang == "en" || lang == "английский") factory = new EnglishFilmFactory();
            else if (lang == "german" || lang == "de" || lang == "немецкий") factory = new GermanFilmFactory();
            else throw new ArgumentException($"Язык '{language}' не поддерживается");

            return factory.CreateFilm(name);
        }
    }
    // ЗАДАНИЕ 3. ПАТТЕРН FACTORY METHOD + PROTOTYPE Фигуры
    public abstract class Shape : ICloneable
    {
        public string Name { get; set; } = "";
        public List<Tuple<int, int>> Cells { get; set; } = new List<Tuple<int, int>>();
        public bool IsSuper { get; set; }

        public abstract Shape Clone();
        object ICloneable.Clone() => Clone();

        public override string ToString()
        {
            return $"{Name} [{(IsSuper ? "SUPER" : "normal")}] клеток: {Cells.Count}";
        }
    }

    public class NormalShape : Shape
    {
        public NormalShape() { IsSuper = false; }
        public override Shape Clone() => new NormalShape { Name = Name, Cells = new List<Tuple<int, int>>(Cells), IsSuper = IsSuper };
    }

    public class SuperShape : Shape
    {
        public SuperShape() { IsSuper = true; }
        public override Shape Clone() => new SuperShape { Name = Name, Cells = new List<Tuple<int, int>>(Cells), IsSuper = IsSuper };
    }

    public abstract class ShapeFactory
    {
        public abstract Shape CreateShape();
    }

    public class TetrominoFactory : ShapeFactory
    {
        private static readonly Random rnd = new Random();
        private static readonly List<Tuple<int, int>[]> Tetrominoes = new List<Tuple<int, int>[]>
        {
            new[] { Tuple.Create(0,0), Tuple.Create(1,0), Tuple.Create(2,0), Tuple.Create(3,0) },
            new[] { Tuple.Create(0,0), Tuple.Create(0,1), Tuple.Create(1,1), Tuple.Create(1,2) },
            new[] { Tuple.Create(0,0), Tuple.Create(1,0), Tuple.Create(1,1), Tuple.Create(2,1) },
            new[] { Tuple.Create(0,0), Tuple.Create(0,1), Tuple.Create(0,2), Tuple.Create(1,2) },
            new[] { Tuple.Create(0,0), Tuple.Create(1,0), Tuple.Create(0,1), Tuple.Create(1,1) },
            new[] { Tuple.Create(0,0), Tuple.Create(1,0), Tuple.Create(2,0), Tuple.Create(1,1) },
            new[] { Tuple.Create(0,0), Tuple.Create(0,1), Tuple.Create(0,2), Tuple.Create(1,0) },
        };

        public override Shape CreateShape()
        {
            var cells = Tetrominoes[rnd.Next(Tetrominoes.Count)];
            return new NormalShape { Name = "Tetromino", Cells = cells.ToList() };
        }
    }

    public class SuperShapeFactory : ShapeFactory
    {
        private static readonly Random rnd = new Random();
        private static readonly List<Tuple<int, int>[]> SuperShapes = new List<Tuple<int, int>[]>
        {
            new[] { Tuple.Create(0,0), Tuple.Create(1,0), Tuple.Create(2,0), Tuple.Create(3,0), Tuple.Create(0,1), Tuple.Create(3,1) },
            new[] { Tuple.Create(0,0), Tuple.Create(1,0), Tuple.Create(2,0), Tuple.Create(3,0), Tuple.Create(4,0), Tuple.Create(0,1), Tuple.Create(4,1) },
            new[] { Tuple.Create(0,0), Tuple.Create(1,0), Tuple.Create(1,1), Tuple.Create(2,1), Tuple.Create(2,2), Tuple.Create(3,2), Tuple.Create(3,3), Tuple.Create(4,3) },
        };

        public override Shape CreateShape()
        {
            var cells = SuperShapes[rnd.Next(SuperShapes.Count)];
            return new SuperShape { Name = "SuperShape", Cells = cells.ToList() };
        }
    }

    public class ShapeGenerator
    {
        private static readonly Random rnd = new Random();
        private readonly TetrominoFactory _normal = new TetrominoFactory();
        private readonly SuperShapeFactory _super = new SuperShapeFactory();

        public Shape GenerateRandom()
        {
            return rnd.Next(100) < 30 ? _super.CreateShape() : _normal.CreateShape();
        }
    }
    // ЗАДАНИЕ 4. ПАТТЕРН SINGLETON Лифт в здании
    public sealed class Elevator
    {
        private static readonly Lazy<Elevator> _instance = new Lazy<Elevator>(() => new Elevator());
        public static Elevator Instance => _instance.Value;

        public int CurrentFloor { get; private set; }
        public bool IsMoving { get; private set; }
        public event Action<int, int> FloorChanged = delegate { };

        private Elevator()
        {
            CurrentFloor = 1;
            IsMoving = false;
        }

        public void MoveTo(int floor)
        {
            IsMoving = true;
            int from = CurrentFloor;
            CurrentFloor = floor;
            IsMoving = false;
            FloorChanged(from, floor);
        }
    }

    public sealed class Building
    {
        private static readonly Lazy<Building> _instance = new Lazy<Building>(() => new Building());
        public static Building Instance => _instance.Value;

        public string Name { get; set; } = "Главное здание";
        public List<Floor> Floors { get; } = new List<Floor>();
        public Elevator Elevator => Elevator.Instance;

        private Building() { }

        public void AddFloor(Floor f) => Floors.Add(f);

        public void Reset()
        {
            Floors.Clear();
            Elevator.MoveTo(1);
        }
    }

    public class Floor
    {
        public int Number { get; set; }
        public List<Room> Rooms { get; } = new List<Room>();
        public Elevator Elevator => Elevator.Instance;

        public Floor(int number) => Number = number;
        public void AddRoom(Room r) => Rooms.Add(r);
    }

    public class Room
    {
        public string Name { get; set; } = "";
        public Room(string name) => Name = name;
    }
}