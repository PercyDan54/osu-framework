﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Framework.Caching;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace osu.Framework.Graphics.Containers
{
    /// <summary>
    /// A drawable text object that supports more advanced text formatting.
    /// </summary>
    public class TextFlowContainer : FillFlowContainer
    {
        private float firstLineIndent;
        private readonly Action<SpriteText> defaultCreationParameters;

        /// <summary>
        /// An indent value for the first (header) line of a paragraph.
        /// </summary>
        public float FirstLineIndent
        {
            get { return firstLineIndent; }
            set
            {
                if (value == firstLineIndent) return;
                firstLineIndent = value;

                layout.Invalidate();
            }
        }

        private float contentIndent;

        /// <summary>
        /// An indent value for all lines proceeding the first line in a paragraph.
        /// </summary>
        public float ContentIndent
        {
            get { return contentIndent; }
            set
            {
                if (value == contentIndent) return;
                contentIndent = value;

                layout.Invalidate();
            }
        }

        private float paragraphSpacing = 0.5f;

        /// <summary>
        /// Vertical space between paragraphs (i.e. text separated by '\n') in multiples of the text size.
        /// The default value is 0.5.
        /// </summary>
        public float ParagraphSpacing
        {
            get { return paragraphSpacing; }
            set
            {
                if (value == paragraphSpacing) return;
                paragraphSpacing = value;

                layout.Invalidate();
            }
        }

        private float lineSpacing;

        /// <summary>
        /// Vertical space between lines both when a new paragraph begins and when line wrapping occurs.
        /// Additive with <see cref="ParagraphSpacing"/> on new paragraph. Default value is 0.
        /// </summary>
        public float LineSpacing
        {
            get { return lineSpacing; }
            set
            {
                if (value == lineSpacing) return;
                lineSpacing = value;

                layout.Invalidate();
            }
        }

        /// <summary>
        /// An easy way to set the full text of a text flow in one go.
        /// This will overwrite any existing text added using this method of <see cref="AddText(string, Action{SpriteText})"/>
        /// </summary>
        public string Text
        {
            set
            {
                Clear();
                AddText(value);
            }
        }

        public override bool HandleInput => false;

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) > 0)
                layout.Invalidate();
            return base.Invalidate(invalidation, source, shallPropagate);
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!layout.IsValid)
            {
                computeLayout();
                layout.Validate();
            }
        }

        /// <summary>
        /// Add new text to this text flow. The \n character will create a new paragraph, not just a line break. If you need \n to be a line break, use <see cref="AddParagraph(string, Action{SpriteText})"/> instead.
        /// </summary>
        /// <returns>A collection of the <see cref="SpriteText" /> objects for each word created from the given text.</returns>
        /// <param name="text">The text to add.</param>
        /// <param name="creationParameters">A callback providing any <see cref="SpriteText" /> instances created for this new text.</param>
        public IEnumerable<SpriteText> AddText(string text, Action<SpriteText> creationParameters = null) => AddLine(new TextLine(text, creationParameters), true);

        /// <summary>
        /// Add a new paragraph to this text flow. The \n character will create a line break. If you need \n to be a new paragraph, not just a line break, use <see cref="AddText(string, Action{SpriteText})"/> instead.
        /// </summary>
        /// <returns>A collection of the <see cref="SpriteText" /> objects for each word created from the given text.</returns>
        /// <param name="paragraph">The paragraph to add.</param>
        /// <param name="creationParameters">A callback providing any <see cref="SpriteText" /> instances created for this new paragraph.</param>
        public IEnumerable<SpriteText> AddParagraph(string paragraph, Action<SpriteText> creationParameters = null) => AddLine(new TextLine(paragraph, creationParameters), false);

        /// <summary>
        /// End current line and start a new one.
        /// </summary>
        public void NewLine() => base.Add(new NewLineContainer(false));

        /// <summary>
        /// End current paragraph and start a new one.
        /// </summary>
        public void NewParagraph() => base.Add(new NewLineContainer(true));

        public TextFlowContainer(Action<SpriteText> defaultCreationParameters = null)
        {
            this.defaultCreationParameters = defaultCreationParameters;
        }

        protected virtual SpriteText CreateSpriteText() => new SpriteText();

        internal SpriteText CreateSpriteTextWithLine(TextLine line)
        {
            var spriteText = CreateSpriteText();
            defaultCreationParameters?.Invoke(spriteText);
            line.ApplyParameters(spriteText);
            return spriteText;
        }

        public override void Add(Drawable drawable)
        {
            throw new InvalidOperationException($"Use {nameof(AddText)} to add text to a {nameof(TextFlowContainer)}.");
        }

        internal virtual IEnumerable<SpriteText> AddLine(TextLine line, bool newLineIsParagraph)
        {
            bool first = true;
            var sprites = new List<SpriteText>();

            // !newLineIsParagraph effectively means that we want to add just *one* paragraph, which means we need to make sure that any previous paragraphs
            // are terminated. Thus, we add a NewLineContainer that indicates the end of the paragraph before adding our current paragraph.
            if (!newLineIsParagraph)
                base.Add(new NewLineContainer(true));

            foreach (string l in line.Text.Split('\n'))
            {
                if (!first)
                {
                    var lastChild = Children.LastOrDefault();
                    if (lastChild != null)
                        base.Add(new NewLineContainer(newLineIsParagraph));
                }

                foreach (string word in SplitWords(l))
                {
                    if (string.IsNullOrEmpty(word)) continue;

                    var textSprite = CreateSpriteTextWithLine(line);
                    textSprite.Text = word;
                    sprites.Add(textSprite);
                    base.Add(textSprite);
                }

                first = false;
            }

            return sprites;
        }

        protected string[] SplitWords(string text)
        {
            var words = new List<string>();
            var builder = new StringBuilder();

            for (var i = 0; i < text.Length; i++)
            {
                if (i == 0 || char.IsSeparator(text[i - 1]) || char.IsControl(text[i - 1]))
                {
                    words.Add(builder.ToString());
                    builder.Clear();
                }

                builder.Append(text[i]);
            }

            if (builder.Length > 0)
                words.Add(builder.ToString());

            return words.ToArray();
        }

        private Cached layout = new Cached();

        private void computeLayout()
        {
            List<List<Drawable>> childrenByLine = new List<List<Drawable>>();
            List<Drawable> curLine = new List<Drawable>();
            foreach (var c in Children)
            {
                NewLineContainer nlc = c as NewLineContainer;
                if (nlc != null)
                {
                    curLine.Add(nlc);
                    childrenByLine.Add(curLine);
                    curLine = new List<Drawable>();
                }
                else
                {
                    if (c.X == 0)
                    {
                        if (curLine.Count > 0)
                            childrenByLine.Add(curLine);
                        curLine = new List<Drawable>();
                    }
                    curLine.Add(c);
                }
            }

            if (curLine.Count > 0)
                childrenByLine.Add(curLine);

            bool isFirstLine = true;
            float lastLineHeight = 0f;
            foreach (List<Drawable> line in childrenByLine)
            {
                bool isFirstChild = true;
                IEnumerable<float> lineBaseHeightValues = line.OfType<IHasLineBaseHeight>().Select(l => l.LineBaseHeight);
                float lineBaseHeight = lineBaseHeightValues.Any() ? lineBaseHeightValues.Max() : 0f;
                float currentLineHeight = 0f;
                float lineSpacingValue = lastLineHeight * LineSpacing;

                foreach (Drawable c in line)
                {
                    NewLineContainer nlc = c as NewLineContainer;
                    if (nlc != null)
                    {
                        nlc.Height = nlc.IndicatesNewParagraph ? (currentLineHeight == 0 ? lastLineHeight : currentLineHeight) * ParagraphSpacing : 0;
                        continue;
                    }

                    float childLineBaseHeight = (c as IHasLineBaseHeight)?.LineBaseHeight ?? 0f;
                    MarginPadding margin = new MarginPadding { Top = (childLineBaseHeight != 0f ? lineBaseHeight - childLineBaseHeight : 0f) + lineSpacingValue };
                    if (isFirstLine)
                        margin.Left = FirstLineIndent;
                    else if (isFirstChild)
                        margin.Left = ContentIndent;

                    c.Margin = margin;

                    if (c.Height > currentLineHeight)
                        currentLineHeight = c.Height;

                    isFirstChild = false;
                }

                if (currentLineHeight != 0f)
                    lastLineHeight = currentLineHeight;

                isFirstLine = false;
            }
        }

        internal class NewLineContainer : Container
        {
            public readonly bool IndicatesNewParagraph;

            public NewLineContainer(bool newParagraph)
            {
                RelativeSizeAxes = Axes.X;
                IndicatesNewParagraph = newParagraph;
            }
        }

        internal class TextLine
        {
            public readonly string Text;
            private readonly Action<SpriteText> creationParameters;

            public TextLine(string text, Action<SpriteText> creationParameters = null)
            {
                Text = text;
                this.creationParameters = creationParameters;
            }

            public void ApplyParameters(SpriteText spriteText)
            {
                creationParameters?.Invoke(spriteText);
            }
        }
    }
}
