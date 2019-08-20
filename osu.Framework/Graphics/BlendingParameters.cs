﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK.Graphics.ES30;

namespace osu.Framework.Graphics
{
    /// <summary>
    /// Contains information about how an <see cref="IDrawable"/> should be blended into its destination.
    /// </summary>
    public struct BlendingParameters : IEquatable<BlendingParameters>
    {
        #region Public Members

        /// <summary>
        /// The blending factor for the source color of the blend.
        /// </summary>
        public BlendingType Source;

        /// <summary>
        /// The blending factor for the destination color of the blend.
        /// </summary>
        public BlendingType Destination;

        /// <summary>
        /// The blending factor for the source alpha of the blend.
        /// </summary>
        public BlendingType SourceAlpha;

        /// <summary>
        /// The blending factor for the destination alpha of the blend.
        /// </summary>
        public BlendingType DestinationAlpha;

        /// <summary>
        /// Gets or sets the <see cref="BlendingEquation"/> to use for the RGB components of the blend.
        /// </summary>
        public BlendingEquation RGBEquation;

        /// <summary>
        /// Gets or sets the <see cref="BlendingEquation"/> to use for the alpha component of the blend.
        /// </summary>
        public BlendingEquation AlphaEquation;

        #endregion

        #region Default Blending Parameter Types

        public static BlendingParameters None => new BlendingParameters
        {
            Source = BlendingType.One,
            Destination = BlendingType.Zero,
            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.Zero,
            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add,
        };

        public static BlendingParameters Inherit => new BlendingParameters
        {
            Source = BlendingType.Inherit,
            Destination = BlendingType.Inherit,
            SourceAlpha = BlendingType.Inherit,
            DestinationAlpha = BlendingType.Inherit,
            RGBEquation = BlendingEquation.Inherit,
            AlphaEquation = BlendingEquation.Inherit,
        };

        public static BlendingParameters Mixture => new BlendingParameters
        {
            Source = BlendingType.SrcAlpha,
            Destination = BlendingType.OneMinusSrcAlpha,
            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.One,
            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add,
        };

        public static BlendingParameters Additive => new BlendingParameters
        {
            Source = BlendingType.SrcAlpha,
            Destination = BlendingType.One,
            SourceAlpha = BlendingType.One,
            DestinationAlpha = BlendingType.One,
            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add,
        };

        #endregion

        #region GL Type Getters

        /// <summary>
        /// Gets the <see cref="BlendEquationMode"/> for the currently specified RGB Equation.
        /// </summary>
        public BlendEquationMode RGBEquationMode => translateEquation(RGBEquation);

        /// <summary>
        /// Gets the <see cref="BlendEquationMode"/> for the currently specified Alpha Equation.
        /// </summary>
        public BlendEquationMode AlphaEquationMode => translateEquation(AlphaEquation);

        /// <summary>
        /// Gets the <see cref="BlendingFactorSrc"/> for the currently specified source blending mode.
        /// </summary>
        public BlendingFactorSrc SourceBlendingFactor => translateBlendingFactorSrc(Source);

        /// <summary>
        /// Gets the <see cref="BlendingFactorDest"/> for the currently specified destination blending mode.
        /// </summary>
        public BlendingFactorDest DestinationBlendingFactor => translateBlendingFactorDest(Destination);

        /// <summary>
        /// Gets the <see cref="BlendingFactorSrc"/> for the currently specified source alpha mode.
        /// </summary>
        public BlendingFactorSrc SourceAlphaBlendingFactor => translateBlendingFactorSrc(SourceAlpha);

        /// <summary>
        /// Gets the <see cref="BlendingFactorDest"/> for the currently specified destination alpha mode.
        /// </summary>
        public BlendingFactorDest DestinationAlphaBlendingFactor => translateBlendingFactorDest(DestinationAlpha);

        private static BlendingFactorSrc translateBlendingFactorSrc(BlendingType factor)
        {
            switch (factor)
            {
                case BlendingType.ConstantAlpha:
                    return BlendingFactorSrc.ConstantAlpha;

                case BlendingType.ConstantColor:
                    return BlendingFactorSrc.ConstantColor;

                case BlendingType.DstAlpha:
                    return BlendingFactorSrc.DstAlpha;

                case BlendingType.DstColor:
                    return BlendingFactorSrc.DstColor;

                case BlendingType.One:
                    return BlendingFactorSrc.One;

                case BlendingType.OneMinusConstantAlpha:
                    return BlendingFactorSrc.OneMinusConstantAlpha;

                case BlendingType.OneMinusConstantColor:
                    return BlendingFactorSrc.OneMinusConstantColor;

                case BlendingType.OneMinusDstAlpha:
                    return BlendingFactorSrc.OneMinusDstAlpha;

                case BlendingType.OneMinusDstColor:
                    return BlendingFactorSrc.OneMinusDstColor;

                case BlendingType.OneMinusSrcAlpha:
                    return BlendingFactorSrc.OneMinusSrcColor;

                case BlendingType.SrcAlpha:
                    return BlendingFactorSrc.SrcAlpha;

                case BlendingType.SrcAlphaSaturate:
                    return BlendingFactorSrc.SrcAlphaSaturate;

                case BlendingType.SrcColor:
                    return BlendingFactorSrc.SrcColor;

                default:
                case BlendingType.Zero:
                    return BlendingFactorSrc.Zero;
            }
        }

