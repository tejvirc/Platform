using System;
using System.Collections.Generic;

namespace Aristocrat.Monaco.UI.Common.Services
{
    /// <summary>
    /// ICache.  Cache for Monaco that allows for timed or event driven expiration.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Add with full parameter support.  Specify events to invalidate the cache and Cache timeout
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="invalidateEvents"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool Add<T>(string key, T value, List<Type> invalidateEvents, TimeSpan timeout) where T : class;

        /// <summary>
        /// Add with timeout parameter but no invalidate events
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        bool Add<T>(string key, T value, TimeSpan timeout) where T : class;

        /// <summary>
        /// Add with invalidate events.  Uses default timeout of 30 minutes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="invalidateEvents"></param>
        /// <returns></returns>
        bool Add<T>(string key, T value, List<Type> invalidateEvents) where T : class;

        /// <summary>
        /// Add with no invalidate events.  Uses default timeout of 30 minutes
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool Add<T>(string key, T value) where T : class;

        /// <summary>
        /// AddOrGetExisting.  Will return the existing value or add the passed in value to the cache.  
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        T AddOrGetExisting<T>(string key, T value) where T : class;

        /// <summary>
        /// Get.  Gets a value from the cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key) where T : class;

        /// <summary>
        /// Remove.  Removes a value from the cache.
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);
    }
}
