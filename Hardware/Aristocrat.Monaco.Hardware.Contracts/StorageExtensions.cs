namespace Aristocrat.Monaco.Hardware.Contracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Kernel;
    using Persistence;

    /// <summary>
    ///     Extension methods for the <see cref="IPersistentStorageManager" /> interface.
    /// </summary>
    public static class StorageExtensions
    {
        /// <summary>
        ///     Gets an accessor for the provided block name.  The block will be created if it doesn't exist.  See
        ///     <see cref="IPersistentStorageManager" /> for full documentation.
        /// </summary>
        /// <param name="this">The IPropertiesManager instance to act on.</param>
        /// <param name="level">The persistent storage level to store the data at.</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <param name="size">The number of elements in this block.  (Usually 1 -- one element with multiple fields.)</param>
        /// <returns>The value associated with the property name</returns>
        public static IPersistentStorageAccessor GetAccessor(
            this IPersistentStorageManager @this,
            PersistenceLevel level,
            string name,
            int size = 1)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var accessor = @this.BlockExists(name)
                ? @this.GetBlock(name)
                : @this.CreateBlock(level, name, size);

            if (accessor.Level != level)
            {
                @this.UpdatePersistenceLevel(name, level);
            }

            return accessor;
        }

        /// <summary>
        ///     Gets a list for the given block.  The block must be a byte array.
        /// </summary>
        /// <param name="this">The IPersistentStorageAccessor instance to act on.</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <returns>A list of type T</returns>
        public static IEnumerable<T> GetList<T>(this IPersistentStorageAccessor @this, string name)
        {
            return GetList<T>(@this, 0, name);
        }

        /// <summary>
        ///     Gets a list for the given block.  The block must be a byte array.
        /// </summary>
        /// <param name="this">The IPersistentStorageAccessor instance to act on.</param>
        /// <param name="index">The block index</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <returns>A list of type T</returns>
        public static IEnumerable<T> GetList<T>(this IPersistentStorageAccessor @this, int index, string name)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return StorageUtilities.GetListFromByteArray<T>((byte[])@this[index, name]);
        }

        /// <summary>
        ///     Gets a list for the given block.  The block must be a byte array.
        /// </summary>
        /// <param name="this">The IPersistentStorageAccessor instance to act on.</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <param name="list">The list to save</param>
        public static void UpdateList<T>(this IPersistentStorageTransaction @this, string name, IEnumerable<T> list)
        {
            UpdateList(@this, 0, name, list);
        }

        /// <summary>
        ///     Gets a list for the given block.  The block must be a byte array.
        /// </summary>
        /// <param name="this">The IPersistentStorageAccessor instance to act on.</param>
        /// <param name="index">The block index</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <param name="list">The list to save</param>
        public static void UpdateList<T>(
            this IPersistentStorageTransaction @this,
            int index,
            string name,
            IEnumerable<T> list)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var data = list == null ? null : StorageUtilities.ToByteArray(list);

            @this[index, name] = data;
        }

        /// <summary>
        ///     Gets a list for the given block.  The block must be a byte array.
        /// </summary>
        /// <param name="this">The IPersistentStorageAccessor instance to act on.</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <param name="list">The list to save</param>
        public static void UpdateList<T>(this IPersistentStorageAccessor @this, string name, IEnumerable<T> list)
        {
            UpdateList(@this, 0, name, list);
        }

        /// <summary>
        ///     Gets a list for the given block.  The block must be a byte array.
        /// </summary>
        /// <param name="this">The IPersistentStorageAccessor instance to act on.</param>
        /// <param name="index">The block index</param>
        /// <param name="name">A unique name by which to reference the block.</param>
        /// <param name="list">The list to save</param>
        public static void UpdateList<T>(
            this IPersistentStorageAccessor @this,
            int index,
            string name,
            IEnumerable<T> list)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            using (var transaction = @this.StartTransaction())
            {
                transaction[index, name] = StorageUtilities.ToByteArray(list);
                transaction.Commit();
            }
        }

        /// <summary>An IStorageAccessor{TBlock} extension method that modifies a block.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ServiceException">Thrown when a Service error condition occurs.</exception>
        /// <typeparam name="TBlock">Type of the block.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="updater">The updater.</param>
        /// <param name="blockIndex">(Optional) Zero-based index of the block.</param>
        /// <param name="level">(Optional) The persistent storage level to store the data at.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool ModifyBlock<TBlock>(
            this IStorageAccessor<TBlock> @this,
            string blockName,
            Func<IPersistentStorageTransaction, int, bool> updater,
            int blockIndex = 0,
            PersistenceLevel level = PersistenceLevel.Transient)
        {
            if (!@this.GetOrAddBlock(blockName, out _, out var accessor, blockIndex, level))
            {
                return false;
            }

            using (var transaction = accessor.StartTransaction())
            {
                var result = updater(transaction, blockIndex);
                transaction.Commit();
                return result;
            }
        }

        /// <summary>An IStorageAccessor{TBlock} extension method that gets or adds a block.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ServiceException">Thrown when a Service error condition occurs.</exception>
        /// <typeparam name="TBlock">Type of the block.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="block">[out] The block.</param>
        /// <param name="blockIndex">(Optional) Zero-based index of the block.</param>
        /// <param name="level">(Optional) The persistent storage level to store the data at.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool GetOrAddBlock<TBlock>(
            this IStorageAccessor<TBlock> @this,
            string blockName,
            out TBlock block,
            int blockIndex = 0,
            PersistenceLevel level = PersistenceLevel.Transient)
        {
            return @this.GetOrAddBlock(blockName, out block, out _, blockIndex, level);
        }

        /// <summary>An IStorageAccessor{TBlock} extension method that gets or adds a block.</summary>
        /// <exception cref="ArgumentNullException">Thrown when one or more required arguments are null.</exception>
        /// <exception cref="ServiceException">Thrown when a Service error condition occurs.</exception>
        /// <typeparam name="TBlock">Type of the block.</typeparam>
        /// <param name="this">The @this to act on.</param>
        /// <param name="blockName">Name of the block.</param>
        /// <param name="block">[out] The block.</param>
        /// <param name="accessor">[out] The accessor.</param>
        /// <param name="blockIndex">(Optional) Zero-based index of the block.</param>
        /// <param name="level">(Optional) The persistent storage level to store the data at.</param>
        /// <returns>True if it succeeds, false if it fails.</returns>
        public static bool GetOrAddBlock<TBlock>(
            this IStorageAccessor<TBlock> @this,
            string blockName,
            out TBlock block,
            out IPersistentStorageAccessor accessor,
            int blockIndex = 0,
            PersistenceLevel level = PersistenceLevel.Transient)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var manager = ServiceManager.GetInstance().GetService<IPersistentStorageManager>();
            if (manager.BlockExists(blockName))
            {
                accessor = manager.GetBlock(blockName);
                if (accessor.Count > blockIndex)
                {
                    return @this.TryGetBlock(accessor, blockIndex, out block);
                }

                manager.ResizeBlock(blockName, blockIndex + 1);
            }
            else
            {
                accessor = manager.CreateBlock(level, blockName, blockIndex + 1);
            }

            return @this.TryAddBlock(accessor, blockIndex, out block);
        }

        /// <summary>
        ///     Gets the entity from persistent storage
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>The entity from persistent storage</returns>
        public static T GetEntity<T>(this IPersistentStorageManager @this)
            where T : class, new()
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var type = typeof(T);

            // TODO: Move this implementation and cache the reflected values

            if (!(Attribute.GetCustomAttribute(type, typeof(EntityAttribute)) is EntityAttribute entity))
            {
                return default(T);
            }

            var properties = type.GetProperties();

            if (!@this.BlockExists(type.FullName))
            {
                var format = new BlockFormat { Version = entity.Version };

                foreach (var property in properties)
                {
                    var field = property.GetCustomAttribute<FieldAttribute>();
                    if (field == null)
                    {
                        continue;
                    }

                    var storageType = field.StorageType != FieldType.Unused
                        ? field.StorageType
                        : FieldConverters.FieldTypeMap.FirstOrDefault(t => t.Value == property.PropertyType).Key;
                    
                    if (storageType != FieldType.Unused)
                    {
                        format.AddFieldDescription(
                            field.Size > 0
                                ? new FieldDescription(storageType, field.Size, 0, property.Name)
                                : new FieldDescription(storageType, 0, property.Name));
                    }
                }

                @this.CreateDynamicBlock(entity.Level, type.FullName, entity.Size, format);

                return UpdateEntity(@this, new T());
            }

            var accessor = @this.GetBlock(type.FullName);

            var result = new T();

            foreach (var property in properties)
            {
                var field = property.GetCustomAttribute<FieldAttribute>();
                if (field == null)
                {
                    continue;
                }

                property.SetValue(result, accessor[property.Name]);
            }

            return result;
        }

        /// <summary>
        ///     Gets the entity from persistent storage
        /// </summary>
        /// <typeparam name="T">The entity type</typeparam>
        /// <returns>The entity from persistent storage</returns>
        public static T UpdateEntity<T>(this IPersistentStorageManager @this, T entity)
            where T : class
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var type = typeof(T);

            // TODO: Move this implementation and cache the reflected values

            if (!(Attribute.GetCustomAttribute(type, typeof(EntityAttribute)) is EntityAttribute) || !@this.BlockExists(type.FullName))
            {
                return entity;
            }

            var accessor = @this.GetBlock(type.FullName);

            using (var transaction = accessor.StartTransaction())
            {
                foreach (var property in type.GetProperties())
                {
                    var field = property.GetCustomAttribute<FieldAttribute>();
                    if (field == null)
                    {
                        continue;
                    }

                    accessor[property.Name] = property.GetValue(entity);
                }

                transaction.Commit();
            }

            return entity;
        }
    }
}
