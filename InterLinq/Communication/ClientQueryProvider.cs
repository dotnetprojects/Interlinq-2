﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using InterLinq.Expressions;
using InterLinq.Types;

namespace InterLinq.Communication
{
    /// <summary>
    /// Client implementation of the <see cref="InterLinqQueryProvider"/>.
    /// </summary>
    /// <seealso cref="InterLinqQueryProvider"/>
    /// <seealso cref="IQueryProvider"/>
    public class ClientQueryProvider : InterLinqQueryProvider
    {

        #region Property Handler

        /// <summary>
        /// Gets the <see cref="IQueryRemoteHandler"/>;
        /// </summary>
        public IQueryRemoteHandler Handler { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="queryRemoteHandler"><see cref="IQueryRemoteHandler"/> to communicate with the server.</param>
        public ClientQueryProvider(IQueryRemoteHandler queryRemoteHandler)
        {
            if (queryRemoteHandler == null)
            {
                throw new ArgumentNullException("queryRemoteHandler");
            }
            Handler = queryRemoteHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the query and returns the requested data.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value.</typeparam>
        /// <param name="expression"><see cref="Expression"/> tree to execute.</param>
        /// <returns>Returns the requested data of Type 'TResult'.</returns>
        /// <seealso cref="InterLinqQueryProvider.Execute"/>
        public override TResult Execute<TResult>(Expression expression)
        {
            return (TResult)TypeConverter.ConvertFromSerializable(typeof(TResult), Execute(expression));
        }

        /// <summary>
        /// Executes the query and returns the requested data.
        /// </summary>
        /// <param name="expression"><see cref="Expression"/> tree to execute.</param>
        /// <returns>Returns the requested data of Type <see langword="object"/>.</returns>
        /// <seealso cref="InterLinqQueryProvider.Execute"/>
        public override object Execute(Expression expression)
        {
            var channel = Handler as IClientChannel;

            if (channel == null || channel.State != CommunicationState.Opened)
            {
                Handler = null;
                return null;
            }

            SerializableExpression serExp = expression.MakeSerializable();
#if !SILVERLIGHT           
            object receivedObject = Handler.Retrieve(serExp);            
#else
            IAsyncResult asyncResult = Handler.BeginRetrieve(serExp, null, null);
            object receivedObject = null;

            if (!asyncResult.CompletedSynchronously)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            try
            {
                receivedObject = Handler.EndRetrieve(asyncResult);
            }
            catch
            {
                throw;
            }
            finally
            {
#if !NETFX_CORE
                asyncResult.AsyncWaitHandle.Close();
#else
                asyncResult.AsyncWaitHandle.Dispose();
#endif
            }
#endif
                return receivedObject;
        }

        #endregion

    }
}
