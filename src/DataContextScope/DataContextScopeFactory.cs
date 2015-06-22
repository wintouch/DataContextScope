/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Data;
using System.Data.Linq;
using Wintouch.Data.Linq;

namespace Wintouch.Data.Linq
{
  public class DataContextScopeFactory : IDataContextScopeFactory
  {
    private readonly IDataContextFactory _dataContextFactory;

    public DataContextScopeFactory()
    {
        _dataContextFactory = null;
    }

    public DataContextScopeFactory(IDataContextFactory dataContextFactory = null)
    {
      _dataContextFactory = dataContextFactory;
    }

    public DataContextScopeFactory(Func<DataContext> dataContextFactory)
    {
      if (dataContextFactory != null)
      {
        _dataContextFactory = new DataContextFactory(dataContextFactory);
      }
    }

    public IDataContextScope Create()
    {
      return new DataContextScope(
          readOnly: false,
          isolationLevel: null,
          dataContextFactory: _dataContextFactory);
    }

    public IDataContextReadOnlyScope CreateReadOnly()
    {
      return new DataContextReadOnlyScope(
          isolationLevel: null,
          dataContextFactory: _dataContextFactory);
    }

    public IDataContextScope CreateWithTransaction(IsolationLevel isolationLevel)
    {
      return new DataContextScope(
          readOnly: false,
          isolationLevel: isolationLevel,
          dataContextFactory: _dataContextFactory);
    }

    public IDataContextReadOnlyScope CreateReadOnlyWithTransaction(IsolationLevel isolationLevel)
    {
      return new DataContextReadOnlyScope(
          isolationLevel: isolationLevel,
          dataContextFactory: _dataContextFactory);
    }

  }
}