using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Numero3.EntityFramework.Implementation
{
  public static class DataContextExtensions
  {
    public static DbTransaction BeginTransaction(this DataContext dataContext, IsolationLevel level)
    {
      DbConnection connection = dataContext.Connection;
      if (connection.State != ConnectionState.Open)
      {
        connection.Open();
      }
      DbTransaction transaction = connection.BeginTransaction(level);
      dataContext.Transaction = transaction;
      return transaction;
    }

    public static int SaveChanges(this DataContext dataContext)
    {
      ChangeSet changeSet = dataContext.GetChangeSet();
      int numberOfAffectedEntities = changeSet.Inserts.Count + changeSet.Updates.Count + changeSet.Deletes.Count;
      dataContext.SubmitChanges();
      return numberOfAffectedEntities;
    }

    public static ObjectStateManager GetObjectStateManager(this DataContext dataContext)
    {
      return new ObjectStateManager(dataContext);
    }
      
    internal static object GetNativeCommonDataServices(this DataContext context)
    {
      FieldInfo servicesField = typeof(DataContext).GetField("services", BindingFlags.Instance | BindingFlags.NonPublic);
      object services = servicesField.GetValue(context);
      return services;
    }
  }

  public class ObjectStateManager
  {
    private readonly DataContext _context;
    private readonly IDataServices _dataServices;

    internal ObjectStateManager(DataContext context)
    {
      _context = context;
      _dataServices = CommonDataServicesWrapper.CreateDataServiceServicesWrapper(context);
    }

    public bool TryGetObjectStateEntry(object entity, out ObjectStateEntry entry)
    {
      Type entityType = entity.GetType();
      var primaryKeys = DataContextHelper.GetPrimaryKeys(entityType);
      EntityKey key = new EntityKey(entityType, primaryKeys.Select(x => new KeyValuePair<Property, object>(x, x.GetValue(entity))));
      ChangeSet changeSet = _context.GetChangeSet();
      
      if (changeSet.Inserts.Any(x => x == entity))
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Added };
        return true;
      }

      if (changeSet.Updates.Any(x => x == entity))
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Modified };
        return true;
      }

      if (changeSet.Deletes.Any(x => x == entity))
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Deleted };
        return true;
      }

      MetaType metaType = _context.Mapping.GetTable(key.EntityType).RowType;
      if (_dataServices.IsCachedObject(metaType, entity))
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Unchanged };
        return true;
      }

      entry = null;
      return false;
    }

    public bool TryGetObjectStateEntry(EntityKey key, out ObjectStateEntry entry)
    {
      ChangeSet changeSet = _context.GetChangeSet();
     
      object entity = changeSet.Inserts.SingleOrDefault(x => key.Matches(x));
      if (entity != null)
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Added };
        return true;
      }
      
      entity = changeSet.Updates.SingleOrDefault(x => key.Matches(x));
      if (entity != null)
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Modified };
        return true;
      }
      
      entity = changeSet.Deletes.SingleOrDefault(x => key.Matches(x));
      if (entity != null)
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Deleted };
        return true;
      }
    
      MetaType metaType = _context.Mapping.GetTable(key.EntityType).RowType;
      entity = _dataServices.GetCachedObject(metaType, key.PrimaryKeyValues);
      if (entity != null)
      {
        entry = new ObjectStateEntry { EntityKey = key, Entity = entity, State = EntityState.Unchanged };
        return true;
      }

      entry = null;
      return false;
    }
  }

  public enum EntityState { Added, Deleted, Detached, Modified, Unchanged };

  public class ObjectStateEntry
  {
    public object Entity { get; set; }

    public EntityKey EntityKey { get; set; }

    public EntityState State { get; set; }
  }

  public class EntityKey
  {
    private readonly Type _entityType;
    private readonly List<KeyValuePair<Property, object>>_primaryKeyValuePairs;

    internal Type EntityType
    {
      get
      {
        return _entityType;
      }
    }

    internal object[] PrimaryKeyValues
    {
      get
      {
        return _primaryKeyValuePairs.Select(x => x.Value).ToArray();
      }
    }

    internal EntityKey(Type entityType, IEnumerable<KeyValuePair<Property, object>> primaryKeyValuePairs)
    {
      _entityType = entityType;
      _primaryKeyValuePairs = new List<KeyValuePair<Property, object>>(primaryKeyValuePairs);
    }

    internal bool Matches(object entity)
    {
      return entity.GetType() == _entityType && _primaryKeyValuePairs.All(pair => pair.Key.GetValue(entity).Equals(pair.Value));
    }
  }

  public interface IDataContextAdapter
  {
    DataContext DataContext { get; }
  }

  internal static class DataContextHelper
  {
    private static readonly Dictionary<Type, Property[]> primaryKeyMap = new Dictionary<Type, Property[]>();

    public static Property[] GetPrimaryKeys(Type type)
    {
      Property[] result;
      if (primaryKeyMap.TryGetValue(type, out result))
      {
        return result;
      }

      PropertyInfo[] properties = type.GetProperties();

      List<Property> primaryKeys = new List<Property>();
      foreach (PropertyInfo property in properties)
      {
        var attribs = property.GetCustomAttributes(typeof(ColumnAttribute), true);
        if (attribs.Length > 0)
        {
          ColumnAttribute column = attribs[0] as ColumnAttribute;
          if (column.IsPrimaryKey)
          {
            primaryKeys.Add(new Property(property));
          }
        }
      }

      result = primaryKeys.ToArray();
      primaryKeyMap[type] = result;
      return result;
    }
  }

  internal class Property
  {
    private static readonly Dictionary<PropertyInfo, LateBoundPropertyAccess> MethodMap = new Dictionary<PropertyInfo, LateBoundPropertyAccess>();
    private readonly PropertyInfo propertyInfo;

    public PropertyInfo PropertyInfo
    {
      get { return propertyInfo; }
    }

    public string Name
    {
      get { return propertyInfo.Name; }
    }

    public Property(PropertyInfo propertyInfo)
    {
      if (propertyInfo == null)
        throw new ArgumentNullException("propertyInfo");

      this.propertyInfo = propertyInfo;
      if (!MethodMap.ContainsKey(propertyInfo))
        MethodMap[propertyInfo] = DelegateFactory.Create(propertyInfo);
    }

    public object GetValue(object target)
    {
      return MethodMap[propertyInfo](target);
    }
  }

  public delegate object LateBoundMethod(object target, object[] arguments);
  delegate void LateBoundVoidMethod(object target, object[] arguments);

  public delegate object LateBoundPropertyAccess(object target);//, object[] arguments);

  public static class DelegateFactory
  {
    public static LateBoundMethod Create(MethodInfo method)
    {
      ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
      ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

      MethodCallExpression call = Expression.Call(
        Expression.Convert(instanceParameter, method.DeclaringType),
        method,
        CreateParameterExpressions(method, argumentsParameter));

      Expression<LateBoundMethod> lambda;
      if (method.ReturnType == typeof(void))
      {
        Expression<LateBoundVoidMethod> voidLambda = Expression.Lambda<LateBoundVoidMethod>(
          Expression.Convert(call, typeof(void)),
          instanceParameter,
          argumentsParameter);

        LateBoundVoidMethod compiledVoid = voidLambda.Compile();
        //Inline the method call in a lambda of return til LateBoundMethod
        lambda = (target, arguments) => Invoke(compiledVoid, target, arguments);
      }
      else
      {
        lambda = Expression.Lambda<LateBoundMethod>(
          Expression.Convert(call, typeof(object)),
          instanceParameter,
          argumentsParameter);
      }
      return lambda.Compile();
    }

    private static object Invoke(LateBoundVoidMethod m, object target, object[] arguments)
    {
      m.Invoke(target, arguments);
      return null;
    }
    public static LateBoundPropertyAccess Create(PropertyInfo method)
    {
      ParameterExpression instanceParameter = Expression.Parameter(typeof(object), "target");
      ParameterExpression argumentsParameter = Expression.Parameter(typeof(object[]), "arguments");

      MemberExpression call = Expression.MakeMemberAccess(
        Expression.Convert(instanceParameter, method.DeclaringType),
        method);

      Expression<LateBoundPropertyAccess> lambda = Expression.Lambda<LateBoundPropertyAccess>(
        Expression.Convert(call, typeof(object)),
        instanceParameter);

      return lambda.Compile();
    }

    private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
    {
      return method.GetParameters().Select((parameter, index) =>
        Expression.Convert(
          Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray();
    }
  }

  //TODO: This interface has been expanded significantly from .NET 3.5 to 4.5 - consider exposing them all for completeness
  public interface IDataServices
  {
    // Methods
    object GetCachedObject(Expression query);
    object GetCachedObject(MetaType type, object[] keyValues);
    object GetCachedObjectLike(MetaType type, object instance);
    object GetDeferredSourceFactory(MetaDataMember member);
    object InsertLookupCachedObject(MetaType type, object instance);
    bool IsCachedObject(MetaType type, object instance);
    void OnEntityMaterialized(MetaType type, object instance);
    void ResetServices();

    // Properties
    DataContext Context { get; }
    MetaModel Model { get; }
  }

   class CommonDataServicesWrapper : IDataServices
    {
      private readonly object commonDataServices;

      private static readonly Type ServicesType = typeof(DataContext).Assembly.GetType("System.Data.Linq.CommonDataServices");
      private static readonly Dictionary<string, LateBoundMethod> Methods = new Dictionary<string, LateBoundMethod>();
      private static readonly Dictionary<string, LateBoundPropertyAccess> Properties = new Dictionary<string, LateBoundPropertyAccess>();

      static CommonDataServicesWrapper()
      {
        Methods.Add("GetCachedObject", DelegateFactory.Create(ServicesType.GetMethod("GetCachedObject")));
        Methods.Add("GetCachedObject2", DelegateFactory.Create(ServicesType.GetMethod("GetCachedObject", BindingFlags.NonPublic | BindingFlags.Instance, null, new [] { typeof(MetaType), typeof(object[])}, null)));
        Methods.Add("GetDeferredSourceFactory", DelegateFactory.Create(ServicesType.GetMethod("GetDeferredSourceFactory")));
        Methods.Add("InsertLookupCachedObject", DelegateFactory.Create(ServicesType.GetMethod("InsertLookupCachedObject")));
        Methods.Add("IsCachedObject", DelegateFactory.Create(ServicesType.GetMethod("IsCachedObject")));
        Methods.Add("OnEntityMaterialized", DelegateFactory.Create(ServicesType.GetMethod("OnEntityMaterialized")));
        Methods.Add("ResetServices", DelegateFactory.Create(ServicesType.GetMethod("ResetServices", BindingFlags.NonPublic | BindingFlags.Instance)));

        Properties.Add("Context", DelegateFactory.Create(ServicesType.GetProperty("Context")));
        Properties.Add("Model", DelegateFactory.Create(ServicesType.GetProperty("Model")));
      }

      public CommonDataServicesWrapper(object commonDataServices)
      {
        if (commonDataServices == null)
          throw new ArgumentNullException("commonDataServices");

        this.commonDataServices = commonDataServices;

        Type servicesType = commonDataServices.GetType();
        if (servicesType != ServicesType)
          throw new ArgumentException("This is a wrapper for System.Data.Linq.CommonDataServices");
      }

    internal static IDataServices CreateDataServiceServicesWrapper(DataContext context)
    {
      return new CommonDataServicesWrapper(context.GetNativeCommonDataServices());
    }

      public object GetCachedObject(Expression query)
      {
        return Methods["GetCachedObject"].Invoke(commonDataServices, new object[] { query });
      }

     public object GetCachedObject(MetaType type, object[] keyValues)
     {
       return Methods["GetCachedObject2"].Invoke(commonDataServices, new object[] { type, keyValues });
     }

     public object GetCachedObjectLike(MetaType type, object instance)
      {
        return Methods["GetCachedObjectLike"].Invoke(commonDataServices, new object[] { type, instance });
      }

      public object GetDeferredSourceFactory(MetaDataMember member)
      {
        return Methods["GetDeferredSourceFactory"].Invoke(commonDataServices, new object[] { member });
      }

      public object InsertLookupCachedObject(MetaType type, object instance)
      {
        return Methods["InsertLookupCachedObject"].Invoke(commonDataServices, new [] { type, instance });
      }

      public bool IsCachedObject(MetaType type, object instance)
      {
        return (bool)Methods["IsCachedObject"].Invoke(commonDataServices, new [] { type, instance });
      }

      public void OnEntityMaterialized(MetaType type, object instance)
      {
        Methods["OnEntityMaterialized"].Invoke(commonDataServices, new [] { type, instance });
      }

      public void ResetServices()
      {
        Methods["ResetServices"].Invoke(commonDataServices, null);
      }

      public DataContext Context
      {
        get { return (DataContext)Properties["Context"].Invoke(commonDataServices); }
      }

      public MetaModel Model
      {
        get { return (MetaModel)Properties["Model"].Invoke(commonDataServices); }
      }
     
    }
  }
  

