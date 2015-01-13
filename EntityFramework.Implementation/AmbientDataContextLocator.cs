/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System.Data.Linq;
using Numero3.EntityFramework.Interfaces;

namespace Numero3.EntityFramework.Implementation
{
    public class AmbientDataContextLocator : IAmbientDataContextLocator
    {
        public TDbContext Get<TDbContext>() where TDbContext : DataContext
        {
            var ambientDbContextScope = DataContextScope.GetAmbientScope();
            return ambientDbContextScope == null ? null : ambientDbContextScope.DbContexts.Get<TDbContext>();
        }
    }
}