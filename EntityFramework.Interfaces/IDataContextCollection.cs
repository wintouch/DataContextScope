/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Data.Linq;

namespace Numero3.EntityFramework.Interfaces
{
    /// <summary>
    /// Maintains a list of lazily-created DataContext instances.
    /// </summary>
    public interface IDataContextCollection : IDisposable
    {
        /// <summary>
        /// Get or create a DataContext instance of the specified type. 
        /// </summary>
		TDataContext Get<TDataContext>() where TDataContext : DataContext;
    }
}