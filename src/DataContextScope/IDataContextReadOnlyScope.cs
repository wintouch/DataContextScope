/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;

namespace Wintouch.Data.Linq
{
    /// <summary>
    /// A read-only DataContextScope. Refer to the comments for IDataContextScope
    /// for more details.
    /// </summary>
    public interface IDataContextReadOnlyScope : IDisposable
    {
        /// <summary>
        /// The DataContext instances that this DataContextScope manages.
        /// </summary>
        IDataContextCollection DataContexts { get; }
    }
}