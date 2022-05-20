////////////////////////////////////////////////////////////////////////////////////////////
// <copyright file="Rotate.cs" company="Video Gaming Technologies, Inc.">
// Copyright © 2012 Video Gaming Technologies, Inc.  All rights reserved.
// Confidential and proprietary information.
// </copyright>
////////////////////////////////////////////////////////////////////////////////////////////
namespace Utility
{
    using System;

    /// <summary>
    /// Definition of the Rotate class.
    /// </summary>
    public static class Rotate
    {
        /// <summary>
        /// Rotates the bits left and wraps around for the edge case
        /// </summary>
        /// <param name="value">The value that is to be left shifted</param>
        /// <param name="count">The number of steps to shift</param>
        /// <returns>The results of left shifting</returns>
        [CLSCompliant(false)]
        public static uint RotateLeft(uint value, int count)
        {
            checked
            {
                return (value << count) | (value >> (32 - count));
            }
        }

        /// <summary>
        /// Rotates the bits right and wraps around for the edge case
        /// </summary>
        /// <param name="value">The value that is to be right shifted</param>
        /// <param name="count">The number of steps to shift</param>
        /// <returns>The results of right shifting</returns>
        [CLSCompliant(false)]
        public static uint RotateRight(uint value, int count)
        {
            checked
            {
                return (value >> count) | (value << (32 - count));
            }
        }
    }
}