        private static BlendingFactorDest translateBlendingFactorDest(BlendingType factor)
        {
            switch (factor)
            {
                case BlendingType.ConstantAlpha:
                    return BlendingFactorDest.ConstantAlpha;

                case BlendingType.ConstantColor:
                    return BlendingFactorDest.ConstantColor;

                case BlendingType.DstAlpha:
                    return BlendingFactorDest.DstAlpha;

                case BlendingType.DstColor:
                    return BlendingFactorDest.DstColor;

                case BlendingType.One:
                    return BlendingFactorDest.One;

                case BlendingType.OneMinusConstantAlpha:
                    return BlendingFactorDest.OneMinusConstantAlpha;

                case BlendingType.OneMinusConstantColor:
                    return BlendingFactorDest.OneMinusConstantColor;

                case BlendingType.OneMinusDstAlpha:
                    return BlendingFactorDest.OneMinusDstAlpha;

                case BlendingType.OneMinusDstColor:
                    return BlendingFactorDest.OneMinusDstColor;

                case BlendingType.OneMinusSrcAlpha:
                    return BlendingFactorDest.OneMinusSrcAlpha;

                case BlendingType.OneMinusSrcColor:
                    return BlendingFactorDest.OneMinusSrcColor;

                case BlendingType.SrcAlpha:
                    return BlendingFactorDest.SrcAlpha;

                case BlendingType.SrcAlphaSaturate:
                    return BlendingFactorDest.SrcAlphaSaturate;

                case BlendingType.SrcColor:
                    return BlendingFactorDest.SrcColor;

                default:
                case BlendingType.Zero:
                    return BlendingFactorDest.Zero;
            }
        }

        private static BlendEquationMode translateEquation(BlendingEquation blendingEquation)
        {
            switch (blendingEquation)
            {
                default:
                case BlendingEquation.Inherit:
                case BlendingEquation.Add:
                    return BlendEquationMode.FuncAdd;

                case BlendingEquation.Min:
                    return BlendEquationMode.Min;

                case BlendingEquation.Max:
                    return BlendEquationMode.Max;

                case BlendingEquation.Subtract:
                    return BlendEquationMode.FuncSubtract;

                case BlendingEquation.ReverseSubtract:
                    return BlendEquationMode.FuncReverseSubtract;
            }
        }

        #endregion

        /// <summary>
        /// Copy all properties that are marked as inherited from a parent <see cref="BlendingParameters"/> object.
        /// </summary>
        /// <param name="parent">The parent <see cref="BlendingParameters"/> from which to copy inherited properties.</param>
        public void CopyFromParent(BlendingParameters parent)
        {
            if (Source == BlendingType.Inherit)
                Source = parent.Source;

            if (Destination == BlendingType.Inherit)
                Destination = parent.Destination;

            if (SourceAlpha == BlendingType.Inherit)
                SourceAlpha = parent.SourceAlpha;

            if (DestinationAlpha == BlendingType.Inherit)
                DestinationAlpha = parent.DestinationAlpha;

            if (RGBEquation == BlendingEquation.Inherit)
                RGBEquation = parent.RGBEquation;

            if (AlphaEquation == BlendingEquation.Inherit)
                AlphaEquation = parent.AlphaEquation;
        }

        /// <summary>
        /// Any properties marked as inherited will have their blending mode changed to the default type. This can occur when a root element is set to inherited.
        /// </summary>
        public void ApplyDefaultToInherited()
        {
            if (Source == BlendingType.Inherit)
                Source = BlendingType.SrcAlpha;

            if (Destination == BlendingType.Inherit)
                Destination = BlendingType.OneMinusSrcAlpha;

            if (SourceAlpha == BlendingType.Inherit)
                SourceAlpha = BlendingType.One;

            if (DestinationAlpha == BlendingType.Inherit)
                DestinationAlpha = BlendingType.One;

            if (RGBEquation == BlendingEquation.Inherit)
                RGBEquation = BlendingEquation.Add;

            if (AlphaEquation == BlendingEquation.Inherit)
                AlphaEquation = BlendingEquation.Add;
        }

        public bool Equals(BlendingParameters other) =>
            other.Source == Source
            && other.Destination == Destination
            && other.SourceAlpha == SourceAlpha
            && other.DestinationAlpha == DestinationAlpha
            && other.RGBEquation == RGBEquation
            && other.AlphaEquation == AlphaEquation;

        public bool IsDisabled =>
            Source == BlendingType.One
            && Destination == BlendingType.Zero
            && SourceAlpha == BlendingType.One
            && DestinationAlpha == BlendingType.Zero
            && RGBEquation == BlendingEquation.Add
            && AlphaEquation == BlendingEquation.Add;

        public override string ToString() => $"BlendingParameter: Factor: {Source}/{Destination}/{SourceAlpha}/{DestinationAlpha} RGBEquation: {RGBEquation} AlphaEquation: {AlphaEquation}";
    }
}
