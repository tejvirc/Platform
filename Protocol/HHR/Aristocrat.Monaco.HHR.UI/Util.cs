namespace Aristocrat.Monaco.Hhr.UI
{
    using System;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using System.IO;
    using System.Reflection;
    using System.Collections.Generic;

    public static class Util
    {
        /// <summary>
        /// Extension to shuffle the items of a list
        /// </summary>
        /// <typeparam name="T">Type of the list</typeparam>
        /// <param name="list">The list</param>
        public static void Shuffle<T>(this List<T> list)
        {
            if (list == null)
            {
                return;
            }

            var rng = new Random(Guid.NewGuid().GetHashCode());
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Extension to shuffle and int array
        /// </summary>
        /// <param name="array">The array</param>
        public static void Shuffle(this int[] array)
        {
            var r = new Random(Guid.NewGuid().GetHashCode());

            var n = array.Length;
            while (n > 1)
            {
                var k = r.Next(n--);
                var temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static string ResourcesDirName = "Resources";
        public static string HorseNumberImagesDirName = "Tiles";
        public static string HorsePickImagesDirName = "Picks";
        public static string HorseNumberImageFileExt = ".png";

        public static string HorseNumberImageLargeResourcePrefix = "HHR_HorseNumber_TileA_";
        public static string HorseNumberImageLargeFilePrefix = "TileA_";

        public static string HorseNumberImageMediumResourcePrefix = "HHR_HorseNumber_TileB_";
        public static string HorseNumberImageMediumFilePrefix = "TileB_";

        public static string HorseNumberImageSmallResourcePrefix = "HHR_HorseNumber_TileC_";
        public static string HorseNumberImageSmallFilePrefix = "TileC_";

        public static string HorseNumberImageLargeHighlightBorderResource = "HHR_HorseNumberHighlight_TileA";
        public static string HorseNumberImageLargeHighlightBorderFileName = "TileA_HiLite";

        public static string HorseNumberImageLargeDimmerResource = "HHR_HorseNumberDimmer_TileA";
        public static string HorseNumberImageLargeDimmerFileName = "TileA_Dimmer";

        public static string HorseNumberImageMediumHighlightBorderResource = "HHR_HorseNumberHighlight_TileB";
        public static string HorseNumberImageMediumHighlightBorderFileName = "TileB_HiLite";

        public static string HorseNumberImageMediumDimmerResource = "HHR_HorseNumberDimmer_TileB";
        public static string HorseNumberImageMediumDimmerFileName = "TileB_Dimmer";

        public static string HorseNumberImageSmallHighlightBorderResource = "HHR_HorseNumberHighlight_TileC";
        public static string HorseNumberImageSmallHighlightBorderFileName = "TileC_HiLite";

        public static string HorseNumberImageSmallDimmerResource = "HHR_HorseNumberDimmer_TileC";
        public static string HorseNumberImageSmallDimmerFileName = "TileC_Dimmer";

        public static string HorsePickImageResourcePrefix = "HHR_HorsePick_";
        public static string HorsePickImageFilePrefix = "Pick";

        public static string WinningOddNumberImageResourcePrefix = "HHR_StatNumber_Tile_";
        public static string WinningOddImageFilePrefix = "TileStats_";

        /// <summary>
        /// Returns a resource key for a Winning Odd number image
        /// </summary>
        /// <param name="winningOddNumber">The horse number</param>
        /// <returns>Resource key string</returns>

        public static string WinningOddNumberResource(int winningOddNumber) =>
            $"{WinningOddNumberImageResourcePrefix}{winningOddNumber.ToString("D2")}";

        /// <summary>
        /// Returns a resource key for a large horse number image
        /// </summary>
        /// <param name="horseNumber">The horse number</param>
        /// <returns>Resource key string</returns>
        public static string LargeHorseNumberResource(int horseNumber) =>
            HorseNumberResource(HorseNumberImageLargeResourcePrefix, horseNumber);

        /// <summary>
        /// Returns a resource key for winning odd number
        /// </summary>
        /// <param name="winningOddNumber">The horse number</param>
        /// <returns>Resource key string</returns>

        private static string WinningOddNumberFileName(int winningOddNumber) => $"{WinningOddImageFilePrefix}{winningOddNumber.ToString("D2")}{HorseNumberImageFileExt}";

        /// <summary>
        /// Returns a resource key for a medium horse number image
        /// </summary>
        /// <param name="horseNumber">The horse number</param>
        /// <returns>Resource key string</returns>
        public static string MediumHorseNumberResource(int horseNumber) =>
            HorseNumberResource(HorseNumberImageMediumResourcePrefix, horseNumber);

        /// <summary>
        /// Returns a resource key for a small horse number image
        /// </summary>
        /// <param name="horseNumber">The horse number</param>
        /// <returns>Resource key string</returns>
        public static string SmallHorseNumberResource(int horseNumber) =>
            HorseNumberResource(HorseNumberImageSmallResourcePrefix, horseNumber);

        private static string HorseNumberResource(string prefix, int horseNumber) => $"{prefix}{HorseNumberResourceToString(horseNumber)}";

        private static string HorseNumberImageFileName(string prefix, int horseNumber) => $"{prefix}{HorseNumberResourceToString(horseNumber)}{HorseNumberImageFileExt}";

        private static string HorseNumberResourceToString(int horseNumber) => horseNumber.ToString("D2");

        /// <summary>
        /// Returns a resource given a key
        /// </summary>
        /// <param name="key">The resource key</param>
        /// <returns>The resource</returns>
        public static object GetResource(string key) => Application.Current.Resources[key];

        /// <summary>
        /// Returns a resource key for a horse pick position
        /// </summary>
        /// <param name="position">The position</param>
        /// <returns>Resource key string</returns>
        public static string HorsePickResource(int position) => $"{HorsePickImageResourcePrefix}{position.ToString()}";

        private static string HorsePickFileName(int position) => $"{HorsePickImageFilePrefix}{position.ToString()}{HorseNumberImageFileExt}";


        /// <summary>
        /// Loads images into the application resources dictionary
        /// </summary>
        public static void LoadImageResources()
        {
            // If one of these resources are loaded all of them are
            if (Application.Current?.Resources[LargeHorseNumberResource(1)] != null)
            {
                return;
            }

            var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) ?? "";

            var tilePath = Path.Combine(new[] { basePath, ResourcesDirName, HorseNumberImagesDirName });

            if (Application.Current != null)
            {
                // Load all 3 sizes of horse number images into resources
                for (var horseNumber = 1; horseNumber <= HhrUiConstants.MaxNumberOfHorses; horseNumber++)
                {
                    Application.Current.Resources[LargeHorseNumberResource(horseNumber)] = new BitmapImage(
                        new Uri(
                            Path.Combine(
                                tilePath,
                                HorseNumberImageFileName(HorseNumberImageLargeFilePrefix, horseNumber))));

                    Application.Current.Resources[MediumHorseNumberResource(horseNumber)] = new BitmapImage(
                        new Uri(
                            Path.Combine(
                                tilePath,
                                HorseNumberImageFileName(HorseNumberImageMediumFilePrefix, horseNumber))));

                    Application.Current.Resources[SmallHorseNumberResource(horseNumber)] = new BitmapImage(
                        new Uri(
                            Path.Combine(
                                tilePath,
                                HorseNumberImageFileName(HorseNumberImageSmallFilePrefix, horseNumber))));

                    Application.Current.Resources[WinningOddNumberResource(horseNumber)] = new BitmapImage(
                        new Uri(Path.Combine(tilePath, WinningOddNumberFileName(horseNumber))));
                }

                // Load the 3 tile highlighting borders into resources
                Application.Current.Resources[HorseNumberImageLargeHighlightBorderResource] = new BitmapImage(
                    new Uri(
                        Path.Combine(
                            tilePath,
                            $"{HorseNumberImageLargeHighlightBorderFileName}{HorseNumberImageFileExt}")));
                Application.Current.Resources[HorseNumberImageMediumHighlightBorderResource] = new BitmapImage(
                    new Uri(
                        Path.Combine(
                            tilePath,
                            $"{HorseNumberImageMediumHighlightBorderFileName}{HorseNumberImageFileExt}")));
                Application.Current.Resources[HorseNumberImageSmallHighlightBorderResource] = new BitmapImage(
                    new Uri(
                        Path.Combine(
                            tilePath,
                            $"{HorseNumberImageSmallHighlightBorderFileName}{HorseNumberImageFileExt}")));

                // Load the 3 tile dimmers into resources
                Application.Current.Resources[HorseNumberImageLargeDimmerResource] = new BitmapImage(
                    new Uri(
                        Path.Combine(
                            tilePath,
                            $"{HorseNumberImageLargeDimmerFileName}{HorseNumberImageFileExt}")));
                Application.Current.Resources[HorseNumberImageMediumDimmerResource] = new BitmapImage(
                    new Uri(
                        Path.Combine(
                            tilePath,
                            $"{HorseNumberImageMediumDimmerFileName}{HorseNumberImageFileExt}")));
                Application.Current.Resources[HorseNumberImageSmallDimmerResource] = new BitmapImage(
                    new Uri(
                        Path.Combine(
                            tilePath,
                            $"{HorseNumberImageSmallDimmerFileName}{HorseNumberImageFileExt}")));

                // Load the position pick images(there will always be exactly 8)
                var pickPath = Path.Combine(new[] { basePath, ResourcesDirName, HorsePickImagesDirName });

                for (var i = 1; i <= HhrUiConstants.NumberOfHorsePickPositions; i++)
                {
                    Application.Current.Resources[HorsePickResource(i)] = new BitmapImage(
                        new Uri(Path.Combine(pickPath, HorsePickFileName(i))));
                }
            }

        }

        /// <summary>
        /// Loads a bitmap image given a path
        /// </summary>
        /// <param name="resourcePath">The path</param>
        /// <returns>A bitmap image</returns>
        public static BitmapImage GetBitMapImage(string resourcePath)
        {
            var outPutDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase) ?? "";
            var imagePath = Path.Combine(outPutDirectory, resourcePath);
            var imageLocalPath = new Uri(imagePath).LocalPath;

            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(imageLocalPath);
            image.EndInit();

            return image;
        }

    }
}
