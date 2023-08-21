namespace Aristocrat.Monaco.Hardware.Persistence
{
    using System;
    using System.Linq;

    /// <summary> A key creator. </summary>
    public static class KeyCreator
    {
        /// <summary> Key. </summary>
        /// <param name="type"> The type. </param>
        /// <returns> A string. </returns>
        public static string Key(Type type)
        {
            return type.Name;
        }

        /// <summary> key. </summary>
        /// <param name="key">  The key. </param>
        /// <param name="type"> The type. </param>
        /// <returns> A string. </returns>
        public static string Key(string key, Type type)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return key + "@" + Key(type);
        }

        /// <summary> Gets the prefix of the given key. </summary>
        /// <param name="key"> The key. </param>
        /// <returns> The key prefix. </returns>
        public static string KeyPrefix(string key)
        {
            return string.IsNullOrEmpty(key) ? null : key.Split('@').FirstOrDefault();
        }

        /// <summary> Indexed key. </summary>
        /// <param name="index"> Zero-based index of the. </param>
        /// <param name="type">  The type. </param>
        /// <returns> A string. </returns>
        public static string IndexedKey(int index, Type type)
        {
            return type.Name + $"[{index.ToString()}]";
        }

        /// <summary> Creates an indexed key from key and index. </summary>
        /// <param name="key">   The key. </param>
        /// <param name="index"> Zero-based index. </param>
        /// <param name="type">  The type. </param>
        /// <returns> An indexed key. </returns>
        public static string IndexedKey(string key, int index, Type type)
        {
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }

            return key + "@" + IndexedKey(index, type);
        }

        /// <summary> Creates a complete key for block field. </summary>
        /// <param name="blockName"> Name of the block. </param>
        /// <param name="key"> The key. </param>
        /// <returns> A string. </returns>
        public static string BlockFieldKey(string blockName, string key)
        {
            if (string.IsNullOrEmpty(blockName) ||
                string.IsNullOrEmpty(key))
            {
                return null;
            }

            return blockName + "@" + key;
        }
    }
}