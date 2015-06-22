/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Collections;
using System.Data;
using System.Data.Linq;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using Wintouch.Data.Linq;

namespace Wintouch.Data.Linq
{
  public class DataContextScope : IDataContextScope
  {
    private bool _disposed;
    private readonly bool _readOnly;
    private bool _completed;
    private readonly DataContextCollection _dataContexts;

    public IDataContextCollection DataContexts { get { return _dataContexts; } }

    public DataContextScope(IDataContextFactory dataContextFactory = null) :
      this(readOnly: false, isolationLevel: null, dataContextFactory: dataContextFactory)
    { }

    public DataContextScope(bool readOnly, IDataContextFactory dataContextFactory = null)
      : this(readOnly: readOnly, isolationLevel: null, dataContextFactory: dataContextFactory)
    { }

    public DataContextScope(bool readOnly, IsolationLevel? isolationLevel, IDataContextFactory dataContextFactory = null)
    {
      
      _disposed = false;
      _completed = false;
      _readOnly = readOnly;

      _dataContexts = new DataContextCollection(readOnly, isolationLevel, dataContextFactory);
    }

    public int SaveChanges()
    {
      if (_disposed) throw new ObjectDisposedException("DataContextScope");
      if (_completed) throw new InvalidOperationException("You cannot call SaveChanges() more than once on a DataContextScope. A DataContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DataContextScope and each implement a single business transaction.");

      var c = CommitInternal();
      _completed = true;

      return c;
    }

    private int CommitInternal()
    {
      return _dataContexts.Commit();
    }

    private void RollbackInternal()
    {
      _dataContexts.Rollback();
    }

    public void Dispose()
    {
        if (_disposed) return;

        // Commit / Rollback and dispose all of our DataContext instances
        if (!_completed)
        {
            // Do our best to clean up as much as we can but don't throw here as it's too late anyway.
            try
            {
                if (_readOnly)
                {
                    // Disposing a read-only scope before having called its SaveChanges() method
                    // is the normal and expected behavior. Read-only scopes get committed automatically.
                    CommitInternal();
                }
                else
                {
                    // Disposing a read/write scope before having called its SaveChanges() method
                    // indicates that something went wrong and that all changes should be rolled-back.
                    RollbackInternal();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }

            _completed = true;
        }

        _dataContexts.Dispose();

        _disposed = true;
    }

  }

  /*
   * The idea of using an object reference as our instance identifier 
   * instead of simply using a unique string (which we could have generated
   * with Guid.NewGuid() for example) comes from the TransactionScope
   * class. As far as I can make out, a string would have worked just fine.
   * I'm guessing that this is done for optimization purposes. Creating
   * an empty class is cheaper and uses up less memory than generating
   * a unique string.
  */
  internal class InstanceIdentifier : MarshalByRefObject
  { }
}

