﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Data;
using EPiServer.Data.Providers.Internal;
using EPiServer.Data.SchemaUpdates;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.KPI.SchemaUpdater
{
    [ExcludeFromCodeCoverage]
    [ServiceConfiguration(typeof(ISchemaUpdater))]
    public class DatabaseVersionValidator : ISchemaUpdater
    {
        private const long RequiredDatabaseVersion = 201604051658334;
        private const string Schema = "dbo";
        private const string ContextKey = "KPI.Migrations.Configuration";
        private const string UpdateDatabaseResource = "EPiServer.Marketing.KPI.SchemaUpdater.Kpi.zip";

        private readonly IDatabaseExecutor _databaseHandler;
        private readonly ScriptExecutor _scriptExecutor;
        private readonly IDatabaseConnectionResolver _databaseConnectionResolver;
        private readonly IKpiManager _kpiManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseVersionValidator"/> class.
        /// </summary>
        public DatabaseVersionValidator(IDatabaseExecutor databaseHandler, ScriptExecutor scriptExecutor, IDatabaseConnectionResolver databaseConnectionResolver, IKpiManager kpiManager)
        {
            _databaseHandler = databaseHandler;
            _scriptExecutor = scriptExecutor;
            _databaseConnectionResolver = databaseConnectionResolver;
            _kpiManager = kpiManager;
        }

        /// <inheritdoc/>
        public SchemaStatus GetStatus(IEnumerable<ConnectionStringOptions> connectionStringOptions)
        {
            long version = 0;

            version = _kpiManager.GetDatabaseVersion(Schema, ContextKey);

            if (version < RequiredDatabaseVersion)
            {
                // need to upgrade, versions can only be int, so we force it with fake versions based off our real veresions which are longs from EF
                return new SchemaStatus
                {
                    ConnectionStringOption = _databaseConnectionResolver.Resolve(),
                    ApplicationRequiredVersion = new Version(2, 0),
                    DatabaseVersion = new Version(1, 0)
                };
            }

            // don't need to upgrade
            return new SchemaStatus
            {
                ConnectionStringOption = _databaseConnectionResolver.Resolve(),
                ApplicationRequiredVersion = new Version(1, 0),
                DatabaseVersion = new Version(1, 0)
            };
        }
        

        public void Update(ConnectionStringOptions connectionStringOptions)
        {
            _scriptExecutor.OrderScriptsByVersion = true;
            _scriptExecutor.ExecuteEmbeddedZippedScripts(connectionStringOptions.ConnectionString, typeof(DatabaseVersionValidator).Assembly, UpdateDatabaseResource);

            IKpiManager kpiManager;
            ServiceLocator.Current.TryGetExistingInstance<IKpiManager>(out kpiManager);

            var version = kpiManager.GetDatabaseVersion(Schema, ContextKey, true);

            if (RequiredDatabaseVersion != version)
            {
                //something went wrong!
            }
        }
    }
}
