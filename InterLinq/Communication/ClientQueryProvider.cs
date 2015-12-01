using System;
using System.Linq;
using System.Linq.Expressions;
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

		#region Properties

		public ClientQueryHandler.ExceptionOccuredHandler ExceptionOccured { get; set; }

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
            SerializableExpression serExp = expression.MakeSerializable();
#if !SILVERLIGHT
			try
            {
				return Handler.Retrieve(serExp); 
			}
			catch (Exception ex)
            {
	            if (ExceptionOccured != null)
					ExceptionOccured(ex);
                throw;
            }           
#else
			IAsyncResult asyncResult = Handler.BeginRetrieve(serExp, null, null);
            object receivedObject = null;

            if (!asyncResult.CompletedSynchronously)
            {
                asyncResult.AsyncWaitHandle.WaitOne();
            }

            try
            {
                return Handler.EndRetrieve(asyncResult);
            }
            catch (Exception ex)
            {
	            if (ExceptionOccured != null)
					ExceptionOccured(ex);
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
        }

        #endregion

    }
}
