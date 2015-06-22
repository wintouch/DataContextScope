/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Data;

namespace Wintouch.Data.Linq
{
    /// <summary>
    /// Convenience methods to create a new ambient DataContextScope. This is the prefered method
    /// to create a DataContextScope.
    /// </summary>
    public interface IDataContextScopeFactory
    {
        /// <summary>
        /// Creates a new DataContextScope.
        /// </summary>
        IDataContextScope Create();

        /// <summary>
        /// Creates a new DataContextScope for read-only queries.
        /// </summary>
        IDataContextReadOnlyScope CreateReadOnly();

        /// <summary>
        /// Forces the creation of a new ambient DataContextScope (i.e. does not
        /// join the ambient scope if there is one) and wraps all DataContext instances
        /// created within that scope in an explicit database transaction with 
        /// the provided isolation level. 
        /// 
        /// WARNING: the database transaction will remain open for the whole 
        /// duration of the scope! So keep the scope as short-lived as possible.
        /// Don't make any remote API calls or perform any long running computation 
        /// within that scope.
        /// 
        /// This is an advanced feature that you should use very carefully
        /// and only if you fully understand the implications of doing this.
        /// </summary>
        IDataContextScope CreateWithTransaction(IsolationLevel isolationLevel);

        /// <summary>
        /// Forces the creation of a new ambient read-only DataContextScope (i.e. does not
        /// join the ambient scope if there is one) and wraps all DataContext instances
        /// created within that scope in an explicit database transaction with 
        /// the provided isolation level. 
        /// 
        /// WARNING: the database transaction will remain open for the whole 
        /// duration of the scope! So keep the scope as short-lived as possible.
        /// Don't make any remote API calls or perform any long running computation 
        /// within that scope.
        /// 
        /// This is an advanced feature that you should use very carefully
        /// and only if you fully understand the implications of doing this.
        /// </summary>
        IDataContextReadOnlyScope CreateReadOnlyWithTransaction(IsolationLevel isolationLevel);

    }
}
