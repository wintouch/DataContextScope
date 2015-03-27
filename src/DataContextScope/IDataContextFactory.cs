/* 
 * Copyright (C) 2014 Mehdi El Gueddari
 * http://mehdi.me
 *
 * This software may be modified and distributed under the terms
 * of the MIT license.  See the LICENSE file for details.
 */
using System.Data.Linq;

namespace Wintouch.Data.Linq
{
    /// <summary>
    /// Factory for DataContext-derived classes that don't expose 
    /// a default constructor.
    /// </summary>
    /// <remarks>
	/// If your DataContext-derived classes have a default constructor, 
	/// you can ignore this factory. DataContextScope will take care of
	/// instanciating your DataContext class with Activator.CreateInstance() 
	/// when needed.
	/// 
	/// If your DataContext-derived classes don't expose a default constructor
	/// however, you must impement this interface and provide it to DataContextScope
	/// so that it can create instances of your DataContexts.
	/// 
	/// A typical situation where this would be needed is in the case of your DataContext-derived 
	/// class having a dependency on some other component in your application. For example, 
	/// some data in your database may be encrypted and you might want your DataContext-derived
	/// class to automatically decrypt this data on entity materialization. It would therefore 
	/// have a mandatory dependency on an IDataDecryptor component that knows how to do that. 
	/// In that case, you'll want to implement this interface and pass it to the DataContextScope
	/// you're creating so that DataContextScope is able to create your DataContext instances correctly. 
    /// </remarks>
    public interface IDataContextFactory
    {
		TDataContext CreateDataContext<TDataContext>() where TDataContext : DataContext;
    }
}
