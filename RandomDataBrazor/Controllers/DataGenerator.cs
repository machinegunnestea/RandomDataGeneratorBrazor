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

        public IEnumerable<PersonModel> GeneratePeople(string region, double errorCount, int seed)
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

        private string ApplyErrors1(string input, double errorCount, string countryCode)
        {
            int length = input.Length;
            double numErrors = Math.Floor(errorCount);
            double additionalErrorProbability = errorCount - numErrors;

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
            // Дополнительная ошибка с вероятностью 50%
            if (additionalErrorProbability > 0 && Randomizer.Seed.NextDouble() < 0.5)
            {
                int errorType = Randomizer.Seed.Next(3);

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
        private string ApplyErrors(string input, double errorCount, string countryCode)
        {
            int length = input.Length;
            double numErrors = Math.Floor(errorCount);
            double additionalErrorProbability = errorCount - numErrors;

            Action applyError = () =>
                {
                    int errorType = Randomizer.Seed.Next(3);
                    int index;

                    switch (errorType)
                    {
                        case 0:
                            if (length > 1)
                            {
                                index = GetRandomIndex(length);
                                input = input.Remove(index, 1);
                                length--;
                            }
                            break;
                        case 1:
                            index = GetRandomIndex(length + 1);
                            char randomChar = GetRandomChar(countryCode);
                            input = input.Insert(index, randomChar.ToString());
                            length++;
                            break;
                        case 2:
                            if (length > 1)
                            {
                                index = GetRandomIndex(length - 1);
                                char temp = input[index];
                                input = input.Remove(index, 1).Insert(index + 1, temp.ToString());
                            }
                            break;
                    }
                };
            for (int i = 0; i < numErrors; i++)
            {
                applyError();
            }
            if (additionalErrorProbability > 0 && additionalErrorProbability >= Randomizer.Seed.NextDouble())
            {
                applyError();
            }
            return input;
        }
        private int GetRandomIndex(int maxIndex) => Randomizer.Seed.Next(maxIndex);
        private char GetRandomChar(string countryCode)
        {
            string characters = GetAlphabetForCountry(countryCode) + "0123456789";
            int randomIndex = Randomizer.Seed.Next(characters.Length);
            return characters[randomIndex];
        }
        private string GetAlphabetForCountry(string countryCode)
        {
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
