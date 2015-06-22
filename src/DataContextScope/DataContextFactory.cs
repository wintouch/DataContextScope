/* 
 * Copyright (C) 2015 Kasper Frank
 * http://www.geturi.com
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Data.Linq;

namespace Wintouch.Data.Linq
{
  class DataContextFactory : IDataContextFactory
  {
    private readonly Func<DataContext> _factory;

    public DataContextFactory(Func<DataContext> factory)
    {
      if (factory == null)
        throw new ArgumentNullException("factory");

      _factory = factory;
    }

    public TDataContext CreateDataContext<TDataContext>() where TDataContext : DataContext
    {
      return (TDataContext)_factory();
    }
  }
}
