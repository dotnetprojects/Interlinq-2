using InterLinq;
using Sample.Data;
using System;
using System.Linq;

namespace Sample.Server
{
    class SampleQueryHandler : IQueryHandler
    {
        public bool CloseSession(object sessionObject)
        {
            // here, close linq2db session
            return true;
        }

        public IQueryable Get(Type type, object additionalObject, string queryName, object sessionObject, params object[] parameters)
        {
            //here query the session for the DTOs

            //atm sample data

            return (new[] {
                new Dto1() { Id = Guid.NewGuid(), Name = "bbaacc" },
                new Dto1() { Id = Guid.NewGuid(), Name = "dssaarer" },
                new Dto1() { Id = Guid.NewGuid(), Name = "hzfjh" },
                new Dto1() { Id = Guid.NewGuid(), Name = "hjfhfj" }
            }).AsQueryable();
        }

        public IQueryable<T> Get<T>(object additionalObject, string queryName, object sessionObject, params object[] parameters) where T : class
        {
            return (IQueryable<T>)Get(typeof(T), additionalObject, queryName, sessionObject, parameters);
        }

        public object StartSession(Type expressionQueryType)
        {
            // here, open linq2db session and return it
            return null;
        }
    }
}
