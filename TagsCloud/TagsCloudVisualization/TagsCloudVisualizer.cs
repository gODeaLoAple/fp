﻿using System;
using System.Linq;
using TagsCloudVisualization.Drawable.Displayer;
using TagsCloudVisualization.Drawable.Tags.Factory;
using TagsCloudVisualization.WordsPreprocessor;
using TagsCloudVisualization.WordsProvider;
using TagsCloudVisualization.WordsToTagsTransformers;

namespace TagsCloudVisualization
{
    public class TagsCloudVisualizer
    {
        private readonly IWordsProvider _wordsProvider;
        private readonly IWordsPreprocessor _preprocessor;
        private readonly ITagDrawableFactory _tagDrawableFactory;
        private readonly IWordsToTagsTransformer _transformer;
        private readonly IDrawableDisplayer _displayer;

        public TagsCloudVisualizer(IWordsProvider wordsProvider,
            IWordsPreprocessor preprocessor,
            ITagDrawableFactory tagDrawableFactory,
            IWordsToTagsTransformer transformer,
            IDrawableDisplayer displayer)
        {
            _wordsProvider = wordsProvider ?? throw new ArgumentNullException(nameof(wordsProvider));
            _preprocessor = preprocessor ?? throw new ArgumentNullException(nameof(preprocessor));
            _tagDrawableFactory = tagDrawableFactory ?? throw new ArgumentNullException(nameof(tagDrawableFactory));
            _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            _displayer = displayer ?? throw new ArgumentNullException(nameof(displayer));
        }

        public void Visualize(int limit = int.MaxValue)
        {
            if (limit <= 0)
                throw new ArgumentException($"Expected {nameof(limit)} to be positive, but actual {limit}");
            var words = _wordsProvider.GetWords();
            var processedWords = _preprocessor.Process(words);
            var tags = _transformer.Transform(processedWords);
            var drawables = tags
                .OrderByDescending(tag => tag.Weight)
                .Select(_tagDrawableFactory.Create)
                .Take(limit);
            _displayer.Display(drawables);
        }
    }
}