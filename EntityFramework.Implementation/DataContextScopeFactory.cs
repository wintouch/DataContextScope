/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Data;
using Numero3.EntityFramework.Interfaces;

namespace Numero3.EntityFramework.Implementation
{
    public class DataContextScopeFactory : IDataContextScopeFactory
    {
        private readonly IDataContextFactory _dbContextFactory;

        public DataContextScopeFactory(IDataContextFactory dbContextFactory = null)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IDataContextScope Create(DataContextScopeOption joiningOption = DataContextScopeOption.JoinExisting)
        {
            return new DataContextScope(
                joiningOption: joiningOption, 
                readOnly: false, 
                isolationLevel: null, 
                dbContextFactory: _dbContextFactory);
        }

        public IDataContextReadOnlyScope CreateReadOnly(DataContextScopeOption joiningOption = DataContextScopeOption.JoinExisting)
        {
            return new DataContextReadOnlyScope(
                joiningOption: joiningOption, 
                isolationLevel: null, 
                dbContextFactory: _dbContextFactory);
        }

        public IDataContextScope CreateWithTransaction(IsolationLevel isolationLevel)
        {
            return new DataContextScope(
                joiningOption: DataContextScopeOption.ForceCreateNew, 
                readOnly: false, 
                isolationLevel: isolationLevel, 
                dbContextFactory: _dbContextFactory);
        }

        public IDataContextReadOnlyScope CreateReadOnlyWithTransaction(IsolationLevel isolationLevel)
        {
            return new DataContextReadOnlyScope(
                joiningOption: DataContextScopeOption.ForceCreateNew, 
                isolationLevel: isolationLevel, 
                dbContextFactory: _dbContextFactory);
        }

        public IDisposable SuppressAmbientContext()
        {
            return new AmbientContextSuppressor();
        }
    }
}