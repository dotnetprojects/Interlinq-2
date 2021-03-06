﻿using System;
using System.Linq;

namespace InterLinq.Communication
{
    /// <summary>
    /// Client implementation of the <see cref="InterLinqQueryHandler"/>.
    /// </summary>
    /// <seealso cref="InterLinqQueryHandler"/>
    /// <seealso cref="IQueryHandler"/>
    public class ClientQueryHandler : InterLinqQueryHandler
    {

        #region Properties

        public delegate void ExceptionOccuredHandler(Exception ex);

        public ExceptionOccuredHandler ExceptionOccured { get; set; }

        /// <summary>
        /// Gets the <see cref="IQueryProvider"/>.
        /// </summary>
        /// <seealso cref="InterLinqQueryHandler.QueryProvider"/>
        public override IQueryProvider QueryProvider
        {
            get { return new ClientQueryProvider(QueryRemoteHandler) { ExceptionOccured = ExceptionOccured }; }
        }

        /// <summary>
        /// <see cref="IQueryRemoteHandler"/> instance.
        /// </summary>
        protected IQueryRemoteHandler queryRemoteHandler;
        /// <summary>
        /// Gets the <see cref="IQueryRemoteHandler"/>;
        /// </summary>
        public virtual IQueryRemoteHandler QueryRemoteHandler
        {
            get
            {
                if (queryRemoteHandler == null)
                {
                    Connect();
                }
                return queryRemoteHandler;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public ClientQueryHandler() { }

        /// <summary>
        /// Initializes this class.
        /// </summary>
        /// <param name="queryRemoteHandler"><see cref="IQueryRemoteHandler"/> to communicate with the server.</param>
        public ClientQueryHandler(IQueryRemoteHandler queryRemoteHandler)
        {
            this.queryRemoteHandler = queryRemoteHandler;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Connects to the server.
        /// </summary>
        public virtual void Connect()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
