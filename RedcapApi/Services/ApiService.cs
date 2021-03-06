﻿using System;
using System.Threading.Tasks;
using Redcap.Broker;

namespace Redcap.Services
{
    public class ApiService : IApiService
    {
        private readonly IApiBroker apiBroker;

        public ApiService(IApiBroker apiBroker)
        {
            this.apiBroker = apiBroker;
        }

        /// <summary>
        /// Exports a single record from REDCap.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
#pragma warning disable 1998
        public async Task<T> ExportRecordAsync<T>()
#pragma warning restore 1998
        {
            throw new NotImplementedException();
        }
    }
}