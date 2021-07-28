﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using ManagedBass;
using osu.Framework.Audio.Mixing;
using osu.Framework.Audio.Mixing.Bass;
using osu.Framework.Audio.Track;

namespace osu.Framework.Audio.Sample
{
    internal sealed class SampleChannelBass : SampleChannel, IBassAudioChannel
    {
        private readonly SampleBass sample;
        private volatile int channel;

        /// <summary>
        /// Whether the channel is currently playing.
        /// </summary>
        /// <remarks>
        /// This is set to <c>true</c> immediately upon <see cref="Play"/>, but the channel may not be audibly playing yet.
        /// </remarks>
        public override bool Playing => playing || enqueuedPlaybackStart;

        private volatile bool playing;

        /// <summary>
        /// <c>true</c> if the user last called <see cref="Play"/>.
        /// <c>false</c> if the user last called <see cref="Stop"/>.
        /// </summary>
        private volatile bool userRequestedPlay;

        /// <summary>
        /// Whether the playback start has been enqueued.
        /// </summary>
        private volatile bool enqueuedPlaybackStart;

        private readonly BassRelativeFrequencyHandler relativeFrequencyHandler;
        private BassAmplitudeProcessor? bassAmplitudeProcessor;

        /// <summary>
        /// Creates a new <see cref="SampleChannelBass"/>.
        /// </summary>
        /// <param name="sample">The <see cref="SampleBass"/> to create the channel from.</param>
        public SampleChannelBass(SampleBass sample)
        {
            this.sample = sample;

            relativeFrequencyHandler = new BassRelativeFrequencyHandler
            {
                FrequencyChangedToZero = stopChannel,
                FrequencyChangedFromZero = () =>
                {
                    // Only unpause if the channel has been played by the user.
                    if (userRequestedPlay)
                        playChannel();
                },
            };

            ensureChannel();
        }

        public override void Play()
        {
            userRequestedPlay = true;

            // Pin Playing and IsAlive to true so that the channel isn't killed by the next update. This is only reset after playback is started.
            enqueuedPlaybackStart = true;

            // Bring this channel alive, allowing it to receive updates.
            base.Play();

            playChannel();
        }

        internal override void OnStateChanged()
        {
            base.OnStateChanged();

            if (!hasChannel)
                return;

            Bass.ChannelSetAttribute(channel, ChannelAttribute.Volume, AggregateVolume.Value);
            Bass.ChannelSetAttribute(channel, ChannelAttribute.Pan, AggregateBalance.Value);
            relativeFrequencyHandler.SetFrequency(AggregateFrequency.Value);
        }

        public override bool Looping
        {
            get => base.Looping;
            set
            {
                base.Looping = value;
                setLoopFlag(Looping);
            }
        }

        protected override void UpdateState()
        {
            if (hasChannel)
            {
                switch (channelInterface.ChannelIsActive(this))
                {
                    case PlaybackState.Playing:
                    // Stalled counts as playing, as playback will continue once more data has streamed in.
                    case PlaybackState.Stalled:
                    // The channel is in a "paused" state via zero-frequency. It should be marked as playing even if it's in a paused state internally.
                    case PlaybackState.Paused when userRequestedPlay:
                        playing = true;
                        break;

                    default:
                        playing = false;
                        break;
                }
            }
            else
            {
                // Channel doesn't exist - a rare case occurring as a result of device updates.
                playing = false;
            }

            base.UpdateState();

            bassAmplitudeProcessor?.Update();
        }

        public override void Stop()
        {
            userRequestedPlay = false;

            base.Stop();

            stopChannel();
        }

        public override ChannelAmplitudes CurrentAmplitudes => (bassAmplitudeProcessor ??= new BassAmplitudeProcessor(this)).CurrentAmplitudes;

        private bool hasChannel => channel != 0;

        private void playChannel() => EnqueueAction(() =>
        {
            try
            {
                // Channel may have been freed via UpdateDevice().
                ensureChannel();

                if (!hasChannel)
                    return;

                // Ensure state is correct before starting.
                InvalidateState();

                // Bass will restart the sample if it has reached its end. This behavior isn't desirable so block locally.
                // Unlike TrackBass, sample channels can't have sync callbacks attached, so the stopped state is used instead
                // to indicate the natural stoppage of a sample as a result of having reaching the end.
                if (Played && channelInterface.ChannelIsActive(this) == PlaybackState.Stopped)
                    return;

                playing = true;

                if (!relativeFrequencyHandler.IsFrequencyZero)
                    channelInterface.ChannelPlay(this);
            }
            finally
            {
                enqueuedPlaybackStart = false;
            }
        });

        private void stopChannel() => EnqueueAction(() =>
        {
            if (hasChannel)
                channelInterface.ChannelPause(this);
        });

        private void setLoopFlag(bool value) => EnqueueAction(() =>
        {
            if (hasChannel)
                Bass.ChannelFlags(channel, value ? BassFlags.Loop : BassFlags.Default, BassFlags.Loop);
        });

        private void ensureChannel() => EnqueueAction(() =>
        {
            if (hasChannel)
                return;

            channel = Bass.SampleGetChannel(sample.SampleId, BassFlags.SampleChannelStream | BassFlags.Decode);

            if (!hasChannel)
                return;

            Bass.ChannelSetAttribute(channel, ChannelAttribute.NoRamp, 1);
            setLoopFlag(Looping);

            relativeFrequencyHandler.SetChannel(channel);
        });

        #region Mixing

        private IBassAudioChannelInterface channelInterface = new PassThroughBassAudioChannelInterface();

        protected override AudioMixer? Mixer
        {
            get => base.Mixer;
            set
            {
                base.Mixer = value;
                channelInterface = value as IBassAudioChannelInterface ?? new PassThroughBassAudioChannelInterface();
            }
        }

        bool IBassAudioChannel.IsActive => IsAlive;

        int IBassAudioChannel.Handle => channel;

        bool IBassAudioChannel.MixerChannelPaused { get; set; } = true;

        IBassAudioChannelInterface IBassAudioChannel.Interface => channelInterface;

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (hasChannel)
            {
                channelInterface.StreamFree(this);
                channel = 0;
            }

            playing = false;

            base.Dispose(disposing);
        }
    }
}
