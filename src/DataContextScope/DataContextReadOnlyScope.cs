/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System.Data;
using Wintouch.Data.Linq;

namespace Wintouch.Data.Linq
{
    public class DataContextReadOnlyScope : IDataContextReadOnlyScope
    {
        private DataContextScope _internalScope;

        public IDataContextCollection DataContexts { get { return _internalScope.DataContexts; } }

        public DataContextReadOnlyScope(IDataContextFactory dataContextFactory = null)
            : this(isolationLevel: null, dataContextFactory: dataContextFactory)
        {}

        public DataContextReadOnlyScope(IsolationLevel? isolationLevel, IDataContextFactory dataContextFactory = null)
        {
            _internalScope = new DataContextScope( readOnly: true, isolationLevel: isolationLevel, dataContextFactory: dataContextFactory);
        }

        public void Dispose()
        {
            _internalScope.Dispose();
        }
    }
}