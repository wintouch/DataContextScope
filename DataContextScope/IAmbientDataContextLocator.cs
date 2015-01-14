/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System.Data.Linq;

namespace Numero3.EntityFramework.Interfaces
{
    /// <summary>
    /// Convenience methods to retrieve ambient DataContext instances. 
    /// </summary>
    public interface IAmbientDataContextLocator
    {
        /// <summary>
        /// If called within the scope of a DataContextScope, gets or creates 
        /// the ambient DataContext instance for the provided DataContext type. 
        /// 
        /// Otherwise returns null. 
        /// </summary>
        TDataContext Get<TDataContext>() where TDataContext : DataContext;
    }
}
