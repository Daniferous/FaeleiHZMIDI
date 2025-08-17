using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using NAudio.Midi;

// This program is licensed under the Creative Commons
// Attribution-NonCommercial-ShareAlike 4.0 International License.
// To view a copy of this license, visit
// http://creativecommons.org/licenses/by-nc-sa/4.0/

// Created by Daniferous, with the use of NAudio by Mark Heath.
// See Credits in the README.md.

namespace FaeHZMIDI
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                switch (args.Length)
                {
                    case 0:
                        Console.WriteLine($"Please drag and drop (or run this program with) a MIDI file here.\nHere's a scarameow:\n/ᐠ - ˕ -マ");
                        Console.ReadLine();
                        break;
                    case 1:
                        Console.WriteLine("Here's a scarameow:\n/ᐠ - ˕ -マ");
                        AnalyzeFile(args[0]);
                        break;
                    default:
                        Console.WriteLine($"Multiple file analysis not supported.\nHere's a scarameow:\n/ᐠ - ˕ -マ");
                        Console.ReadLine();
                        break;
                }
            }
            catch (Exception dead)
            {
                Console.WriteLine($"Something went terribly wrong:\n{dead.Message}\n");
                Console.ReadLine();
            }
        }
        
        private static void AnalyzeFile(string file)
        {
            if (File.Exists(file))
            {
                string? dir = Path.GetDirectoryName(file);
                if (string.IsNullOrEmpty(dir))
                {
                    dir = Environment.CurrentDirectory;
                }
                string outfile = Path.Combine(dir,Path.GetFileNameWithoutExtension(file) + " - converted.mid");

                // Here we go...
                try
                {
                    var midi = new MidiFile(file,false);

                    var tempoList = new List<(long tick, int microsecondsPerQuart)>();
                    foreach (var a in midi.Events[0]) // Remember that all Tempos are on Track 0.
                    {
                        if (a is TempoEvent tempo)
                        {
                            tempoList.Add((tempo.AbsoluteTime, tempo.MicrosecondsPerQuarterNote));
                        }
                    }
                    tempoList = tempoList.OrderBy(tem => tem.tick).ToList(); // Probably not needed, as the tempo events are already sorted by tick, and there is only one track.

                    // List of notes pre-conversion
                    var noteListOriginal = new List<(NoteOnEvent note_on, NoteEvent note_off, int track, bool doFreq, double coefficientWrap, int deltaOctave)>();


                    /*******************************************************
                        The following are default values for parameters.
                    *******************************************************/

                    // Determining which notes to divide by a nonzero value and multiply finite times.
                    bool DoFreq = false;
                    // Wrap Frequencies (Prevents ultra-long and ultra-short notes.)
                    bool WrapFreq = false;
                    // Set Octave Delta (Multiplies/Divides note lengths)
                    int DeltaOctave = -3;
                    // Set Wrap Coefficient (Proportion of Time Interval to Wrap Frequencies with)
                    double CoefficientWrap = 0.01;

                    // Generic isNumerical Boolean
                    bool genericIsNumerical = true;
                    double genericNumber = 0;

                    // Iterate through events of each track.
                    for (int trk = 0; trk < midi.Tracks; trk++)
                    {
                        foreach (var ev in midi.Events[trk])
                        {
                            if (ev is TextEvent text) // I do not know any better method than this...
                            {
                                switch (text.Text)
                                {
                                    case "beginFreq":
                                        DoFreq = true;
                                        Console.WriteLine($"Will now convert notes starting at Track {trk} at Position {text.AbsoluteTime}.");
                                        break;
                                    case "endFreq":
                                        DoFreq = false;
                                        Console.WriteLine($"Will no longer convert notes starting at Track {trk} at Position {text.AbsoluteTime}.");
                                        break;
                                    case "enableFreqWrap":
                                        WrapFreq = true;
                                        Console.WriteLine($"Enabled Frequency Wrapping.");
                                        break;
                                    default:
                                        if ((text.Text).Length > 16)
                                        {
                                            if ((text.Text).Substring(0,15) == "coefficientWrap")
                                            {
                                                genericIsNumerical = double.TryParse((text.Text).Substring(16), out genericNumber);
                                                if (genericIsNumerical)
                                                {
                                                    if (genericNumber > 0 && genericNumber < 1)
                                                    {
                                                        CoefficientWrap = genericNumber;
                                                        Console.WriteLine($"Set the Wrapping Coefficient to {CoefficientWrap}.");
                                                    }
                                                }
                                            }
                                        }
                                        if ((text.Text).Length > 10)
                                        {
                                            if ((text.Text).Substring(0,9) == "octaveSet")
                                            {
                                                genericIsNumerical = double.TryParse((text.Text).Substring(10), out genericNumber);
                                                if (genericIsNumerical)
                                                {
                                                    if (genericNumber > -8 && genericNumber < 7)
                                                    {
                                                        DeltaOctave = (int)Math.Round(genericNumber);
                                                        Console.WriteLine($"Set the Octave Offset to {DeltaOctave}.");
                                                    }
                                                }
                                            }
                                        }
                                        break;
                                }
                            }
                            if (ev is NoteOnEvent note && note.Velocity > 0 && note.OffEvent is NoteEvent note_off) // FL Studio Users: PLEASE DO NOT enable 0 velocity notes.
                            {
                                noteListOriginal.Add((note,note_off,trk,DoFreq,CoefficientWrap,DeltaOctave)); // To do: Add DeltaOctave and CoefficientWrap to NoteListOriginal
                            }
                        }
                    }

                    // Strict Note Limit (Max limit to prevent large notecounts as a result!)
                    // Only increases as the number of repeated notes increases.
                    // Is not influenced by the MIDI's actual note count.
                    long note_limit = 16777215; long nc = 0;

                    // Dealing with notes, finally!
                    foreach (var (note_on, note_off, track, doFreq, coefficientWrap, deltaOctave) in noteListOriginal)
                    {
                        // Tempo at note start...
                        // This is ineffective if the tempo changes rapidly,
                        // So MIDIs containing BPM Jumps, Ramps, Spirals, Dodecahedrons are discouraged with this program.
                        int microsecondsPerQuart = TempoAtTick(note_on.AbsoluteTime, tempoList);
                        double BPM = 60000000.0 / microsecondsPerQuart;
                        double BPS = 1000000.0 / Math.Floor((60000000.0 / BPM) + 0.5);

                        // Tick Interval (PPQ of MIDI x Beats per Seconds)
                        double tInt = (midi.DeltaTicksPerQuarterNote * BPS);

                        // Frequency of a note (assuming a Tick Interval of exactly 1):
                        // Frequency(Note) = 440 * 2^((Note - 69)/12) * 2^(Octave Change)
                        // Octave Change is by default -3.
                        // Lower values = Lower Frequencies and Longer Divisions
                        // Higher values = Not recommended for small PPQs.
                        // Should I increase the resolution of the MIDI on demand?
                        // Anyways, step (note length):
                        double step = tInt / NoteFrequency(note_on.NoteNumber, deltaOctave);

                        if (WrapFreq)
                        {
                            if (!(coefficientWrap > 0 && coefficientWrap < 1))
                            {
                                double step_old = step;
                                step = step_old / (Math.Pow(2, Math.Ceiling(Math.Log(step_old / (0.01 * tInt), 2))));
                                Console.WriteLine($"Warning: Invalid Wrapping Coefficient.\nCoefficient must be greater than 0 but lesser than 1.\nWill use a Wrapping of Coefficient value of 0.01.");
                            }
                            else
                            {
                                double step_old = step;
                                step = step_old / (Math.Pow(2, Math.Ceiling(Math.Log(step_old / (coefficientWrap * tInt), 2))));
                            }                            
                        }

                        if (step > 1 && doFreq && (nc <= note_limit))
                        {
                            int rep = (int)Math.Ceiling((note_off.AbsoluteTime - note_on.AbsoluteTime)/step);
                            double time_start = note_on.AbsoluteTime;

                            // Making the notes to make the notes...
                            for (int note_add = 0; note_add < rep; note_add++)
                            {
                                var new_on = new NoteOnEvent((long)Math.Round(time_start), note_on.Channel, note_on.NoteNumber, note_on.Velocity, (int)Math.Ceiling(step));
                                var new_off = new NoteEvent((long)Math.Round(time_start + step), note_on.Channel, MidiCommandCode.NoteOff, note_on.NoteNumber, 0);
                                midi.Events[track].Add(new_on);
                                midi.Events[track].Add(new_off);
                                time_start += step;
                                nc++;
                            }

                            // Remove after finishing
                            midi.Events[track].Remove(note_on);
                            midi.Events[track].Remove(note_off);
                        }
                    }

                    // Sorting the list
                    for (int trk = 0; trk < midi.Tracks; trk++)
                    {
                        var scarameowList = midi.Events[trk].OrderBy(scarameow => scarameow.AbsoluteTime).ToList();
                        midi.Events[trk].Clear();
                        foreach (var kazuha in scarameowList)
                        {
                            midi.Events[trk].Add(kazuha);
                        } 
                    }

                    // Saving, probably
                    if (nc > note_limit)
                    {
                        Console.WriteLine($"Warning: Some notes were skipped due to have exceeded the notecount limit of {note_limit}.\nThe notecount is independent of the MIDI's actual note count and only counts the repeated notes.");
                        Console.ReadLine();
                    }
                    else if (nc == 0)
                    {
                        Console.WriteLine($"Warning: No notes were converted. Did you set a control boundary?\nUse \"beginFreq\" and \"endFreq\" to set a control boundary and try again.");
                        Console.ReadLine();
                    }
                    MidiFile.Export(outfile, midi.Events);
                    Console.WriteLine($"MIDI exported successfully!\n{outfile}\n");
                    Console.ReadLine();
                }
                catch (Exception dead)
                {
                    Console.WriteLine($"Something went terribly wrong:\n{dead.Message}\n");
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine($"No MIDI detected...");
                Console.ReadLine();
            }
        }

        static int TempoAtTick(long note_tick, List<(long tick, int microsecondsPerQuart)> tempoList)
        {
            int microsecondsPerQuart = 500000; // 120 BPM
            foreach (var (tick, tempo) in tempoList)
            {
                if (note_tick >= tick)
                {
                    microsecondsPerQuart = tempo;
                }
                else
                {
                    break;
                }
            }
            return microsecondsPerQuart;
        }
        
        static double NoteFrequency(int note, int octave) // By default, octave is 0.
        {
            return 440.0 * Math.Pow(2.0, ((note - 69) / 12.0)) * Math.Pow(2.0, octave);
        }
    }
}