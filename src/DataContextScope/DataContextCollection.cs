/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Runtime.ExceptionServices;
using Wintouch.Data.Linq;

namespace Wintouch.Data.Linq
{
    /// <summary>
    /// As its name suggests, DataContextCollection maintains a collection of DataContext instances.
    /// 
    /// What it does in a nutshell:
    /// - Lazily instantiates DataContext instances when its Get Of TDataContext () method is called
    /// (and optionally starts an explicit database transaction).
    /// - Keeps track of the DataContext instances it created so that it can return the existing
    /// instance when asked for a DataContext of a specific type.
    /// - Takes care of committing / rolling back changes and transactions on all the DataContext
    /// instances it created when its Commit() or Rollback() method is called.
    /// 
    /// </summary>
    public class DataContextCollection : IDataContextCollection
    {
        private readonly Dictionary<String, DataContext> _initializedDataContexts;
        private readonly Dictionary<DataContext, DbTransaction> _transactions; 
        private IsolationLevel? _isolationLevel;
        private readonly IDataContextFactory _dataContextFactory;
        private bool _disposed;
        private bool _completed;
        private readonly bool _readOnly;

        internal Dictionary<String, DataContext> InitializedDataContexts { get { return _initializedDataContexts; } }

        public DataContextCollection(bool readOnly = false, IsolationLevel? isolationLevel = null, IDataContextFactory dataContextFactory = null)
        {
            _disposed = false;
            _completed = false;

            _initializedDataContexts = new Dictionary<String, DataContext>();
            _transactions = new Dictionary<DataContext, DbTransaction>();

            _readOnly = readOnly;
            _isolationLevel = isolationLevel;
            _dataContextFactory = dataContextFactory;
        }

        public TDataContext Get<TDataContext>() where TDataContext : DataContext
        {
            if (_disposed)
                throw new ObjectDisposedException("DataContextCollection");

            var requestedType = typeof(TDataContext).ToString();

            if (!_initializedDataContexts.ContainsKey(requestedType))
            {
                // First time we've been asked for this particular DataContext type.
                // Create one, cache it and start its database transaction if needed.
                var dataContext = _dataContextFactory != null
                    ? _dataContextFactory.CreateDataContext<TDataContext>()
                    : Activator.CreateInstance<TDataContext>();

                _initializedDataContexts.Add(requestedType, dataContext);

                if (_readOnly)
                {
                  //TOOD: IS THIS CORRECT?
                  dataContext.ObjectTrackingEnabled = false;
                }

                if (_isolationLevel.HasValue)
                {
                    var tran = dataContext.BeginTransaction(_isolationLevel.Value);
                    _transactions.Add(dataContext, tran);
                }
            }

            return _initializedDataContexts[requestedType] as TDataContext;
        }

        public TDataContext Get<TDataContext>(string connectionString) where TDataContext : DataContext
        {
            if (_disposed)
                throw new ObjectDisposedException("DataContextCollection");

            var requestedType = typeof(TDataContext);
            var requestedContextKey = String.Format("{0}_{1}", requestedType, connectionString);

            if (!_initializedDataContexts.ContainsKey(requestedContextKey))
            {
                // First time we've been asked for this particular DataContext type.
                // Create one, cache it and start its database transaction if needed.
                var dataContext = _dataContextFactory != null
                    ? _dataContextFactory.CreateDataContext<TDataContext>()
                    : (TDataContext) Activator.CreateInstance(requestedType, connectionString);

                _initializedDataContexts.Add(requestedContextKey, dataContext);

                if (_readOnly)
                {
                    //TOOD: IS THIS CORRECT?
                    dataContext.ObjectTrackingEnabled = false;
                }

                if (_isolationLevel.HasValue)
                {
                    var tran = dataContext.BeginTransaction(_isolationLevel.Value);
                    _transactions.Add(dataContext, tran);
                }
            }

            return _initializedDataContexts[requestedContextKey] as TDataContext;
        }

