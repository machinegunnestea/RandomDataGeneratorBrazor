using Bogus;
using CsvHelper.Configuration;
using CsvHelper;
using RandomDataBrazor.Models;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System;

namespace RandomDataBrazor.Controllers
{
    public class DataGenerator
    {
        Faker<PersonModel> modelFaker;
        private readonly Dictionary<string, string[]> regionData = new Dictionary<string, string[]>
        {
            { "USA", new string[] { "en_US", "en" } },
            { "Poland", new string[] { "pl-PL", "pl" } },
            { "Russia", new string[] { "ru-RU", "ru" } }
        };

        public DataGenerator()
        {
        }

        public IEnumerable<PersonModel> GeneratePeople(string region, int errorCount, int seed)
        {
            var culture = regionData[region];
            var countryCode = regionData[region][1];

            Randomizer.Seed = seed >= 1 && seed <= 1000 ? new Random(seed.GetHashCode()) : new Random();

            var fakerConfig = ConfigureFaker(countryCode);
            if (errorCount == 0)
            {
                return fakerConfig.GenerateForever();
            }
            else
            {
                var people = fakerConfig.GenerateForever()
                    .Select(person =>
                    {
                        Randomizer.Seed = seed >= 1 && seed <= 1000 ? new Random(seed.GetHashCode()) : new Random();

                        person.FullName = ApplyErrors(person.FullName, errorCount, countryCode);
                        person.Address = ApplyErrors(person.Address, errorCount, countryCode);
                        person.Phone = ApplyErrors(person.Phone, errorCount, countryCode);
                        return person;
                    });
                return people;
            }
        }

        private Faker<PersonModel> ConfigureFaker(string countryCode)
        {
            return new Faker<PersonModel>(countryCode)
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.Number, f => f.Random.Int(1, 10000))
                .RuleFor(u => u.Identifier, f => f.Random.Int(1, 10000))
                .RuleFor(u => u.Phone, f => f.Phone.PhoneNumber())
                .RuleFor(u => u.Address, f => f.Address.StreetAddress());
        }

        private string ApplyErrors(string input, int errorCount, string countryCode)
        {
            int length = input.Length;
            int numErrors = errorCount;

            for (int i = 0; i < numErrors; i++)
            {
                int errorType = Randomizer.Seed.Next(3); // 0: удаление, 1: добавление, 2: перестановка

                switch (errorType)
                {
                    case 0:
                        // Удаление символа
                        if (length > 1)
                        {
                            int indexToRemove = Randomizer.Seed.Next(length);
                            input = input.Remove(indexToRemove, 1);
                            length--;
                        }
                        break;
                    case 1:
                        // Добавление случайного символа
                        int indexToAdd = Randomizer.Seed.Next(length + 1);
                        char randomChar = GetRandomChar(countryCode);
                        input = input.Insert(indexToAdd, randomChar.ToString());
                        length++;
                        break;
                    case 2:
                        // Перестановка двух соседних символов местами
                        if (length > 1)
                        {
                            int indexToSwap = Randomizer.Seed.Next(length - 1);
                            char temp = input[indexToSwap];
                            input = input.Remove(indexToSwap, 1).Insert(indexToSwap + 1, temp.ToString());
                        }
                        break;
                }
            }

            return input;
        }
        private char GetRandomChar(string countryCode)
        {
            string characters = GetAlphabetForCountry(countryCode) + "0123456789";
            int randomIndex = Randomizer.Seed.Next(characters.Length);
            return characters[randomIndex];
        }
        private string GetAlphabetForCountry(string countryCode)
        {
            // Добавьте логику для определения алфавита страны по ее коду
            // Здесь вы можете использовать вашу существующую логику или внести изменения в зависимости от ваших требований

            // Пример:
            switch (countryCode)
            {
                case "en_US":
                case "en":
                    return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                case "pl-PL":
                case "pl":
                    return "AĄBCĆDEĘFGHIJKLŁMNŃOÓPQRSŚTUVWXYZaąbcćdeęfghijklłmnńoópqrsśtuvwxyz";
                case "ru-RU":
                case "ru":
                    return "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдеёжзийклмнопрстуфхцчшщъыьэюя";
                default:
                    return "";
            }
        }
    }
}
