/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System.Data.Linq;
using Wintouch.Data.Linq;

namespace Wintouch.Data.Linq
{
    public class AmbientDataContextLocator : IAmbientDataContextLocator
    {
        public TDataContext Get<TDataContext>() where TDataContext : DataContext
        {
            var ambientDataContextScope = DataContextScope.GetAmbientScope();
            return ambientDataContextScope == null ? null : ambientDataContextScope.DataContexts.Get<TDataContext>();
        }
    }
}