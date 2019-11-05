using InterLinq;
using InterLinq.Communication;
using InterLinq.Expressions;
using System;
using System.Linq;

namespace Sample.Client
{
    public class QueryableProvider
    {
        private IQueryRemoteHandler _queryRemoteHandler;
        private InterLinqContext _interLinqContext;
        
        public QueryableProvider(IQueryRemoteHandler queryHandler)
        {
            this._queryRemoteHandler = queryHandler;
            this._interLinqContext = new InterLinqContext(new ClientQueryHandler(queryHandler));
        }

        public IQueryable Query(Type t)
        {
            return _interLinqContext.ExecuteMethod(t, null, null, null);
        }

        public IQueryable Query(Type t, params object[] parameters)
        {
            return _interLinqContext.ExecuteMethod(t, null, null, parameters);
        }

        public IQueryable<T> Query<T>() where T : class
        {
            return (IQueryable<T>)_interLinqContext.ExecuteMethod(typeof(T), null, null, null);
        }

        public IQueryable<T> Query<T>(params object[] parameters) where T : class
        {
            return (IQueryable<T>)_interLinqContext.ExecuteMethod(typeof(T), null, null, parameters);
        }
    }
}