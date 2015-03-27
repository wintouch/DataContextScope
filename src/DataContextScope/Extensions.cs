/* 
 * Copyright (C) 2015 Kasper Frank
 * http://www.Wintouch.com
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System.Data.Linq;

namespace Wintouch.Data.Linq
{
  public static class Extensions
  {
    public static TDataContext Get<TDataContext>(this IDataContextScope dataContextScope) where TDataContext : DataContext
    {
      return dataContextScope.DataContexts.Get<TDataContext>();
    }

    public static TDataContext Get<TDataContext>(this IDataContextReadOnlyScope dataContextScope) where TDataContext : DataContext
    {
      return dataContextScope.DataContexts.Get<TDataContext>();
    }
  }
}
