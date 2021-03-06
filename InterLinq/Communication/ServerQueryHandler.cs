﻿using System;
using System.Linq;
using System.Reflection;
using InterLinq.Types;
using InterLinq.Expressions;
using System.Collections.Generic;

namespace InterLinq.Communication
{
    /// <summary>
    /// Server implementation of the <see cref="IQueryRemoteHandler"/>.
    /// </summary>
    /// <seealso cref="IQueryRemoteHandler"/>
    public class ServerQueryHandler : IQueryRemoteHandler, IDisposable
    {

        #region Properties

        /// <summary>
        /// Gets the <see cref="IQueryHandler"/>.
        /// </summary>
        public IQueryHandler QueryHandler { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="queryHandler"><see cref="IQueryHandler"/> instance.</param>
        public ServerQueryHandler(IQueryHandler queryHandler)
        {
            if (queryHandler == null)
            {
                throw new ArgumentNullException("queryHandler");
            }
            QueryHandler = queryHandler;
        }

        #endregion

        #region IQueryRemoteHandler Members

        /// <summary>
        /// Retrieves data from the server by an <see cref="SerializableExpression">Expression</see> tree.
        /// </summary>
        /// <remarks>
        /// This method's return type depends on the submitted 
        /// <see cref="SerializableExpression">Expression</see> tree.
        /// Here some examples ('T' is the requested type):
        /// <list type="list">
        ///     <listheader>
        ///         <term>Method</term>
        ///         <description>Return Type</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Select(...)</term>
        ///         <description>T[;</description>
        ///     </item>
        ///     <item>
        ///         <term>First(...), Last(...)</term>
        ///         <description>T</description>
        ///     </item>
        ///     <item>
        ///         <term>Count(...)</term>
        ///         <description><see langword="int"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Contains(...)</term>
        ///         <description><see langword="bool"/></description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <param name="expression">
        ///     <see cref="SerializableExpression">Expression</see> tree 
        ///     containing selection and projection.
        /// </param>
        /// <returns>Returns requested data.</returns>
        /// <seealso cref="IQueryRemoteHandler.Retrieve"/>
        public virtual object Retrieve(SerializableExpression expression)
        {
            try
            {
                MethodInfo mInfo;
                Type realType = (Type)expression.Type.GetClrVersion();
                if (typeof(IQueryable).IsAssignableFrom(realType) &&
                    realType.GetGenericArguments().Length == 1)
                {
                    // Find Generic Retrieve Method
                    mInfo = typeof(ServerQueryHandler).GetMethod("RetrieveGeneric");
                    mInfo = mInfo.MakeGenericMethod(realType.GetGenericArguments()[0]);
                }
                else
                {
                    // Find Non-Generic Retrieve Method
                    mInfo = typeof(ServerQueryHandler).GetMethod("RetrieveNonGenericObject");
                }

                object returnValue = mInfo.Invoke(this, new object[] { expression });

                return returnValue;
            }
            catch (Exception ex)
            {
                if (!HandleExceptionInRetrieve(ex, expression))
                    throw;
                else
                    return null;
            }
        }

        /// <summary>
        /// Retrieves data from the server by an <see cref="SerializableExpression">Expression</see> tree.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IQueryable"/>.</typeparam>
        /// <param name="serializableExpression">
        ///     <see cref="SerializableExpression">Expression</see> tree 
        ///     containing selection and projection.
        /// </param>
        /// <returns>Returns requested data.</returns>
        /// <seealso cref="IQueryRemoteHandler.Retrieve"/>
        /// <remarks>
        /// This method's return type depends on the submitted 
        /// <see cref="SerializableExpression">Expression</see> tree.
        /// Here some examples ('T' is the requested type):
        /// <list type="list">
        ///     <listheader>
        ///         <term>Method</term>
        ///         <description>Return Type</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Select(...)</term>
        ///         <description>T[;</description>
        ///     </item>
        ///     <item>
        ///         <term>First(...), Last(...)</term>
        ///         <description>T</description>
        ///     </item>
        ///     <item>
        ///         <term>Count(...)</term>
        ///         <description><see langword="int"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Contains(...)</term>
        ///         <description><see langword="bool"/></description>
        ///     </item>
        /// </list>
        /// </remarks>
        public virtual object RetrieveGeneric<T>(SerializableExpression serializableExpression)
        {
            object session = null;
            try
            {
                Type expressionQueryType = null;
                if (serializableExpression.Type != null)
                {
                    var ilt = serializableExpression.Type;
                    if (ilt.GenericArguments.Count > 0)
                    {
                        if (ilt.GenericArguments[0] != null)
                        {
                            ilt = ilt.GenericArguments[0];
                            expressionQueryType = ilt.RepresentedType;
                        }
                    }
                }
                session = QueryHandler.StartSession(expressionQueryType);
                IQueryable<T> query = serializableExpression.Convert(QueryHandler, session) as IQueryable<T>;
                if (query != null)
                {
                    var returnValue = query.ToArray();
                    object convertedReturnValue = TypeConverter.ConvertToSerializable(returnValue);
                    return convertedReturnValue;
                }
                return null;
            }
            catch
            {
                throw;
            }
            finally
            {
                QueryHandler.CloseSession(session);
            }
        }

        /// <summary>
        /// Retrieves data from the server by an <see cref="SerializableExpression">Expression</see> tree.
        /// </summary>
        /// <param name="serializableExpression">
        ///     <see cref="SerializableExpression">Expression</see> tree 
        ///     containing selection and projection.
        /// </param>
        /// <returns>Returns requested data.</returns>
        /// <remarks>
        /// This method's return type depends on the submitted 
        /// <see cref="SerializableExpression">Expression</see> tree.
        /// Here some examples ('T' is the requested type):
        /// <list type="list">
        ///     <listheader>
        ///         <term>Method</term>
        ///         <description>Return Type</description>
        ///     </listheader>
        ///     <item>
        ///         <term>Select(...)</term>
        ///         <description>T[;</description>
        ///     </item>
        ///     <item>
        ///         <term>First(...), Last(...)</term>
        ///         <description>T</description>
        ///     </item>
        ///     <item>
        ///         <term>Count(...)</term>
        ///         <description><see langword="int"/></description>
        ///     </item>
        ///     <item>
        ///         <term>Contains(...)</term>
        ///         <description><see langword="bool"/></description>
        ///     </item>
        /// </list>
        /// </remarks>
        /// <seealso cref="IQueryRemoteHandler.Retrieve"/>
        public object RetrieveNonGenericObject(SerializableExpression serializableExpression)
        {
            object session = null;
            try
            {
                Type expressionQueryType = null;
                if (serializableExpression.Type != null)
                {
                    var ilt = serializableExpression.Type;
                    if (ilt.GenericArguments.Count > 0)
                    {
                        if (ilt.GenericArguments[0] != null)
                        {
                            ilt = ilt.GenericArguments[0];
                            expressionQueryType = ilt.RepresentedType;
                        }
                    }
                }
                if (expressionQueryType == null && serializableExpression is SerializableMethodCallExpression)
                {
                    var m = (SerializableMethodCallExpression) serializableExpression;
                    if (m.Method.IsGeneric)
                        expressionQueryType = m.Method.GenericArguments[0].RepresentedType;
                }
                session = QueryHandler.StartSession(expressionQueryType);
                object returnValue = serializableExpression.Convert(QueryHandler, session);
                object convertedReturnValue = TypeConverter.ConvertToSerializable(returnValue);
                return convertedReturnValue;
            }
            catch
            {
                throw;
            }
            finally
            {
                QueryHandler.CloseSession(session);
            }
        }

        /// <summary>
        /// Handles an <see cref="Exception"/> occured in the 
        /// <see cref="IQueryRemoteHandler.Retrieve"/> Method.
        /// </summary>
        /// <param name="exception">
        /// Thrown <see cref="Exception"/> 
        /// in <see cref="IQueryRemoteHandler.Retrieve"/> Method.
        /// </param>
        protected virtual bool HandleExceptionInRetrieve(Exception exception, SerializableExpression expression)
        {
            return false;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes the server instance.
        /// </summary>
        public virtual void Dispose() { }

        #endregion
    }
}
