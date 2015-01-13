/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System.Data;
using Numero3.EntityFramework.Interfaces;

namespace Numero3.EntityFramework.Implementation
{
    public class DataContextReadOnlyScope : IDataContextReadOnlyScope
    {
        private DataContextScope _internalScope;

        public IDataContextCollection DbContexts { get { return _internalScope.DbContexts; } }

        public DataContextReadOnlyScope(IDataContextFactory dbContextFactory = null)
            : this(joiningOption: DataContextScopeOption.JoinExisting, isolationLevel: null, dbContextFactory: dbContextFactory)
        {}

        public DataContextReadOnlyScope(IsolationLevel isolationLevel, IDataContextFactory dbContextFactory = null)
            : this(joiningOption: DataContextScopeOption.ForceCreateNew, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory)
        { }

        public DataContextReadOnlyScope(DataContextScopeOption joiningOption, IsolationLevel? isolationLevel, IDataContextFactory dbContextFactory = null)
        {
            _internalScope = new DataContextScope(joiningOption: joiningOption, readOnly: true, isolationLevel: isolationLevel, dbContextFactory: dbContextFactory);
        }

        public void Dispose()
        {
            _internalScope.Dispose();
        }
    }
}