        public int Commit()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataContextCollection");
            if (_completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DataContextCollection. All the changes in the DataContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DataContextCollection and make your changes there.");

            // Best effort. You'll note that we're not actually implementing an atomic commit 
            // here. It entirely possible that one DataContext instance will be committed successfully
            // and another will fail. Implementing an atomic commit would require us to wrap
            // all of this in a TransactionScope. The problem with TransactionScope is that 
            // the database transaction it creates may be automatically promoted to a 
            // distributed transaction if our DataContext instances happen to be using different 
            // databases. And that would require the DTC service (Distributed Transaction Coordinator)
            // to be enabled on all of our live and dev servers as well as on all of our dev workstations.
            // Otherwise the whole thing would blow up at runtime. 

            // In practice, if our services are implemented following a reasonably DDD approach,
            // a business transaction (i.e. a service method) should only modify entities in a single
            // DataContext. So we should never find ourselves in a situation where two DataContext instances
            // contain uncommitted changes here. We should therefore never be in a situation where the below
            // would result in a partial commit. 

            ExceptionDispatchInfo lastError = null;

            var c = 0;

            foreach (var dataContext in _initializedDataContexts.Values)
            {
                try
                {
                    if (!_readOnly)
                    {
                        c += dataContext.SaveChanges();
                    }

                    // If we've started an explicit database transaction, time to commit it now.
                    var tran = GetValueOrDefault(_transactions, dataContext);
                    if (tran != null)
                    {
                        tran.Commit();
                        tran.Dispose();
                    }
                }
                catch (Exception e)
                {
                    lastError = ExceptionDispatchInfo.Capture(e);
                }
            }

            _transactions.Clear();
            _completed = true;

            if (lastError != null)
                lastError.Throw(); // Re-throw while maintaining the exception's original stack track

            return c;
        }

        //public Task<int> CommitAsync()
        //{
        //    return CommitAsync(CancellationToken.None);
        //}

        //public async Task<int> CommitAsync(CancellationToken cancelToken)
        //{
        //    if (cancelToken == null)
        //        throw new ArgumentNullException("cancelToken");
        //    if (_disposed)
        //        throw new ObjectDisposedException("DataContextCollection");
        //    if (_completed)
        //        throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DataContextCollection. All the changes in the DataContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DataContextCollection and make your changes there.");

        //    // See comments in the sync version of this method for more details.

        //    ExceptionDispatchInfo lastError = null;

        //    var c = 0;

        //    foreach (var dataContext in _initializedDataContexts.Values)
        //    {
        //        try
        //        {
        //            if (!_readOnly)
        //            {
        //                c += await dataContext.SaveChangesAsync(cancelToken);
        //            }

        //            // If we've started an explicit database transaction, time to commit it now.
        //            var tran = GetValueOrDefault(_transactions, dataContext);
        //            if (tran != null)
        //            {
        //                tran.Commit();
        //                tran.Dispose();
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            lastError = ExceptionDispatchInfo.Capture(e);
        //        }
        //    }

        //    _transactions.Clear();
        //    _completed = true;

        //    if (lastError != null)
        //        lastError.Throw(); // Re-throw while maintaining the exception's original stack track

        //    return c;
        //}

        public void Rollback()
        {
            if (_disposed)
                throw new ObjectDisposedException("DataContextCollection");
            if (_completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DataContextCollection. All the changes in the DataContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DataContextCollection and make your changes there.");

            ExceptionDispatchInfo lastError = null;

            foreach (var dataContext in _initializedDataContexts.Values)
            {
                // There's no need to explicitly rollback changes in a DataContext as
                // DataContext doesn't save any changes until its SaveChanges() method is called.
                // So "rolling back" for a DataContext simply means not calling its SaveChanges()
                // method. 

                // But if we've started an explicit database transaction, then we must roll it back.
                var tran = GetValueOrDefault(_transactions, dataContext);
                if (tran != null)
                {
                    try
                    {
                        tran.Rollback();
                        tran.Dispose();
                    }
                    catch (Exception e)
                    {
                        lastError = ExceptionDispatchInfo.Capture(e);
                    }
                }
            }

            _transactions.Clear();
            _completed = true;

            if (lastError != null)
                lastError.Throw(); // Re-throw while maintaining the exception's original stack track
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            // Do our best here to dispose as much as we can even if we get errors along the way.
            // Now is not the time to throw. Correctly implemented applications will have called
            // either Commit() or Rollback() first and would have got the error there.

            if (!_completed)
            {
                try
                {
                    if (_readOnly) Commit();
                    else Rollback();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            foreach (var dataContext in _initializedDataContexts.Values)
            {
                try
                {
                    dataContext.Dispose();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            _initializedDataContexts.Clear();
            _disposed = true;
        }

        /// <summary>
        /// Returns the value associated with the specified key or the default 
        /// value for the TValue  type.
        /// </summary>
        private static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return dictionary.TryGetValue(key, out value) ? value : default(TValue);
        }
    }
}