/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Data;
using Geturi.Data.Linq;

namespace Geturi.Data.Linq
{
    public class DataContextScopeFactory : IDataContextScopeFactory
    {
        private readonly IDataContextFactory _dataContextFactory;

        public DataContextScopeFactory(IDataContextFactory dataContextFactory = null)
        {
            _dataContextFactory = dataContextFactory;
        }

        public IDataContextScope Create(DataContextScopeOption joiningOption = DataContextScopeOption.JoinExisting)
        {
            return new DataContextScope(
                joiningOption: joiningOption, 
                readOnly: false, 
                isolationLevel: null, 
                dataContextFactory: _dataContextFactory);
        }

        public IDataContextReadOnlyScope CreateReadOnly(DataContextScopeOption joiningOption = DataContextScopeOption.JoinExisting)
        {
            return new DataContextReadOnlyScope(
                joiningOption: joiningOption, 
                isolationLevel: null, 
                dataContextFactory: _dataContextFactory);
        }

        public IDataContextScope CreateWithTransaction(IsolationLevel isolationLevel)
        {
            return new DataContextScope(
                joiningOption: DataContextScopeOption.ForceCreateNew, 
                readOnly: false, 
                isolationLevel: isolationLevel, 
                dataContextFactory: _dataContextFactory);
        }

        public IDataContextReadOnlyScope CreateReadOnlyWithTransaction(IsolationLevel isolationLevel)
        {
            return new DataContextReadOnlyScope(
                joiningOption: DataContextScopeOption.ForceCreateNew, 
                isolationLevel: isolationLevel, 
                dataContextFactory: _dataContextFactory);
        }

        public IDisposable SuppressAmbientContext()
        {
            return new AmbientContextSuppressor();
        }
    }
}