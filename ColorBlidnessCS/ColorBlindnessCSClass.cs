using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ColorBlindnessCS
{
    public class ColorBlindnessCSClass
    {
        /*
         * Executes color blindness simulation on an image.
         * Processes the image in parallel using a specified number of threads.
         *
         * Parameters:
         * - originalImagePtr: Pointer to the original image data in memory.
         * - processedImagePtr: Pointer to the memory where processed image data will be stored.
         * - pixelCount: Total number of pixels in the image.
         * - stride: Number of bytes per row in the image, including padding.
         * - blindnessType: Type of color blindness to simulate (0: Deuteranopia, 1: Protanopia, 2: Tritanopia).
         * - threadCount: Number of threads to use for parallel processing.
         */
        public static void Execute(IntPtr originalImagePtr, IntPtr processedImagePtr, int pixelCount, int stride, int blindnessType, int threadCount)
        {
            // Divide the workload among the threads using Parallel.For.
            Parallel.For(0, threadCount, threadIndex =>
            {
                // Calculate the range of pixels to process for this thread.
                int partitionSize = pixelCount / threadCount;
                int start = threadIndex * partitionSize;
                int end = (threadIndex == threadCount - 1) ? pixelCount : start + partitionSize;

                // Process pixels using unsafe pointer operations for performance.
                unsafe
                {
                    byte* originalImage = (byte*)originalImagePtr; // Pointer to the original image data.
                    byte* processedImage = (byte*)processedImagePtr; // Pointer to the processed image data.

                    // Iterate through the pixels assigned to this thread.
                    for (int i = start; i < end; i++)
                    {
                        // Calculate the memory offset for the current pixel (3 bytes per pixel for RGB format).
                        int offset = i * 3;

                        // Read the original RGB values.
                        byte originalB = originalImage[offset];
                        byte originalG = originalImage[offset + 1];
                        byte originalR = originalImage[offset + 2];

                        // Initialize new RGB values with the original ones.
                        int newR = originalR, newG = originalG, newB = originalB;

                        // Apply the appropriate transformation based on the type of color blindness.
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

                        // Ensure the calculated values are within the valid RGB range (0-255).
                        newR = Clamp(newR, 0, 255);
                        newG = Clamp(newG, 0, 255);
                        newB = Clamp(newB, 0, 255);

                        // Write the processed RGB values back to the processed image buffer.
                        processedImage[offset] = (byte)newB;
                        processedImage[offset + 1] = (byte)newG;
                        processedImage[offset + 2] = (byte)newR;
                    }
                }
            });
        }

        /*
         * Helper method for clamping a value within a specified range.
         * Ensures that the value remains between the minimum and maximum values.
         *
         * Parameters:
         * - value: The value to clamp.
         * - min: The minimum allowed value.
         * - max: The maximum allowed value.
         *
         * Returns:
         * - The clamped value.
         */
        public static int Clamp(int value, int min, int max) =>
            value < min ? min : value > max ? max : value;
    }
}
