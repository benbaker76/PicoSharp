using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoSharp
{
    class DSP
    {
        private const int SAMPLE_RATE = 44100;

        private static Random m_random = null;

        static DSP()
        {
            m_random = new Random();
        }

        public static void SquareWave(uint frequency, short amplitude, short offset, int position, int destOffset, int destLength, ref short[] dest)
        {
            int periodLength = (int)Math.Floor((float)SAMPLE_RATE / frequency);
            int halfPeriod = periodLength / 2;

            for (int i = 0; i < destLength; i++)
            {
                var sampleInPeriod = position % periodLength;
                dest[destOffset + i] += (short)(offset + sampleInPeriod < halfPeriod ? -amplitude : amplitude);
                position++;
            }
        }

        public static void PulseWave(uint frequency, short amplitude, short offset, float dutyCycle, int position, int destOffset, int destLength, ref short[] dest)
        {
            int periodLength = (int)Math.Floor((float)SAMPLE_RATE / frequency);
            int dutyOnLength = (int)Math.Floor(dutyCycle * periodLength);

            for (int i = 0; i < destLength; i++)
            {
                var sampleInPeriod = position % periodLength;
                dest[destOffset + i] += (short)(offset + sampleInPeriod < dutyOnLength ? amplitude : -amplitude);
                position++;
            }
        }

        public static void TriangleWave(uint frequency, short amplitude, short offset, int position, int destOffset, int destLength, ref short[] dest)
        {
            int periodLength = (int)Math.Floor((float)SAMPLE_RATE / frequency);

            for (int i = 0; i < destLength; i++)
            {
                int repetitions = position / periodLength;
                float p = position / (float)periodLength - repetitions;

                if (p < 0.50f)
                    dest[destOffset + i] += (short)Math.Floor(offset + amplitude - amplitude * 2 * (p / 0.5f));
                else
                    dest[destOffset + i] += (short)Math.Floor(offset - amplitude + amplitude * 2 * ((p - 0.5f) / 0.5f));

                position++;
            }
        }

        public static void SawtoothWave(uint frequency, short amplitude, short offset, int position, int destOffset, int destLength, ref short[] dest)
        {
            int periodLength = (int)Math.Floor((float)SAMPLE_RATE / frequency);

            for (int i = 0; i < destLength; i++)
            {
                int repetitions = position / periodLength;
                float p = position / (float)periodLength - repetitions;
                dest[destOffset + i] += (short)Math.Floor(offset - amplitude + amplitude * 2 * p);
                position++;
            }
        }

        public static void TiltedSawtoothWave(uint frequency, short amplitude, short offset, float dutyCycle, int position, int destOffset, int destLength, ref short[] dest)
        {
            int periodLength = (int)Math.Floor((float)SAMPLE_RATE / frequency);

            for (int i = 0; i < destLength; i++)
            {
                int repetitions = position / periodLength;
                float p = position / (float)periodLength - repetitions;

                if (p < dutyCycle)
                    dest[destOffset + i] += (short)Math.Floor(offset - amplitude + amplitude * 2 * (p / dutyCycle));
                else
                {
                    float op = (p - dutyCycle) / (1.0f - dutyCycle);
                    dest[destOffset + i] += (short)Math.Floor(offset + amplitude - amplitude * 2 * op);
                }

                position++;
            }
        }

        public static void OrganWave(uint frequency, short amplitude, short offset, float coefficient, int position, int destOffset, int destLength, ref short[] dest)
        {
            int periodLength = (int)Math.Floor((float)SAMPLE_RATE / frequency);

            for (int i = 0; i < destLength; i++)
            {
                int repetitions = position / periodLength;
                float p = position / (float)periodLength - repetitions;

                if (p < 0.25f) // drop +a -a
                    dest[destOffset + i] += (short)Math.Floor(offset + amplitude - amplitude * 2 * (p / 0.25f));
                else if (p < 0.50f) // raise -a +c
                    dest[destOffset + i] += (short)Math.Floor(offset - amplitude + amplitude * (1.0f + coefficient) * (p - 0.25) / 0.25);
                else if (p < 0.75) // drop +c -a
                    dest[destOffset + i] += (short)Math.Floor(offset + amplitude * coefficient - amplitude * (1.0f + coefficient) * (p - 0.50) / 0.25f);
                else
                    dest[destOffset + i] += (short)Math.Floor(offset - amplitude + amplitude * 2 * (p - 0.75f) / 0.25f);

                position++;
            }
        }

        public static void Noise(uint frequency, short amplitude, int position, int destOffset, int destLength, ref short[] dest)
        {
            int periodLength = (int)Math.Floor((float)SAMPLE_RATE / frequency);
            int halfPeriod = periodLength / 2;

            for (int i = 0; i < destLength; i++)
            {
                var sampleInPeriod = position % periodLength;
                dest[destOffset + i] += (short)m_random.Next(-amplitude / 2, amplitude / 2);
                position++;
            }
        }

        public static void FadeIn(short amplitude, int destOffset, int destLength, ref short[] dest)
        {
            float incr = 1.0f / dest.Length;

            for (int i = 0; i < destLength; i++)
            {
                float v = dest[destOffset + i] / amplitude;
                dest[destOffset + i] = (short)Math.Floor(v * incr * i * amplitude);
            }
        }

        public static void FadeOut(short amplitude, int destOffset, int destLength, ref short[] dest)
        {
            float incr = 1.0f / dest.Length;

            for (int i = 0; i < destLength; i++)
            {
                float v = dest[destOffset + i] / amplitude;
                dest[destOffset + i] = (short)Math.Floor(v * incr * (dest.Length - i - 1) * amplitude);
            }
        }
    }
}
