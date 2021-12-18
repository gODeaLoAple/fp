﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autofac;
using NUnit.Framework;
using TagsCloudVisualization.Module;

namespace TagsCloudVisualization.Tests
{
    [TestFixture]
    public class IntegrationTests
    {
        private const string ImageFile = "TagsCloud";

        private readonly TagsCloudVisualisationSettings _settings = new()
        {
            WordsFile = Path.Combine(Directory.GetCurrentDirectory(), "Words.txt"),
            BoringWords = new[] { "a", "b", "c" }
        };

        [OneTimeSetUp]
        public void BeforeAll()
        {
            File.Create(_settings.WordsFile).Close();
        }

        [OneTimeTearDown]
        public void AfterAll()
        {
            File.Delete(_settings.WordsFile);
        }

        [Test]
        public void TagsCloudVisualisation_ShouldCreateImageWithTagsCloud()
        {
            var uniqueWords = new[]
                {
                    "A",
                    "B",
                    "C"
                }.Concat(Enumerable.Range(0, 100).Select(i => $"Word{i}"))
                .ToArray();
            var generated = GenerateWordsList(uniqueWords, 1000);
            File.WriteAllLines(_settings.WordsFile, generated);

            var container = new ContainerBuilder()
                .RegisterTagsClouds(_settings)
                .RegisterImageCreation(ImageFile)
                .Build();

            var visualizer = container.Resolve<TagsCloudVisualizer>();
            visualizer.Visualize();
        }

        private static IEnumerable<string> GenerateWordsList(IList<string> uniqueWords, int count)
        {
            var rnd = new Random();
            return Enumerable.Range(0, count).Select(_ => uniqueWords[rnd.Next(uniqueWords.Count)]);
        }
    }
}