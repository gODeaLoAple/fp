﻿using Autofac;
using ResultMonad;
using TagsCloudDrawer.Drawer;
using TagsCloudDrawer.ImageCreator;
using TagsCloudDrawer.ImageSaveService;
using TagsCloudDrawer.ImageSettings;
using TagsCloudVisualization.CloudLayouter;
using TagsCloudVisualization.Drawable.Tags.Factory;
using TagsCloudVisualization.Drawable.Tags.Settings;
using TagsCloudVisualization.WordsPreprocessor;
using TagsCloudVisualization.WordsProvider;
using TagsCloudVisualization.WordsToTagsTransformers;

namespace TagsCloudVisualization.Module
{
    public class TagsCloudDrawerModule : Autofac.Module
    {
        private readonly TagsCloudVisualisationSettings _settings;

        private TagsCloudDrawerModule(TagsCloudVisualisationSettings settings)
        {
            _settings = settings;
        }

        public static Result<TagsCloudDrawerModule> Create(TagsCloudVisualisationSettings settings)
        {
            return Result.Ok()
                .ValidateNonNull(settings, nameof(settings))
                .ToValue(new TagsCloudDrawerModule(settings));
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterInstance(_settings.WordsProvider).As<IWordsProvider>();
            RegisterWordPreprocessors(builder);
            builder.RegisterInstance(_settings.ImageSettingsProvider).As<IImageSettingsProvider>();
            builder.RegisterInstance(_settings.TagDrawableSettingsProvider).As<ITagDrawableSettingsProvider>();
            builder.RegisterType<Drawer>().As<IDrawer>();
            builder.RegisterInstance(_settings.ImageSaveService).As<IImageSaveService>();
            builder.RegisterInstance(_settings.Layouter).As<ILayouter>();
            builder.RegisterType<ImageCreator>().As<IImageCreator>();
            builder.RegisterType<LayoutWordsTransformer>().As<IWordsToTagsTransformer>();
            builder.RegisterType<TagDrawableFactory>().As<ITagDrawableFactory>();
            builder.RegisterType<TagsCloudVisualizer>().AsSelf();
        }

        private void RegisterWordPreprocessors(ContainerBuilder builder)
        {
            builder.RegisterType<ToLowerCasePreprocessor>().As<IWordsPreprocessor>();
            foreach (var preprocessor in _settings.WordsPreprocessors)
                builder.RegisterInstance(preprocessor).As<IWordsPreprocessor>();
            builder.RegisterInstance(new RemoveBoringPreprocessor(_settings.BoringWords)).As<IWordsPreprocessor>();
            builder.RegisterComposite<IWordsPreprocessor>((_, processors) => new CombinedPreprocessor(processors));
        }
    }
}