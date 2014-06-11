﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cireon.Audio;

namespace yatl
{
    /// <summary>
    /// Abstract class for all kinds of sound events
    /// </summary>
    abstract class SoundEvent
    {
        public double StartTime;

        public SoundEvent(double startTime)
        {
            this.StartTime = startTime;
        }

        public void AddOffset(double offsetTime)
        {
            this.StartTime += offsetTime;
        }

        public void MultiplyOffset(double multiplier)
        {
            this.StartTime *= multiplier;
        }

        public abstract void Execute();
    }

    class NoteOn : SoundEvent
    {
        public Instrument Instrument;
        public Sound Sound = null;
        public double Frequency;

        public NoteOn(double startTime, Instrument instrument, double frequency)
            : base(startTime)
        {
            this.Frequency = frequency;
            this.Instrument = instrument;
        }

        public override void Execute()
        {
            this.Sound = this.Instrument.CreateSound(MusicManager.Volume, this.Frequency);
            this.Sound.Play();
        }
    }

    class NoteOff : SoundEvent
    {
        NoteOn noteOn;

        public NoteOff(double startTime, NoteOn noteOn)
            : base(startTime)
        {
            this.noteOn = noteOn;
        }

        public override void Execute()
        {
            MusicManager.SustainSet.Add(this.noteOn.Sound);
        }
    }

    class LiftSustain : SoundEvent
    {
        public LiftSustain(double startTime)
            : base(startTime)
        {
        }

        public override void Execute()
        {
            foreach (var sound in MusicManager.SustainSet) {
                sound.Stop();
            }
            MusicManager.SustainSet.Clear();
        }
    }

    class StartRubato : SoundEvent
    {
        public StartRubato(double startTime)
            : base(startTime)
        {
        }

        public override void Execute()
        {
            MusicManager.Acceleration = 1.0;
            MusicManager.Speed = 0.50;
        }
    }
}