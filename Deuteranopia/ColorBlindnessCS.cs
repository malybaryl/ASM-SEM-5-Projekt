using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace ColorBlindnessCS
{
    public class ColorBlindnessCSClass
    {
        public static void Execute(IntPtr originalImagePtr, IntPtr processedImagePtr, int pixelCount, int stride, int blindnessType, int threadCount)
        {
            // Podział obrazu na wątki
            Parallel.For(0, threadCount, threadIndex =>
            {
                int partitionSize = pixelCount / threadCount;
                int start = threadIndex * partitionSize;
                int end = (threadIndex == threadCount - 1) ? pixelCount : start + partitionSize;

                // Przekształcenie wskaźników na tablice bajtów
                unsafe
                {
                    byte* originalImage = (byte*)originalImagePtr;
                    byte* processedImage = (byte*)processedImagePtr;

                    // Przetwarzanie pikseli
                    for (int i = start; i < end; i++)
                    {
                        int offset = i * 3; // Pixel w formacie RGB (3 bajty na piksel)

                        byte originalB = originalImage[offset];
                        byte originalG = originalImage[offset + 1];
                        byte originalR = originalImage[offset + 2];

                        int newR = originalR, newG = originalG, newB = originalB;

                        // Zastosowanie odpowiedniej transformacji w zależności od typu ślepoty kolorystycznej
                        if (blindnessType == 0) // Deuteranopia
                        {
                            newR = (int)(originalR * 0.625 + originalG * 0.375);
                            newG = (int)(originalG * 0.7);
                            newB = (int)(originalB * 0.8);
                        }
                        else if (blindnessType == 1) // Protanopia
                        {
                            newR = (int)(originalR * 0.567 + originalG * 0.433);
                            newG = (int)(originalG * 0.558);
                            newB = (int)(originalB * 0.0);
                        }
                        else if (blindnessType == 2) // Tritanopia
                        {
                            newR = (int)(originalR * 0.95);
                            newG = (int)(originalG * 0.433);
                            newB = (int)(originalB * 0.567);
                        }

                        // Clamp values to ensure they remain in the valid RGB range
                        newR = Clamp(newR, 0, 255);
                        newG = Clamp(newG, 0, 255);
                        newB = Clamp(newB, 0, 255);

                        // Zapisz przetworzone piksele
                        processedImage[offset] = (byte)newB;
                        processedImage[offset + 1] = (byte)newG;
                        processedImage[offset + 2] = (byte)newR;
                    }
                }
            });
        }

        // Helper method for clamping values to ensure they are within the valid RGB range
        public static int Clamp(int value, int min, int max) =>
            value < min ? min : value > max ? max : value;
    }
}
